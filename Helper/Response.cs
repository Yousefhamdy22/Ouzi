using System.Net;

namespace WembyResturant.Helper
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        // Success responses
        public static Response<T> Ok(T data = default, string message = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new Response<T>
            {
                Success = true,
                Message = message ?? "Operation completed successfully",
                Data = data,
                StatusCode = statusCode
            };
        }

        public static Response<T> Created(T data = default, string message = null)
        {
            return Ok(data, message ?? "Resource created successfully", HttpStatusCode.Created);
        }

        // Failure responses
        public static Response<T> Fail(string message, IEnumerable<string> errors = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new Response<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        public static Response<T> NotFound(string message = "Resource not found")
        {
            return Fail(message, statusCode: HttpStatusCode.NotFound);
        }

        public static Response<T> Conflict(string message = "Conflict detected")
        {
            return Fail(message, statusCode: HttpStatusCode.Conflict);
        }

        public static Response<T> Unauthorized(string message = "Unauthorized access")
        {
            return Fail(message, statusCode: HttpStatusCode.Unauthorized);
        }

        public static Response<T> InternalError(string message = "An internal error occurred")
        {
            return Fail(message, statusCode: HttpStatusCode.InternalServerError);
        }
    }
}
