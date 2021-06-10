//using Android.Content;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.Android;
//using ARecyclerView = AndroidX.RecyclerView.Widget.RecyclerView;
//using XFRecyclerView = System.Application.UI.Views.Controls.RecyclerView;

//namespace System.Application.UI.Views.Controls
//{
//    partial class RecyclerViewRenderer : ViewRenderer<XFRecyclerView, ARecyclerView>
//    {
//        TableViewModelRenderer _adapter;
//        bool _disposed;

//        public RecyclerViewRenderer(Context context) : base(context)
//        {
//            AutoPackage = false;
//        }

//        protected virtual TableViewModelRenderer GetModelRenderer(ARecyclerView listView, XFRecyclerView view)
//        {
//            return new TableViewModelRenderer(Context, listView, view);
//        }

//        protected override Size MinimumSize()
//        {
//            return new Size(20, 20);
//        }

//        protected override ARecyclerView CreateNativeControl()
//        {
//            return new ARecyclerView(Context);
//        }

//        protected override void OnElementChanged(ElementChangedEventArgs<XFRecyclerView> e)
//        {
//            base.OnElementChanged(e);

//            var listView = Control;
//            if (listView == null)
//            {
//                listView = CreateNativeControl();
//                SetNativeControl(listView);
//            }

//            listView.Focusable = false;
//            listView.DescendantFocusability = DescendantFocusability.AfterDescendants;

//            var view = e.NewElement;

//            _adapter = GetModelRenderer(listView, view);
//            listView.Adapter = _adapter;
//        }

//        protected override void OnAttachedToWindow()
//        {
//            base.OnAttachedToWindow();

//            if (Forms.IsLollipopOrNewer && Control != null)
//                Control.NestedScrollingEnabled = (Parent.GetParentOfType<NestedScrollView>() != null);
//        }

//        class CellAdapter : ARecyclerView.Adapter
//        {
//            readonly Context context;

//            public CellAdapter(Context context)
//            {
//                this.context = context;
//            }
//        }

//        class TableViewModelRenderer : CellAdapter
//        {
//            public TableViewModelRenderer(Context context, ARecyclerView listView, XFRecyclerView view) : base(context)
//            {
//            }
//        }
//    }
//}