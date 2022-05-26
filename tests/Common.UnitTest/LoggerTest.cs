using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace System;

[TestFixture]
public class LoggerTest
{
    [Test]
    public void Full()
    {
        var logger = Log.Factory.CreateLogger<LoggerTest>();
        logger.LogTrace("(LogTrace)test logger in OtherTest.");
        logger.LogDebug("(LogDebug)test logger in OtherTest.");
        logger.LogInformation("(LogInformation)test logger in OtherTest.");
        logger.LogWarning("(LogWarning)test logger in OtherTest.");
        logger.LogError("(LogError)test logger in OtherTest.");
        logger.LogCritical("(LogCritical)test logger in OtherTest.");

        EventId eventId;

        Log.Debug("tag", "msg");
        Log.Debug("tag", "msg{0}", 3);
        Log.Debug("tag", new Exception("Debug3"), "msg");
        Log.Debug("tag", new Exception("Debug4"), "msg{0}", 4);
        eventId = new EventId(1);
        Log.Debug("tag", eventId, new Exception("Debug5"), "msg{0}", 5);
        Log.Debug("tag", eventId, "msg{0}", 4);

        Log.Error("tag", "msg");
        Log.Error("tag", "msg{0}", 3);
        Log.Error("tag", new Exception("Error3"), "msg");
        Log.Error("tag", new Exception("Error4"), "msg{0}", 4);
        eventId = new EventId(2);
        Log.Error("tag", eventId, new Exception("Error5"), "msg{0}", 5);
        Log.Error("tag", eventId, "msg{0}", 4);

        Log.Info("tag", "msg");
        Log.Info("tag", "msg{0}", 3);
        Log.Info("tag", new Exception("Info3"), "msg");
        Log.Info("tag", new Exception("Info4"), "msg{0}", 4);
        eventId = new EventId(3);
        Log.Info("tag", eventId, new Exception("Info5"), "msg{0}", 5);
        Log.Info("tag", eventId, "msg{0}", 4);

        Log.Warn("tag", "msg");
        Log.Warn("tag", "msg{0}", 3);
        Log.Warn("tag", new Exception("Warn3"), "msg");
        Log.Warn("tag", new Exception("Warn4"), "msg{0}", 4);
        eventId = new EventId(4);
        Log.Warn("tag", eventId, new Exception("Warn5"), "msg{0}", 5);
        Log.Warn("tag", eventId, "msg{0}", 4);
    }
}