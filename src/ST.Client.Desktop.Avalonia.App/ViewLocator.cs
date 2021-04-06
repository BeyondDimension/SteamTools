using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using System.Application.UI.ViewModels;

namespace System.Application.UI
{
    public class ViewLocator : IDataTemplate
    {
#pragma warning disable CA1822 // 将成员标记为 static
        public bool SupportsRecycling => false;
#pragma warning restore CA1822 // 将成员标记为 static

        public IControl Build(object data)
        {
            var name = data.GetType().FullName?.Replace("ViewModel", "View");
            var type = name == null ? null : Type.GetType(name);
            if (type != null)
            {
                var view = (Control?)Activator.CreateInstance(type);
                if (view != null)
                {
                    return view;
                }
            }
            return new TextBlock { Text = "Not Found: " + name, Padding = Thickness.Parse("24, 4, 24, 24"), FontWeight = FontWeight.Bold, FontSize = 22, };
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}