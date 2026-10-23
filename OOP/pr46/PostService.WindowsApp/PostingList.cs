using System.Net;
using PostService.Core;

namespace PostService.WindowsApp;
public class PostingList : Form
{
    private readonly IPostServiceClient client;
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AllowUserToAddRows = false };
    private readonly FlowLayoutPanel buttons = new() { Dock = DockStyle.Top, Height = 50, Padding = new Padding(5) };
    public PostingList(IPostServiceClient client)
    {
        this.client = client;
        Text = "Поштовий сервіс — відправлення"; ClientSize = new Size(850, 480);
        Font = new Font("Segoe UI", 11); StartPosition = FormStartPosition.CenterScreen;
        foreach (var column in new[] { ("Id", "Id"), ("From", "Звідки"), ("To", "Куди"), ("Weight", "Вага, кг") })
            grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = column.Item1, HeaderText = column.Item2, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        AddButton("Оновити", async () => { await Reload(); });
        AddButton("Створити", async () => { using var editor = new PostingEditor(client, null); if (editor.ShowDialog(this) == DialogResult.OK) await Reload(); });
        AddButton("Редагувати", Edit);
        AddButton("Видалити", Delete);
        Controls.Add(grid); Controls.Add(buttons);
        Shown += async (_, _) => await RunBusy(async () => { await Reload(); });
    }
    private PostingGetDto? Selected => grid.CurrentRow?.DataBoundItem as PostingGetDto;
    private void AddButton(string title, Func<Task> action)
    {
        var button = new Button { Text = title, AutoSize = true };
        button.Click += async (_, _) => await RunBusy(action);
        buttons.Controls.Add(button);
    }
    private async Task RunBusy(Func<Task> action)
    {
        buttons.Enabled = false;
        try { await action(); }
        finally { buttons.Enabled = true; }
    }
    private Task<bool> Reload() => RetryOperation.RunAsync(this, async () => grid.DataSource = await client.GetPostingsAsync());
    private async Task Edit()
    {
        if (Selected is not { } selected) return;
        PostingGetDto? posting = null;
        try
        {
            if (!await RetryOperation.RunAsync(this, async () => posting = await client.GetPostingByIdAsync(selected.Id))) return;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            MessageBox.Show(this, "Відправлення вже відсутнє. Список буде оновлено."); await Reload(); return;
        }
        using var editor = new PostingEditor(client, posting);
        if (editor.ShowDialog(this) == DialogResult.OK) await Reload();
    }
    private async Task Delete()
    {
        if (Selected is not { } selected) return;
        if (MessageBox.Show(this, "Видалити вибране відправлення?", "Підтвердження", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        try
        {
            if (!await RetryOperation.RunAsync(this, () => client.DeletePostingAsync(selected.Id))) return;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        { MessageBox.Show(this, "Відправлення вже відсутнє. Список буде оновлено."); }
        await Reload();
    }
}
