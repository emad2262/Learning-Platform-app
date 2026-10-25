namespace Learning_Platform_app.Common.Result_pattern
{
    public sealed class RequestResponse<T>
    {
        private RequestResponse(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public bool IsSuccess { get; }
        public T? Data { get; }
        public string? Error { get; }

        public static RequestResponse<T> Success(T data) => new(true, data, null);

        public static RequestResponse<T> Failure(string error)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(error);
            return new(false, default, error);
        }
    }
}
