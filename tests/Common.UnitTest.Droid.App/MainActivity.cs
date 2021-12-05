using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using NUnit.Runner.Services;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using NUnitApp = NUnit.Runner.App;
using XEPlatform = Xamarin.Essentials.Platform;
using TinyPinyin;

namespace System.UnitTest
{
    [Activity(MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsApplicationActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            PinyinHelper.InitWithCnCityDict(ApplicationContext!);

            Forms.Init(this, savedInstanceState);

            // This will load all tests within the current project
            var nunit = new NUnitApp
            {
                // If you want to add tests in another assembly
                //nunit.AddTestAssembly(typeof(MyTests).Assembly);

                // Available options for testing
                Options = new TestOptions
                {
                    // If True, the tests will run automatically when the app starts
                    // otherwise you must run them manually.
                    AutoRun = true,

                    // If True, the application will terminate automatically after running the tests.
                    //TerminateAfterExecution = true,

                    // Information about the tcp listener host and port.
                    // For now, send result as XML to the listening server.
                    //TcpWriterParameters = new TcpWriterInfo("192.168.0.108", 13000),

                    // Creates a NUnit Xml result file on the host file system using PCLStorage library.
                    // CreateXmlResultFile = true,

                    // Choose a different path for the xml result file
                    // ResultFilePath = Path.Combine(Environment.ExternalStorageDirectory.Path, Environment.DirectoryDownloads, "Nunit", "Results.xml")
                }
            };

            nunit.AddTestAssembly(typeof(ByteArrayTest).Assembly);

            LoadApplication(nunit);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            XEPlatform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}