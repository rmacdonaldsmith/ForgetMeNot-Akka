using System;
using System.Runtime.Serialization;

namespace ForgetMeNot.Common
{
    public class SystemInitializationException : Exception
    {
        public SystemInitializationException()
        {
        }

        public SystemInitializationException(string message) : base(message)
        {
        }

        public SystemInitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SystemInitializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
