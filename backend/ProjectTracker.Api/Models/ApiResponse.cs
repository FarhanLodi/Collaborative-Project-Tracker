namespace ProjectTracker.Api.Models;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T? data, string message = "OK", int statusCode = 200)
        => new() { StatusCode = statusCode, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, int statusCode)
        => new() { StatusCode = statusCode, Message = message, Data = default };
}


