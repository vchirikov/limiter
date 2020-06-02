using System;
using System.Runtime.Serialization;

namespace Limiter.Extensions
{
    /// <inheritdoc cref="Exception"/>
    [Serializable]
    public class LimiterException : Exception
    {
        public LimiterException() { }

        public LimiterException(string message) : base(message) { }

        public LimiterException(string message, Exception innerException) : base(message, innerException) { }

        protected LimiterException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}