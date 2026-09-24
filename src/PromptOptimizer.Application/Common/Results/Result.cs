namespace PromptOptimizer.Application.Common.Results;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, string error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? (string.IsNullOrEmpty(error) ? new List<string>() : new List<string> { error });
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(List<string> errors) => new(false, errors.FirstOrDefault() ?? "Multiple errors occurred", errors);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(List<string> errors) => Result<T>.Failure(errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, T? value, string error, List<string>? errors = null)
        : base(isSuccess, error, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public new static Result<T> Failure(string error) => new(false, default, error);
    public new static Result<T> Failure(List<string> errors) => new(false, default, errors.FirstOrDefault() ?? "Error", errors);
}
