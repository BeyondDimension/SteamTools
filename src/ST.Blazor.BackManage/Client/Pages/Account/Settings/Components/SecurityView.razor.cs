using System.Application.Models;

namespace System.Application.Pages.Account.Settings
{
    public partial class SecurityView
    {
        private readonly UserLiteItem[] _data =
        {
            new UserLiteItem
            {
                Title = "Account Password",
                Description = "Current password strength: : Strong"
            },
            new UserLiteItem
            {
                Title = "Security Phone",
                Description = "Bound phone: : 138****8293"
            },
            new UserLiteItem
            {
                Title = "Security Question",
                Description =
                    "The security question is not set, and the security policy can effectively protect the account security"
            },
            new UserLiteItem
            {
                Title = "Backup Email",
                Description = "Bound Email: : ant***sign.com"
            },
            new UserLiteItem
            {
                Title = "MFA Device",
                Description = "Unbound MFA device, after binding, can be confirmed twice"
            }
        };
    }
}