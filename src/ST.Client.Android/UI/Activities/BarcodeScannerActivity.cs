using Android.App;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Core.Content;
using Binding;
using Google.Common.Util.Concurrent;
using Java.Lang;
using Java.Util.Concurrent;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;
using AUri = Android.Net.Uri;
using Fragment = AndroidX.Fragment.App.Fragment;
using GmsTask = Android.Gms.Tasks.Task;
using JException = Java.Lang.Exception;
using JObject = Java.Lang.Object;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(BarcodeScannerActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class BarcodeScannerActivity : BaseActivity<activity_barcode_scanner>, ViewTreeObserver.IOnGlobalLayoutListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_barcode_scanner;

        ICamera? camera;
        IListenableFuture? cameraProviderFuture;
        QRCodeAnalyser? analyser;

        protected override async void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                Title = AppResources.ScanQRCode;
            }).AddTo(this);

            binding.previewView.ViewTreeObserver!.AddOnGlobalLayoutListener(this);
            cameraProviderFuture = ProcessCameraProvider.GetInstance(this);

            var status = await IPermissions.Instance.CheckAndRequestAsync<Permissions.Camera>();
            if (!status.IsOk())
            {
                Finish();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ScanCompletedHandler = null;
        }

        public override bool OnCreateOptionsMenu(IMenu? menu)
        {
            MenuInflater.Inflate(Resource.Menu.barcode_scanner_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_gallery)
            {
                AnalyzeByGallery();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        async void AnalyzeByGallery()
        {
            if (analyser == null) return;
            //var fileResult = await FilePicker.PickAsync(PickOptions.Images);
            var fileResult = await MediaPicker.PickPhotoAsync();
            if (fileResult != null)
            {
                analyser.Analyze(fileResult.FullPath);
            }
        }

        void ViewTreeObserver.IOnGlobalLayoutListener.OnGlobalLayout()
        {
            binding!.previewView.ViewTreeObserver!.RemoveOnGlobalLayoutListener(this);
            cameraProviderFuture!.AddListener(new Runnable(() =>
            {
                var cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get()!;
                BindScan(cameraProvider, binding!.previewView.Width, binding!.previewView.Height);
            }), ContextCompat.GetMainExecutor(this));
        }

        static Action<IEnumerable<byte[]>>? ScanCompletedHandler;

        void BindScan(ProcessCameraProvider cameraProvider, int width, int height)
        {
            var preview = new Preview.Builder().Build();
            // 绑定预览
            preview.SetSurfaceProvider(binding!.previewView.SurfaceProvider);
            // 使用后置相机
            var cameraSelector = new CameraSelector.Builder()
                .RequireLensFacing(CameraSelector.LensFacingBack)
                .Build();
            // 绑定图片扫描解析
            var imageAnalysis = new ImageAnalysis.Builder()
                .SetTargetResolution(new(width, height))
                .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                .Build();
            // 绑定图片扫描解析
            analyser = new((barCodes, imageWidth, imageHeight) =>
            {
                // 解绑当前所有相机操作
                cameraProvider.UnbindAll();
                ScanCompletedHandler?.Invoke(barCodes.Select(x => x.GetRawBytes()));
                ScanCompletedHandler = null;
                Finish();
            });
            imageAnalysis.SetAnalyzer(Executors.NewSingleThreadExecutor(), analyser);
            camera = cameraProvider.BindToLifecycle(this, cameraSelector, imageAnalysis, preview);
        }

        sealed class QRCodeAnalyser : JObject, ImageAnalysis.IAnalyzer, IOnSuccessListener, IOnFailureListener, IOnCompleteListener
        {
            IImageProxy? imageProxy;
            readonly IBarcodeScanner detector;
            readonly Action<IList<Barcode>, int, int> listener;

            public QRCodeAnalyser(Action<IList<Barcode>, int, int> listener)
            {
                var options = new BarcodeScannerOptions.Builder()
                    .SetBarcodeFormats(Barcode.FormatQrCode)
                    .Build();
                detector = BarcodeScanning.GetClient(options);
                this.listener = listener;
            }

            void Analyze(InputImage image)
            {
                detector.Process(image)
                    .AddOnSuccessListener(this)
                    .AddOnFailureListener(this)
                    .AddOnCompleteListener(this);
            }

            public void Analyze(AUri imageUri)
            {
                var image = InputImage.FromFilePath(XEPlatform.AppContext, imageUri);
                Analyze(image);
            }

            public void Analyze(string imageFilePath)
                => Analyze(AUri.FromFile(new(imageFilePath))!);

            void ImageAnalysis.IAnalyzer.Analyze(IImageProxy imageProxy)
            {
                var mediaImage = imageProxy.Image;
                if (mediaImage == null)
                {
                    imageProxy.Close();
                    return;
                }
                this.imageProxy = imageProxy;
                var image = InputImage.FromMediaImage(mediaImage, imageProxy.ImageInfo.RotationDegrees);
                Analyze(image);
            }

            void IOnSuccessListener.OnSuccess(JObject result)
            {
                var result_ = result.JavaCast<JavaList<Barcode>>();
                if (!result_.Any_Nullable()) return;
                OnSuccess(result_!);
            }

            void OnSuccess(IList<Barcode> barCodes)
            {
                listener(barCodes, imageProxy?.Width ?? 0, imageProxy?.Height ?? 0);
                // 接收到结果后，就关闭解析
                detector.Close();
            }

            void IOnFailureListener.OnFailure(JException e)
            {
#if DEBUG
                Log.Error(nameof(QRCodeAnalyser), e, "IOnFailureListener.OnFailure");
#endif
            }

            void IOnCompleteListener.OnComplete(GmsTask task)
            {
                if (imageProxy != null)
                {
                    imageProxy.Close();
                    imageProxy = null;
                }
            }
        }

        public static void StartActivity(Activity activity, Action<IEnumerable<byte[]>> onScanCompleted)
        {
            ScanCompletedHandler = onScanCompleted;
            activity.StartActivity<BarcodeScannerActivity>();
        }

        public static void StartActivity(Fragment fragment, Action<IEnumerable<byte[]>> onScanCompleted)
        {
            ScanCompletedHandler = onScanCompleted;
            fragment.StartActivity<BarcodeScannerActivity>();
        }

        public static void Analyze(string imageFilePath, Action<IList<Barcode>, int, int> listener)
        {
            var analyser = new QRCodeAnalyser(listener);
            analyser.Analyze(imageFilePath);
        }
    }
}