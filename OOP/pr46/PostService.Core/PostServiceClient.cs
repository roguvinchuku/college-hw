using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PostService.Core;

public sealed class PostServiceClient : IPostServiceClient, IDisposable
{
    private readonly HttpClient httpClient;
    public PostServiceClient(string baseAddress = "http://localhost:5211", HttpMessageHandler? handler = null)
    {
        httpClient = handler is null ? new HttpClient() : new HttpClient(handler);
        httpClient.BaseAddress = new Uri(baseAddress);
        httpClient.Timeout = TimeSpan.FromSeconds(5);
    }
    public async Task<PostingGetDto[]> GetPostingsAsync()
    {
        using var response = await httpClient.GetAsync("/postings");
        await EnsureSuccessAsync(response);
        return await ReadAsync<PostingGetDto[]>(response);
    }
    public async Task<PostingGetDto> GetPostingByIdAsync(int id)
    {
        using var response = await httpClient.GetAsync($"/postings/{id}");
        await EnsureSuccessAsync(response);
        return await ReadAsync<PostingGetDto>(response);
    }
    public async Task<PostingGetDto> CreatePostingAsync(PostingPostDto posting)
    {
        using var response = await httpClient.PostAsJsonAsync("/postings", posting);
        await EnsureSuccessAsync(response);
        return await ReadAsync<PostingGetDto>(response);
    }
    public async Task<PostingGetDto> UpdatePostingAsync(int id, PostingPutDto posting)
    {
        using var response = await httpClient.PutAsJsonAsync($"/postings/{id}", posting);
        await EnsureSuccessAsync(response);
        return await ReadAsync<PostingGetDto>(response);
    }
    public async Task DeletePostingAsync(int id)
    {
        using var response = await httpClient.DeleteAsync($"/postings/{id}");
        await EnsureSuccessAsync(response);
    }
    private static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>() ?? throw new JsonException("Сервер повернув null замість очікуваних даних.");

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await ReadAsync<ValidationErrorResponse>(response);
            if (problem.Errors is null || problem.Errors.Count == 0)
                throw new JsonException("У відповіді 400 немає переліку помилок валідації.");
            throw new PostingValidationException(problem.Errors);
        }
        throw new HttpRequestException($"Сервер повернув помилку {(int)response.StatusCode}.", null, response.StatusCode);
    }
    public void Dispose() => httpClient.Dispose();
}
