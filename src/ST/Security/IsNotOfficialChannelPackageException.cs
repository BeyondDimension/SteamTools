using System.Runtime.Serialization;

namespace System.Application.Security
{
    public sealed class IsNotOfficialChannelPackageException : Exception
    {
        public IsNotOfficialChannelPackageException()
        {
        }

        public IsNotOfficialChannelPackageException(string message) : base(message)
        {
        }

        public IsNotOfficialChannelPackageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IsNotOfficialChannelPackageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}