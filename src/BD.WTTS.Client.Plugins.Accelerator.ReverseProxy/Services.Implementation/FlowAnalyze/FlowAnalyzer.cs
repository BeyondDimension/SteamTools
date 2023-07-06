// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.FlowAnalyze/FlowAnalyzer.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class FlowAnalyzer : IFlowAnalyzer
{
    const int INTERVAL_SECONDS = 5;

    readonly FlowQueues readQueues = new(INTERVAL_SECONDS);
    readonly FlowQueues writeQueues = new(INTERVAL_SECONDS);

    /// <summary>
    /// 收到数据
    /// </summary>
    /// <param name="flowType"></param>
    /// <param name="length"></param>
    public void OnFlow(FlowType flowType, int length)
    {
        if (flowType == FlowType.Read)
            readQueues.OnFlow(length);
        else
            writeQueues.OnFlow(length);
    }

    /// <summary>
    /// 获取流量分析
    /// </summary>
    /// <returns></returns>
    public FlowStatistics GetFlowStatistics() => new()
    {
        TotalRead = readQueues.TotalBytes,
        TotalWrite = writeQueues.TotalBytes,
        ReadRate = readQueues.GetRate(),
        WriteRate = writeQueues.GetRate(),
    };

    sealed class FlowQueues
    {
        int cleaning = 0;
        long totalBytes = 0L;

        record QueueItem(long Ticks, int Length);

        readonly ConcurrentQueue<QueueItem> queues = new();

        readonly int intervalSeconds;

        public long TotalBytes => totalBytes;

        public FlowQueues(int intervalSeconds)
        {
            this.intervalSeconds = intervalSeconds;
        }

        public void OnFlow(int length)
        {
            Interlocked.Add(ref totalBytes, length);
            CleanInvalidRecords();
            queues.Enqueue(new QueueItem(Environment.TickCount64, length));
        }

        public double GetRate()
        {
            CleanInvalidRecords();
            double intervalSecondsDouble = intervalSeconds;
            return queues.Sum(item => item.Length) / intervalSecondsDouble;
        }

        /// <summary>
        /// 清除无效记录
        /// </summary>
        /// <returns></returns>
        bool CleanInvalidRecords()
        {
            if (Interlocked.CompareExchange(ref cleaning, 1, 0) != 0)
                return false;

            var ticks = Environment.TickCount64;
            while (queues.TryPeek(out var item))
                if (ticks - item.Ticks < intervalSeconds * 1000)
                    break;
                else
                    queues.TryDequeue(out _);

            Interlocked.Exchange(ref cleaning, 0);
            return true;
        }
    }
}