using BetaCensor.Core.Messaging;
using BetaCensor.Web;
using BetaCensor.Workers;

namespace BetaCensor.Server
{
    public class ServerInfoService : IServerInfoService {
        private readonly int _workerCount;
        private readonly ServerOptions _options;
        private readonly Dictionary<string, bool> _enabledServices = new();
        private readonly IAsyncBackgroundQueue<CensorImageRequest> _queue;

        public ServerInfoService(IConfiguration config, IAsyncBackgroundQueue<CensorImageRequest> requestQueue)
        {
            var options = config.GetServerOptions() ?? new ServerOptions();
            _workerCount = options.WorkerCount;
            _options = options;
            _enabledServices.Add("REST", options.EnableRest);
            _enabledServices.Add("SignalR", options.EnableSignalR);
            _enabledServices.Add("Socket", false);
            _queue = requestQueue;
        }

        public Dictionary<string, string> GetComponents() {
            return new Dictionary<string, string> {
                ["Workers"] = _workerCount.ToString(),
                ["Optimization"] = _options.OptimizationMode.ToString()
            };
        }

        public int? GetRequestCount() {
            var count = _queue.GetItemCount();
            return count;
        }

        public Dictionary<string, bool>? GetServices() => _enabledServices;
    }
}