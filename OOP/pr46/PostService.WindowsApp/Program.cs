using System.Diagnostics;
using System.Reflection;
using PostService.Core;

namespace PostService.WindowsApp;
internal static class Program
{
    private static int handling;
    [STAThread]
    private static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Handle(e.ExceptionObject as Exception ?? new Exception("Невідома помилка."));
        // Винятки UI, зокрема async void, спрямовуються до UnhandledException.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        ApplicationConfiguration.Initialize();
        using var client = new PostServiceClient();
        Application.Run(new PostingList(client));
    }
    private static void Handle(Exception ex)
    {
        if (Interlocked.Exchange(ref handling, 1) != 0) return;
        while (ex is TargetInvocationException && ex.InnerException is not null) ex = ex.InnerException;
        string details = $"{DateTimeOffset.Now:O}{Environment.NewLine}{ex}{Environment.NewLine}";
        // Trace лишається активним і у Release; Debug — під час налагодження.
        Debug.WriteLine(details);
        Trace.WriteLine(details);
        string path = Path.Combine(AppContext.BaseDirectory, "errors.log");
        bool logged = false;
        try { File.AppendAllText(path, details); logged = true; }
        catch (Exception loggingError) { Trace.WriteLine(loggingError); }
        MessageBox.Show(logged
            ? "Сталася непередбачена помилка. Подробиці збережено в errors.log. Застосунок буде закрито."
            : "Сталася непередбачена помилка. Не вдалося зберегти журнал. Зверніться до розробника. Застосунок буде закрито.",
            "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        // UnhandledException є сповіщенням: після обробника процес завершується.
    }
}
