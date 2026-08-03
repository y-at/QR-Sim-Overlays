using System;

namespace QRO.Services.AI
{
    public class AIChatException : Exception
    {
        public AIChatException(string message) : base(message) { }

        public AIChatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
