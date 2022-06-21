// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/DelegatingDuplexPipe.cs

namespace System.IO.Pipelines;

class DelegatingDuplexPipe<TDelegatingStream> : IDuplexPipe, IAsyncDisposable where TDelegatingStream : DelegatingStream
{
    private bool disposed;
    private readonly object syncRoot = new();

    public PipeReader Input { get; }

    public PipeWriter Output { get; }

    public DelegatingDuplexPipe(IDuplexPipe duplexPipe, Func<Stream, TDelegatingStream> delegatingStreamFactory) : this(duplexPipe, new StreamPipeReaderOptions(leaveOpen: true), new StreamPipeWriterOptions(leaveOpen: true), delegatingStreamFactory)
    {
    }

    public DelegatingDuplexPipe(IDuplexPipe duplexPipe, StreamPipeReaderOptions readerOptions, StreamPipeWriterOptions writerOptions, Func<Stream, TDelegatingStream> delegatingStreamFactory)
    {
        var delegatingStream = delegatingStreamFactory(duplexPipe.AsStream());
        Input = PipeReader.Create(delegatingStream, readerOptions);
        Output = PipeWriter.Create(delegatingStream, writerOptions);
    }

    public virtual async ValueTask DisposeAsync()
    {
        lock (syncRoot)
        {
            if (disposed == true)
            {
                return;
            }
            disposed = true;
        }

        await Input.CompleteAsync();
        await Output.CompleteAsync();
    }
}