namespace NKSilk.Web.Models;

/// <summary>Standard REST envelope: <c>{ "data": ..., "error": null }</c>.</summary>
public class ApiResponse<T>
{
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Data = data };
    public static ApiResponse<T> Fail(string code, string message) => new() { Error = new ApiError(code, message) };
}

public record ApiError(string Code, string Message);
