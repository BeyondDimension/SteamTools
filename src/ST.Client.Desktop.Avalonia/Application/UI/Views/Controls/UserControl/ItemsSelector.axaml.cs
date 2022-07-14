using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using System.Collections;

namespace System.Application.UI.Views.Controls
{
    public partial class ItemsSelector : UserControl
    {
        /// <summary>
        /// Defines the <see cref="Items"/> property.
        /// </summary>
        public static readonly DirectProperty<ItemsSelector, IEnumerable> ItemsProperty =
            AvaloniaProperty.RegisterDirect<ItemsSelector, IEnumerable>(nameof(Items), o => o.Items, (o, v) => o.Items = v);

        /// <summary>
        /// Defines the <see cref="ItemCount"/> property.
        /// </summary>
        public static readonly DirectProperty<ItemsControl, int> ItemCountProperty =
            AvaloniaProperty.RegisterDirect<ItemsControl, int>(nameof(ItemCount), o => o.ItemCount);

        /// <summary>
        /// Defines the <see cref="Items"/> property.
        /// </summary>
        public static readonly DirectProperty<ItemsSelector, IEnumerable> SelectItemsProperty =
            AvaloniaProperty.RegisterDirect<ItemsSelector, IEnumerable>(nameof(SelectItems), o => o.SelectItems, (o, v) => o.SelectItems = v);

        private IEnumerable _items = new AvaloniaList<object>();
        private IEnumerable _selectItems = new AvaloniaList<object>();
        private int _itemCount;

        /// <summary>
        /// Gets or sets the items to display.
        /// </summary>
        public IEnumerable SelectItems
        {
            get { return _selectItems; }
            set { SetAndRaise(SelectItemsProperty, ref _selectItems, value); }
        }

        /// <summary>
        /// Gets or sets the items to display.
        /// </summary>
        public IEnumerable Items
        {
            get { return _items; }
            set { SetAndRaise(ItemsProperty, ref _items, value); }
        }

        /// <summary>
        /// Gets the number of items in <see cref="Items"/>.
        /// </summary>
        public int ItemCount
        {
            get => _itemCount;
            private set => SetAndRaise(ItemCountProperty, ref _itemCount, value);
        }

        public ItemsSelector()
        {
            InitializeComponent();

            var items = this.FindControl<ItemsRepeater>("SelectItems");

            items[!ItemsRepeater.ItemsProperty] = this[!SelectItemsProperty];
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
