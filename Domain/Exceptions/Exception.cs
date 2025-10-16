using Microsoft.AspNetCore.Http;
using System.Data.Common;
using System.Net.Sockets;

namespace Domain.Exceptions
{
    public abstract class CustomException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        protected CustomException(string message, int statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    public class NotFoundException : CustomException
    {
        public NotFoundException(string entityName, object key)
            : base($"The {entityName} with id {key} was not found.",
                  StatusCodes.Status404NotFound,
                  "not_found")
        {
        }
    }

    public class ValidationException : CustomException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Validation errors occurred",
                  StatusCodes.Status400BadRequest,
                  "validation_error")
        {
            Errors = errors;
        }
    }

    public class ConflictException : CustomException
    {
        public ConflictException(string message)
            : base(message,
                  StatusCodes.Status409Conflict,
                  "conflict")
        {
        }
    }



    public class ServiceUnavailableException : Exception
    {
        public string ServiceName { get; }
        public bool IsTransient { get; }

        public ServiceUnavailableException(string serviceName)
            : base($"Service '{serviceName}' is unavailable")
        {
            ServiceName = serviceName;
            IsTransient = true;
        }

        public ServiceUnavailableException(string serviceName, Exception innerException)
            : base($"Service '{serviceName}' is unavailable", innerException)
        {
            ServiceName = serviceName;
            IsTransient = DetermineTransientStatus(innerException);
        }

        private bool DetermineTransientStatus(Exception ex)
        {
            return ex is DbException || ex is SocketException;
        }
    }
}
