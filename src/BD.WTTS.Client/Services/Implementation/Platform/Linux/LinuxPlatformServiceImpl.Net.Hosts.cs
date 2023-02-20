#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public void WriteDefaultHostsContent(Stream stream)
    {
        stream.Position = 0;
        stream.Write("127.0.0.1\tlocalhost\n127.0.1.1\t"u8);
        stream.Write(Encoding.UTF8.GetBytes(Environment.UserName));
        stream.Write("\n\n# The following lines are desirable for IPv6 capable hosts\n::1     ip6-localhost ip6-loopback\nfe00::0 ip6-localnet\nff00::0 ip6-mcastprefix\nff02::1 ip6-allnodes\nff02::2 ip6-allrouters\n"u8);
        stream.Flush();
        stream.SetLength(stream.Position);
    }
}
#endif