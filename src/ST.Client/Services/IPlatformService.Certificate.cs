using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        bool IsCertificateInstalled(X509Certificate2 certificate2)
        {
            return true;
        }
    }
}
