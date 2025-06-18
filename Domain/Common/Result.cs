namespace Domain.Common;

public class Result
{
    public bool IsFailure => !IsSuccess;
    public bool IsSuccess { get; private set; }
    public string Error { get; private set; }

    protected Result(bool success, string error = "")
    {
        IsSuccess = success;
        Error = error;
    }

    public static Result Success() =>
        new(true);

    public static Result<T> Success<T>(T value) =>
        new(value, true);

    public static Result Failure(string error) =>
        new(false, error);

    public static Result<T> Failure<T>(string error) =>
        new(default, false, error);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    protected internal Result(T? value, bool success, string error = "") : base(success, error)
    {
        Value = value;
    }
}