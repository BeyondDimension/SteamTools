using System.Security.Principal;
using System.Windows.Forms;

var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
MessageBox.Show($"IsAdministrator: {isAdmin}",
    nameof(MessageBoxIcon.Information),
    MessageBoxButtons.OK,
    MessageBoxIcon.Information);