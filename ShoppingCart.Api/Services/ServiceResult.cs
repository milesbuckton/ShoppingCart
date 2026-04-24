namespace ShoppingCart.Api.Services;

public enum ServiceErrorType
{
    NotFound,
    Validation
}

public class ServiceResult<T>
{
    public T? Value { get; private init; }
    public bool IsSuccess { get; private init; }
    public string? Error { get; private init; }
    public ServiceErrorType? ErrorType { get; private init; }

    public static ServiceResult<T> Success(T value) => new() { Value = value, IsSuccess = true };
    public static ServiceResult<T> NotFound(string? error = null) => new() { IsSuccess = false, Error = error, ErrorType = ServiceErrorType.NotFound };
    public static ServiceResult<T> ValidationError(string error) => new() { IsSuccess = false, Error = error, ErrorType = ServiceErrorType.Validation };
}
