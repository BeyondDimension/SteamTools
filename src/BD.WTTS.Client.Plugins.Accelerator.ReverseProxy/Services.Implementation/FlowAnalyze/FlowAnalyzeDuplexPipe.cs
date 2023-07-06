// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowAnalyzeDuplexPipe.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class FlowAnalyzeDuplexPipe : DelegatingDuplexPipe<FlowAnalyzeStream>
{
    public FlowAnalyzeDuplexPipe(IDuplexPipe duplexPipe, IFlowAnalyzer flowAnalyzer) : base(duplexPipe, stream => new FlowAnalyzeStream(stream, flowAnalyzer))
    {
    }
}