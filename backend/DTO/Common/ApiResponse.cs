namespace backend.DTO.Common;

public record ApiResponse<T>
{
    public bool Success { get; set; }
    public int Status {get;set;}
    public T? Data { get; set; }
    public string? Message { get; set; } 
    public List<string>? Errors { get; set; } 
}
