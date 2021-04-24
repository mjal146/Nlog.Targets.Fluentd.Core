using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NLog;
using NLog.Common;
using Nlog.Targets.Fluentd.Core.Sinks.Fluentd.Endpoints;

namespace Nlog.Targets.Fluentd.Core.Sinks.Fluentd
{
    public class FluentdSinkClient : IDisposable
    {
        private readonly FluentdSinkOptions _options;
        private IEndpoint _endpoint;
        private Stream _stream;
        private FluentdEmitter _emitter;

        public FluentdSinkClient(FluentdSinkOptions options)
        {
            _options = options;
        }

        private void InitializeEndpoint()
        {
            Cleanup();

            if(_options.UseUnixDomainSocketEndpoit)
            {
                _endpoint = new UdsEndpoint(_options);
            }
            else
            {
                _endpoint = new TcpEndpoint(_options);
            }
        }

        private async Task EnsureConnectedAsync()
        {
            try
            {
                bool endpointInitialzied = _endpoint?.IsConnected() ?? false;

                if (endpointInitialzied) {
                    return;
                }

                InitializeEndpoint();

                await _endpoint.ConnectAsync();

                _stream = _endpoint.GetStream();
                _emitter = new FluentdEmitter(_stream);
            }
            catch (Exception ex)
            { 
                InternalLogger.Error($"[Serilog.Sinks.Fluentd] Connection exception {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void Cleanup()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
            if (_endpoint != null)
            {
                _endpoint.Dispose();
                _endpoint = null;
            }

            _emitter = null;
        }

        public async Task SendAsync(LogEventInfo logEvent, int retryCount = 1)
        {
            var record = new Dictionary<string, object>
            {
                {"Level", logEvent.Level.ToString()},
            };

            foreach (var log in logEvent.Properties)
            {
                record.Add(log.Key.ToString(),log.Value);
            }

            if (logEvent.Exception != null)
            {
                var exception = logEvent.Exception;
                var errorFormatted = new Dictionary<string, object>
                {
                    {"Type", exception.GetType().FullName},
                    {"Message", exception.Message},
                    {"Source", exception.Source},
                    {"StackTrace", exception.StackTrace},
                    {"Details", exception.ToString()}
                };
                record.Add("Exception", errorFormatted);
            }

            await EnsureConnectedAsync();

            if (_emitter != null)
            {
                try
                {
                    _emitter.Emit(logEvent.TimeStamp, _options.Tag, record);
                }
                catch (Exception ex)
                {
                    InternalLogger.Trace($"[Serilog.Sinks.Fluentd] Send exception {ex.Message}\n{ex.StackTrace}");
                    await RetrySendAsync(logEvent, retryCount);
                }
            }
            else
            {
                await RetrySendAsync(logEvent, retryCount);
            }
        }

        private async Task RetrySendAsync(LogEventInfo logEvent, int retryCount)
        {
            if (retryCount < _options.RetryCount)
            {
                await Task.Delay(_options.RetryDelay);
                InternalLogger.Trace($"[Serilog.Sinks.Fluentd] Retry send {retryCount + 1}");
                await SendAsync(logEvent, retryCount + 1);
            }
            else
            {
                InternalLogger.Trace(
                    $"[Serilog.Sinks.Fluentd] Retry count has exceeded limit {_options.RetryCount}. Giving up. Data will be lost");
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}