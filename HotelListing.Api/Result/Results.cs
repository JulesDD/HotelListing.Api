using Microsoft.AspNetCore.Http.HttpResults;

namespace HotelListing.Api.Results;

// A struct uses less memory than a class and is more efficient for small data types.
// This will record the error code created during operations along with a description.
public readonly record struct ErrorResult(string Code, string Description)
{
    public static readonly  ErrorResult None = new("", "");
    public bool IsNone => string.IsNullOrWhiteSpace(Code);
}

// A struct to represent the result of an operation, indicating success or failure along with any associated errors.
//  Using 'record struct' for value-based equality and immutability.
// This struct is useful for methods that need to return multiple error details without throwing exceptions.
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public ErrorResult[] Errors { get; }

    private Result (bool isSuccess, ErrorResult[] errors)
        => (IsSuccess, Errors) = (isSuccess, errors);
    public static Result Success() => new(true, Array.Empty<ErrorResult>());
    public static Result Failure(params ErrorResult[] errors) => new(false, errors);
    public static Result BadRequest(params ErrorResult[] errors) => new(false, errors);
    public static Result NotFound(params ErrorResult[] errors) => new(false, errors);

    public static Result Combine(params Result[] results)
        => results.Any(r => !r.IsSuccess)
            ? Failure(results.Where(r => !r.IsSuccess).SelectMany(r => r.Errors).ToArray())
            : Success();
}
// Returns a result of an operation that can either be successful with a value of type T or failed with error details.
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorResult[] Errors { get; }
    private Result (bool isSuccess, T? value, ErrorResult[] errors)
        => (IsSuccess, Value, Errors) = (isSuccess, value, errors);

    
    // Factory methods to create success and failure results.
    public static Result<T> Success(T value) => new(true, value, Array.Empty<ErrorResult>());
    public static Result<T> Failure(params ErrorResult[] errors) => new(false, default, errors);
    public static Result<T> NotFound() => new( false, default, []);
    public static Result<T> BadRequest() => new(false, default, []);


    //helper method to combine multiple Result<T> instances into a single Result<T[]>.
    public Result<K> Bind<K>(Func<T,K> map)
        => IsSuccess
            ? Result<K>.Success(map(Value!))
            : Result<K>.Failure(Errors);

    public Result<K> Bind<K>(Func<T, Result<K>> next)
        => IsSuccess
            ? next(Value!)
            : Result<K>.Failure(Errors);

    public Result<T> Ensure(Func<T, bool> predicate, ErrorResult error)
        => IsSuccess && !predicate(Value!)
            ? Failure(error)
            : this;
}



