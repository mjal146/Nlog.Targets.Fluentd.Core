using System;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using NLog.Targets;
using Nlog.Targets.Fluentd.Core.Sinks.Fluentd;

namespace Nlog.Targets.Fluentd.Core
{
    [Target("Fluentd")]
    public class FluentdTarget : TargetWithLayout
    {
        private FluentdSinkClient _sinkClient;
        private readonly FluentdSinkOptions _fluentdSinkOptions;
        private Task _previousTask;

        /// <summary>
        /// Initializes a new instance of the Fluentd logging target.
        /// </summary>
        public FluentdTarget(FluentdSinkOptions fluentdSinkOptions)
        {
            _fluentdSinkOptions = fluentdSinkOptions;
        }

        /// <summary>
        /// Initializes a new instance of the Fluentd logging target.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="fluentdSinkOptions"></param>
        public FluentdTarget(string name, FluentdSinkOptions fluentdSinkOptions) : this(fluentdSinkOptions)
        {
            Name = name;
        }
 
        /// <summary>
        /// Resets all objects related to the fluentd connection.
        /// </summary>
        private void ResetConnection()
        {
            try
            {
                _sinkClient.Dispose();
            }
            catch (Exception ex)
            {
                InternalLogger.Warn("Fluentd: Connection Reset Error  - " + ex.ToString());
            }
            finally
            {
                _sinkClient = null;
            }
        }

        /// <summary>
        /// Closes the Target
        /// </summary>
        protected override void CloseTarget()
        {
            ResetConnection();
            base.CloseTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            WriteTask(logEvent).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if the tcp connection is healthy and that the host hasn't been modified,
        /// if it has then the connection is reset.
        /// </summary>
        private void CheckConnectionIsValid()
        {
            if (_sinkClient != null) return;
            ResetConnection();
            _sinkClient = new FluentdSinkClient(_fluentdSinkOptions);
        }
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            CheckConnectionIsValid();
            var continuation = logEvent.Continuation;
            if (_previousTask == null)
                _previousTask = WriteTask(logEvent.LogEvent);
            else
                _previousTask = _previousTask.ContinueWith(a=>WriteTask(logEvent.LogEvent));
            _previousTask = _previousTask.ContinueWith(prevTask => continuation(prevTask.Exception));
        }

        private Task WriteTask(LogEventInfo logEvent)
        {
            return _sinkClient.SendAsync(logEvent);
        }
    }
}