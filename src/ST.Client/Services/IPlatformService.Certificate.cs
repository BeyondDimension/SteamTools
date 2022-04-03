using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        bool IsCertificateInstalled(X509Certificate2 certificate2)
        {
            return true;
        }

        /// <summary>
        /// 删除证书
        /// </summary>
        void RemoveCertificate(X509Certificate2 certificate2)
        {
            throw new NotImplementedException();
        }
    }
}
