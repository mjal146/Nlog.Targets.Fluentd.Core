﻿using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Nlog.Targets.Fluentd.Core.Sinks.Fluentd.Endpoints.Helpers;

namespace Nlog.Targets.Fluentd.Core.Sinks.Fluentd.Endpoints
{
    class UdsEndpoint : IEndpoint
    {
        private readonly FluentdSinkOptions _options;
        private Socket _socketFile;
        private UnixEndPoint _unixEndpoint;

        public UdsEndpoint(FluentdSinkOptions options)
        {
            _options = options;
            _socketFile = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            _unixEndpoint = new UnixEndPoint(_options.UdsSocketFilePath);
        }

        public async Task ConnectAsync()
        {
            await Task.Run(() => { _socketFile.Connect(_unixEndpoint); });
        }

        public Stream GetStream()
        {
            return new NetworkStream(_socketFile);
        }

        public bool IsConnected()
        {
            return _socketFile != null && _socketFile.Connected;
        }

        public void Dispose()
        {
            _socketFile.Dispose();
            _socketFile = null;
            _unixEndpoint = null;
        }
    }
}
