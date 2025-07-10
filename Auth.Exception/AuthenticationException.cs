
using System; // Ensure the System namespace is included to access the Exception class.

namespace Auth.Exception
{
    // Fix: Fully qualify the Exception type to avoid ambiguity with the namespace 'Auth.Exception'.
    public class AuthenticationException : System.Exception
    {
        public AuthenticationException() : base() { }

        public AuthenticationException(string message) : base(message) { }

        public AuthenticationException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
