using System;
using System.IO;
using System.Threading.Tasks;

namespace Nlog.Targets.Fluentd.Core.Sinks.Fluentd.Endpoints
{
    interface IEndpoint : IDisposable
    {
        Stream GetStream();
        Task ConnectAsync();
        bool IsConnected();
    }
}
