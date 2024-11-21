using System.Threading;
using System.Timers;

namespace TheQueue.Server.Core.Models
{
	public class ConnectedClient : IDisposable
	{
		public string ClientId { get; set; }
		public string Name { get; set; }
		private System.Timers.Timer _heartbeatTimer { get; set; }
		private const int _heartbeatInterval = 4000;
        private bool disposedValue;

        public delegate void DisconnectEventHandler(string clientId, string name);
		public event DisconnectEventHandler OnDisconnect;
		public ConnectedClient()
        {
            _heartbeatTimer = new System.Timers.Timer(_heartbeatInterval);
            _heartbeatTimer.Elapsed += Disconnect;
            _heartbeatTimer.Start();
        }

        private void Disconnect(object source, ElapsedEventArgs e)
        {
            if (OnDisconnect != null)
            {
                OnDisconnect(ClientId, Name);
                _heartbeatTimer.Stop();
            }
        }

		public void OnHeartbeat()
		{
			_heartbeatTimer.Stop();
			_heartbeatTimer.Start();
		}

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _heartbeatTimer.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
