using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Titanium.Web.Proxy.Examples.Wpf
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private bool notificationSuppressed;
        private bool suppressNotification;

        public bool SuppressNotification
        {
            get => suppressNotification;
            set
            {
                suppressNotification = value;
                if (suppressNotification == false && notificationSuppressed)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    notificationSuppressed = false;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SuppressNotification)
            {
                notificationSuppressed = true;
                return;
            }

            base.OnCollectionChanged(e);
        }
    }
}