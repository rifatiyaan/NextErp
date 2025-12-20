namespace NextErp.Application.DTOs.Common
{
    /// <summary>
    /// Generic response wrapper for all operations
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static OperationResult<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new OperationResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Errors = new()
            };
        }

        public static OperationResult<T> FailureResult(string message, List<string>? errors = null)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors ?? new()
            };
        }

        public static OperationResult<T> FailureResult(string message, string error)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = new List<string> { error }
            };
        }
    }
}
