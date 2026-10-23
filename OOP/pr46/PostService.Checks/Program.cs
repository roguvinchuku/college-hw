using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PostService.Core;

Console.OutputEncoding = Encoding.UTF8;
using var client = new PostServiceClient();
if (args.Contains("--offline"))
{
    await Expect<HttpRequestException>(async () => await client.GetPostingsAsync(), ex => ex.StatusCode is null);
    Console.WriteLine("PASS: вимкнений сервер — HttpRequestException, StatusCode=null.");
    return;
}
using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5211") };
Check((await client.GetPostingsAsync()).Length == 0, "Початковий список");
await Expect<PostingValidationException>(async () => await client.CreatePostingAsync(new()), ex => ex.Errors.ContainsKey("From") && ex.Errors.ContainsKey("To") && ex.Errors.ContainsKey("Weight"));
await Expect<HttpRequestException>(async () => await client.GetPostingByIdAsync(999999), ex => ex.StatusCode == HttpStatusCode.NotFound);
var created = await client.CreatePostingAsync(new() { From = "Київ", To = "Львів", Weight = 2.5m });
Check(created.Id > 0 && (await client.GetPostingByIdAsync(created.Id)).Weight == 2.5m, "Створення та читання");
await Expect<PostingValidationException>(async () => await client.UpdatePostingAsync(created.Id, new() { From = "", To = "Львів", Weight = 2.5m }), ex => ex.Errors.ContainsKey("From"));
Check((await client.GetPostingByIdAsync(created.Id)).From == "Київ", "Помилка валідації не змінює дані");
var updated = await client.UpdatePostingAsync(created.Id, new() { From = "Одеса", To = "Львів", Weight = 3 });
Check(updated.From == "Одеса" && updated.Weight == 3, "Оновлення");
await client.DeletePostingAsync(created.Id);
await Expect<HttpRequestException>(() => client.DeletePostingAsync(created.Id), ex => ex.StatusCode == HttpStatusCode.NotFound);
await Expect<HttpRequestException>(async () => await client.UpdatePostingAsync(created.Id, new() { From = "Київ", To = "Львів", Weight = 1 }), ex => ex.StatusCode == HttpStatusCode.NotFound);
try
{
    (await http.PostAsync("/demo/failure/true", null)).EnsureSuccessStatusCode();
    await Expect<HttpRequestException>(async () => await client.GetPostingsAsync(), ex => ex.StatusCode == HttpStatusCode.InternalServerError);
}
finally { (await http.PostAsync("/demo/failure/false", null)).EnsureSuccessStatusCode(); }
using (var malformed = new PostServiceClient(handler: new ResponseHandler("not-json")))
    await Expect<JsonException>(async () => await malformed.GetPostingsAsync(), _ => true);
using (var nullBody = new PostServiceClient(handler: new ResponseHandler("null")))
    await Expect<JsonException>(async () => await nullBody.GetPostingsAsync(), _ => true);
using (var invalid400 = new PostServiceClient(handler: new ResponseHandler("{}", HttpStatusCode.BadRequest)))
    await Expect<JsonException>(async () => await invalid400.GetPostingsAsync(), _ => true);
Console.WriteLine("PASS: п'ять методів клієнта, CRUD, 400 з errors, 404 після видалення, реальний виняток сервера 500, некоректний JSON.");

static void Check(bool condition, string name) { if (!condition) throw new Exception(name); }
static async Task Expect<T>(Func<Task> action, Func<T, bool> predicate) where T : Exception
{
    try { await action(); } catch (T ex) when (predicate(ex)) { return; }
    throw new Exception($"Очікувався {typeof(T).Name} з відповідними даними.");
}
class ResponseHandler(string body, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
}
