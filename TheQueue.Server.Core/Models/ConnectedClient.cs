using System.Xml.Linq;
using TheQueue.Server.Core.Enums;

namespace TheQueue.Server.Core.Models
{
    public class ConnectedClient
    {
        public string Name { get; set; }
        public int HeartbeatTimer { get; set; }
        public ConnectionType ConnectionType { get; set; }

        public ConnectedClient(string name, ConnectionType connectionType = ConnectionType.Student)
        {
            Name = name;
            HeartbeatTimer = 0;
            ConnectionType = connectionType;
        }
    }
}
