using Android.Content;
using Android.Views;
using Binding;
using Google.Android.Material.Dialog;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    internal sealed class ShowWindowServiceImpl : IShowWindowService
    {
        static string PositiveButtonText => AppResources.Confirm;
        static string NegativeButtonText => AppResources.Cancel;

        Task<bool> PlatformShowWindow(CustomWindow customWindow, PageViewModel? viewModel = null, string title = "")
        {
            TaskCompletionSource<bool> tcs = new();
            try
            {
                AlertDialog? dialog = null;

                void SetConfirmButton(MaterialAlertDialogBuilder b)
                {
                    b.SetPositiveButton(PositiveButtonText, (_, e) =>
                    {
                        tcs.TrySetResult(true);
                    });
                }
                void SetCancelButton(MaterialAlertDialogBuilder b)
                {
                    b.SetNegativeButton(NegativeButtonText, (_, e) =>
                    {
                        tcs.TrySetResult(false);
                    });
                }

                Action<AlertDialog>? CreateMessageBoxDialogWindow(MaterialAlertDialogBuilder b)
                {
                    SetConfirmButton(b);
                    SetCancelButton(b);
                    if (viewModel is MessageBoxWindowViewModel viewModel_mb)
                    {
                        b.SetMessage(viewModel_mb.Content);
                    }
                    return null;
                }
                Action<AlertDialog>? CreatePasswordDialogWindow(MaterialAlertDialogBuilder b)
                {
                    // https://material.io/components/text-fields/android
                    // https://material.io/components/dialogs#specs
                    IDialogInterfaceOnClickListener? listener = null;
                    b.SetPositiveButton(PositiveButtonText, listener);
                    SetCancelButton(b);
                    var view = LayoutInflater.From(b.Context)!.Inflate(Resource.Layout.textbox_password, null, false)!;
                    var binding = new textbox_password(view);
                    binding.layoutPassword.Hint = AppResources.LocalAuth_PasswordRequired1;
                    b.SetView(view);
                    return d =>
                    {
                        d.GetButton((int)DialogButtonType.Positive).Click += (_, _) =>
                        {
                            if (viewModel is PasswordWindowViewModel viewModel_p)
                            {
                                viewModel_p.Password = binding.tbPassword.Text;
                                if (viewModel_p.InputValidator())
                                {
                                    d.Dismiss();
                                    tcs.TrySetResult(true);
                                }
                            }
                        };
                    };
                }

                Func<MaterialAlertDialogBuilder, Action<AlertDialog>?> @delegate = customWindow switch
                {
                    CustomWindow.MessageBox => CreateMessageBoxDialogWindow,
                    CustomWindow.Password => CreatePasswordDialogWindow,
                    _ => throw new NotImplementedException(),
                };
                if (!string.IsNullOrEmpty(title) && viewModel != null)
                {
                    viewModel.Title = title;
                }
                var builder = new MaterialAlertDialogBuilder(XEPlatform.CurrentActivity);
                if (!string.IsNullOrWhiteSpace(viewModel?.Title))
                {
                    builder.SetTitle(viewModel!.Title);
                }
                var dialog_delegate = @delegate(builder);
                dialog = builder.Create();
                dialog.CancelEvent += (_, _) =>
                {
                    tcs.TrySetResult(false);
                };
                if (dialog_delegate != null)
                {
                    dialog.ShowEvent += (_, _) =>
                    {
                        dialog_delegate?.Invoke(dialog);
                    };
                }
                dialog.Show();
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
            return tcs.Task;
        }

        public async Task Show<TWindowViewModel>(CustomWindow customWindow, TWindowViewModel? viewModel = null, string title = "", ResizeModeCompat resizeMode = default, bool isDialog = false, bool isParent = true) where TWindowViewModel : PageViewModel, new()
        {
            await PlatformShowWindow(customWindow, viewModel, title);
        }

        public async Task Show(Type typeWindowViewModel, CustomWindow customWindow, PageViewModel? viewModel = null, string title = "", ResizeModeCompat resizeMode = default, bool isDialog = false, bool isParent = true)
        {
            await PlatformShowWindow(customWindow, viewModel, title);
        }

        public async Task<bool> ShowDialog<TWindowViewModel>(CustomWindow customWindow, TWindowViewModel? viewModel = null, string title = "", ResizeModeCompat resizeMode = default, bool isDialog = true) where TWindowViewModel : PageViewModel, new()
        {
            return await PlatformShowWindow(customWindow, viewModel, title);
        }

        public async Task ShowDialog(Type typeWindowViewModel, CustomWindow customWindow, PageViewModel? viewModel = null, string title = "", ResizeModeCompat resizeMode = default, bool isDialog = true)
        {
            await PlatformShowWindow(customWindow, viewModel, title);
        }
    }
}