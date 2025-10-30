namespace LMS_Project.Domain.Common;
public class Result
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static Result Success(string message = "İşlem başarılı")
        => new() { IsSuccess = true, Message = message };

    public static Result Failure(string error)
        => new() { IsSuccess = false, Errors = new List<string> { error } };

    public static Result Failure(List<string> errors)
        => new() { IsSuccess = false, Errors = errors };
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Success(T data, string message = "İşlem başarılı")
        => new() { IsSuccess = true, Data = data, Message = message };

    public new static Result<T> Failure(string error)
        => new() { IsSuccess = false, Errors = new List<string> { error } };

    public new static Result<T> Failure(List<string> errors)
        => new() { IsSuccess = false, Errors = errors };
}


public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResult<T> Create(List<T> items, int count, int pageNumber, int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
