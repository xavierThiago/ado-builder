using System;
using System.Runtime.Serialization;

namespace AdoBuilder.Core
{
    public class AdoBuilderException : Exception
    {
        public AdoBuilderException()
        { }

        public AdoBuilderException(string message)
            : base(message)
        { }

        public AdoBuilderException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected AdoBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
