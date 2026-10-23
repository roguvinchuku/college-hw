using System.Net;
using PostService.Core;

namespace PostService.WindowsApp;
public class PostingEditor : Form
{
    private readonly IPostServiceClient client;
    private readonly int? id;
    private readonly TextBox from = new() { Dock = DockStyle.Fill };
    private readonly TextBox to = new() { Dock = DockStyle.Fill };
    private readonly NumericUpDown weight = new() { DecimalPlaces = 2, Maximum = 10000, Dock = DockStyle.Fill };
    private readonly Button save = new() { Text = "Зберегти", AutoSize = true };
    public PostingEditor(IPostServiceClient client, PostingGetDto? posting)
    {
        this.client = client; id = posting?.Id;
        Text = id is null ? "Нове відправлення" : "Редагування відправлення";
        ClientSize = new Size(520, 235); Font = new Font("Segoe UI", 11);
        StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        string[] labels = ["Звідки", "Куди", "Вага, кг"];
        Control[] inputs = [from, to, weight];
        for (int i = 0; i < inputs.Length; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.Controls.Add(new Label { Text = labels[i], AutoSize = true }, 0, i); layout.Controls.Add(inputs[i], 1, i);
        }
        layout.Controls.Add(save, 1, 3); Controls.Add(layout);
        if (posting is not null) { from.Text = posting.From; to.Text = posting.To; weight.Value = Math.Clamp(posting.Weight, 0, weight.Maximum); }
        save.Click += async (_, _) => await Save();
        AcceptButton = save;
    }
    private async Task Save()
    {
        save.Enabled = false;
        try
        {
            bool success = await RetryOperation.RunAsync(this, async () =>
            {
                if (id is int existingId)
                    await client.UpdatePostingAsync(existingId, new PostingPutDto { From = from.Text, To = to.Text, Weight = weight.Value });
                else
                    await client.CreatePostingAsync(new PostingPostDto { From = from.Text, To = to.Text, Weight = weight.Value });
            });
            if (success) DialogResult = DialogResult.OK;
        }
        catch (PostingValidationException ex)
        {
            var messages = new List<string>();
            foreach (var field in ex.Errors)
                foreach (string message in field.Value) messages.Add($"{field.Key}: {message}");
            MessageBox.Show(this, string.Join(Environment.NewLine, messages), "Перевірте введені дані", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            // DialogResult не змінюється: введені дані збережені, форма відкрита.
        }
        catch (HttpRequestException ex) when (id is not null && ex.StatusCode == HttpStatusCode.NotFound)
        {
            MessageBox.Show(this, "Відправлення вже видалено. Список буде оновлено.");
            DialogResult = DialogResult.OK; // Батьківська форма оновить список.
        }
        finally { save.Enabled = true; }
    }
}
