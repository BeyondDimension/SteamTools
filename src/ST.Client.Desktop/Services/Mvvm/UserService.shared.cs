using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Properties;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public partial class UserService : Abstractions.UserService<UserService>
    {
        public UserService() { }
    }
}
