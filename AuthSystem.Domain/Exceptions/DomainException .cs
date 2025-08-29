namespace AuthSystem.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UserNotFoundException : DomainException
    {
        public UserNotFoundException(string identifier)
            : base($"User with identifier '{identifier}' was not found") { }
    }

    public class InvalidCredentialsException : DomainException
    {
        public InvalidCredentialsException()
            : base("Invalid email or password") { }
    }

    public class UserAlreadyExistsException : DomainException
    {
        public UserAlreadyExistsException(string field, string value)
            : base($"User with {field} '{value}' already exists") { }
    }

    public class InactiveUserException : DomainException
    {
        public InactiveUserException()
            : base("User account is inactive") { }
    }
}