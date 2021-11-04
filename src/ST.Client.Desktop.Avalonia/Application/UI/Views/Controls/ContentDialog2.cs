using Avalonia;
using System;
using System.Threading.Tasks;

namespace FluentAvalonia.UI.Controls
{
    public class ContentDialog2 : ContentDialog
    {
        public static readonly StyledProperty<bool> IsOpenProperty =
    AvaloniaProperty.Register<ContentDialog2, bool>(nameof(IsOpen), false);

        public ContentDialog2() : base()
        {
            this.GetObservable(IsOpenProperty)
              .Subscribe(async x =>
              {
                  if (x)
                      await ShowAsync();
                  else
                      Hide();
              });
        }

        public bool IsOpen
        {
            get
            {
                return GetValue(IsOpenProperty);
            }
            set
            {
                SetValue(IsOpenProperty, value);
            }
        }
    }
}
