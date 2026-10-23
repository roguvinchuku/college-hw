namespace PostService.Core;

public class PostingPostDto
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public decimal Weight { get; set; }
}
public class PostingPutDto : PostingPostDto { }
public class PostingGetDto : PostingPostDto
{
    public int Id { get; set; }
}
public class ValidationErrorResponse
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
}
public class PostingValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    public PostingValidationException(Dictionary<string, string[]> errors)
        : base("Сервер відхилив дані відправлення.") => Errors = errors;
}
public interface IPostServiceClient
{
    Task<PostingGetDto[]> GetPostingsAsync();
    Task<PostingGetDto> GetPostingByIdAsync(int id);
    Task<PostingGetDto> CreatePostingAsync(PostingPostDto posting);
    Task<PostingGetDto> UpdatePostingAsync(int id, PostingPutDto posting);
    Task DeletePostingAsync(int id);
}
