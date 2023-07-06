// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowAnalyzeStream.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class FlowAnalyzeStream : DelegatingStream
{
    readonly IFlowAnalyzer flowAnalyzer;

    public FlowAnalyzeStream(Stream inner, IFlowAnalyzer flowAnalyzer)
        : base(inner)
    {
        this.flowAnalyzer = flowAnalyzer;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = base.Read(buffer, offset, count);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    public override int Read(Span<byte> destination)
    {
        var read = base.Read(destination);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var read = await base.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        var read = await base.ReadAsync(destination, cancellationToken);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, count);
        base.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> source)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, source.Length);
        base.Write(source);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, count);
        return base.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, source.Length);
        return base.WriteAsync(source, cancellationToken);
    }
}