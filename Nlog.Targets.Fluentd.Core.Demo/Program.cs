using NLog;
using Nlog.Targets.Fluentd.Core.Sinks.Fluentd;

namespace Nlog.Targets.Fluentd.Core.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            using var fluentdTarget = new FluentdTarget(new FluentdSinkOptions("127.0.0.1", 2688, "test-tag"));
            fluentdTarget.Layout =
                new NLog.Layouts.SimpleLayout("${longdate}|${level}|${callsite}|${logger}|${message}"); 
            config.AddTarget(fluentdTarget);
            var loggingRule = new NLog.Config.LoggingRule("demo", LogLevel.Debug, fluentdTarget);
            config.LoggingRules.Add(loggingRule);
            
            var loggerFactory = new LogFactory(config);
            var logger = loggerFactory.GetLogger("demo");
            logger.Info("Hello World!");
        }
    }
}