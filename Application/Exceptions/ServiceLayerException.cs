

namespace Application.Exceptions
{
    public class ServiceLayerException : Exception
    {
       
        public string Title { get; } = "Service Error";

        public ServiceLayerException(string message) : base(message)
        {


        }

        public ServiceLayerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public ServiceLayerException(string title, string message) : base(message)
        {
            Title = title;
        }
    }

    // More specific exceptions derive from it (Highly Recommended)
    public class NotFoundException : ServiceLayerException
    {
        public NotFoundException(string name, object key)
            : base("Not Found", $"Entity '{name}' ({key}) was not found.")
        {
        }
    }

    public class DuplicateException : ServiceLayerException
    {
        public DuplicateException(string name, object key)
            : base("Conflict", $"Entity '{name}' with value '{key}' already exists.")
        {
        }
    }

    public class ValidationException : ServiceLayerException
    {
        public ValidationException()
            : base("Validation Error", "One or more validation failures have occurred.")
        {
        }
    }
}