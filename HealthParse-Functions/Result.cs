namespace HealthParseFunctions
{
    public static class Result
    {
        public static Result<T> Success<T>(T value)
        {
            return new Result<T>(value, true);
        }

        public static Result<T> Failure<T>(T value)
        {
            return new Result<T>(value, false);
        }
    }
    public class Result<T>
    {
        public T Value { get; }
        public bool WasSuccessful { get; }


        public Result(T value, bool wasSuccessful)
        {
            Value = value;
            WasSuccessful = wasSuccessful;
        }
    }
}
