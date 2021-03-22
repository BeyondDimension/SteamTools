using System.Security.Cryptography;

namespace System
{
    public static class HashAlgorithmEx
    {
        public static void Dispose(this HashAlgorithm hashAlgorithm)
        {
            IDisposable disposable = hashAlgorithm;
            disposable.Dispose();
        }
    }
}