using PostService.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["urls"] ?? "http://localhost:5211");
var app = builder.Build();
app.MapGet("/", () => Results.Redirect("/postings"));
app.MapGet("/health", () => Results.Ok(new { service = "PostService.Pr46", status = "ready" }));
var sync = new object();
var postings = new Dictionary<int, PostingGetDto>();
int nextId = 1;
// Навчальний режим помилки керується лише локальними REST-запитами.
bool fail = false;
app.MapPost("/demo/failure/{enabled:bool}", (bool enabled) => { lock (sync) fail = enabled; return Results.NoContent(); });
app.MapGet("/postings", () =>
{
    lock (sync)
    {
        if (fail) throw new InvalidOperationException("Навчальна помилка ендпойнта GET /postings.");
        return Results.Ok(postings.Values.ToArray());
    }
});
app.MapGet("/postings/{id:int}", (int id) =>
{
    lock (sync) return postings.TryGetValue(id, out var posting) ? Results.Ok(posting) : Results.NotFound();
});
app.MapPost("/postings", (PostingPostDto dto) =>
{
    var errors = Validate(dto);
    if (errors.Count > 0) return Results.ValidationProblem(errors);
    lock (sync)
    {
        var posting = new PostingGetDto { Id = nextId++, From = dto.From, To = dto.To, Weight = dto.Weight };
        postings.Add(posting.Id, posting);
        return Results.Created($"/postings/{posting.Id}", posting);
    }
});
app.MapPut("/postings/{id:int}", (int id, PostingPutDto dto) =>
{
    lock (sync)
    {
        if (!postings.ContainsKey(id)) return Results.NotFound();
        var errors = Validate(dto);
        if (errors.Count > 0) return Results.ValidationProblem(errors);
        var posting = new PostingGetDto { Id = id, From = dto.From, To = dto.To, Weight = dto.Weight };
        postings[id] = posting;
        return Results.Ok(posting);
    }
});
app.MapDelete("/postings/{id:int}", (int id) =>
{
    lock (sync) return postings.Remove(id) ? Results.NoContent() : Results.NotFound();
});
app.Run();

static Dictionary<string, string[]> Validate(PostingPostDto dto)
{
    var errors = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(dto.From)) errors["From"] = ["Пункт відправлення обов'язковий."];
    if (string.IsNullOrWhiteSpace(dto.To)) errors["To"] = ["Пункт призначення обов'язковий."];
    if (dto.Weight <= 0) errors["Weight"] = ["Вага повинна бути більшою за нуль."];
    return errors;
}

