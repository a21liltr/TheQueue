using System.Threading;
using System.Timers;

namespace TheQueue.Server.Core.Models
{
	public class QueuedClient
	{
		public string ClientId { get; set; }
		public string Name { get; set; }
		private System.Timers.Timer _heartbeatTimer { get; set; }
		private const int _heartbeatInterval = 4000;
		public delegate void DisconnectEventHandler(string clientId);
		public event DisconnectEventHandler OnDisconnect;
		public QueuedClient()
        {
			_heartbeatTimer = new System.Timers.Timer(_heartbeatInterval);
			_heartbeatTimer.Elapsed += Disconnect;
		}
		private void Disconnect(object source, ElapsedEventArgs e)
		{
			OnDisconnect(ClientId);
		}

		public void OnHeartbeat()
		{
			_heartbeatTimer.Stop();
			_heartbeatTimer.Interval = _heartbeatInterval;
			_heartbeatTimer.Start();
		}
	}
}
