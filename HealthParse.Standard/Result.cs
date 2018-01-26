using System;

namespace HealthParse.Standard
{
    public static class Result
    {
        public static Result<T> Success<T>(T value)
        {
            return new Result<T>(value, true);
        }

        public static Result<T> Failure<T>(T value, Exception exception = null)
        {
            return new Result<T>(value, false){Exception = exception};
        }
    }
    public class Result<T>
    {
        public T Value { get; }
        public bool WasSuccessful { get; }
        public Exception Exception { get; set; }


        public Result(T value, bool wasSuccessful)
        {
            Value = value;
            WasSuccessful = wasSuccessful;
        }
    }
}
