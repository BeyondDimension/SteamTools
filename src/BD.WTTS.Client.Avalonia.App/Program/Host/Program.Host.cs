// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    internal sealed partial class Host
    {
        public static Host Instance { get; } = new();

        readonly Lazy<App> mApp = new(() => new());

        public App App => mApp.Value;

        Host()
        {

        }
    }
}