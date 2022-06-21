// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowAnalyzeDuplexPipe.cs

using System.Application.Services;
using System.IO.Pipelines;

namespace System.Application.Internals.FlowAnalyze;

sealed class FlowAnalyzeDuplexPipe : DelegatingDuplexPipe<FlowAnalyzeStream>
{
    public FlowAnalyzeDuplexPipe(IDuplexPipe duplexPipe, IFlowAnalyzer flowAnalyzer) : base(duplexPipe, stream => new FlowAnalyzeStream(stream, flowAnalyzer))
    {
    }
}
