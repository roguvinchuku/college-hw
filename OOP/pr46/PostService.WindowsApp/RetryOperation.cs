using System.Net;

namespace PostService.WindowsApp;
internal static class RetryOperation
{
    public static async Task<bool> RunAsync(IWin32Window owner, Func<Task> action)
    {
        while (true)
        {
            try { await action(); return true; }
            catch (HttpRequestException ex) when (ex.StatusCode is null || ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                if (!Retry(owner, "Сервер недоступний. Повторити операцію?")) return false;
            }
            catch (TaskCanceledException)
            {
                if (!Retry(owner, "Час очікування відповіді вичерпано. Повторити операцію?")) return false;
            }
        }
    }
    private static bool Retry(IWin32Window owner, string text) =>
        MessageBox.Show(owner, text, "З'єднання із сервером", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry;
}
