namespace BD.WTTS;

static partial class Program
{
    internal sealed partial class Host
    {
        public static Host Instance { get; } = new();

        public App App { get; } = new();

        Host()
        {

        }
    }
}