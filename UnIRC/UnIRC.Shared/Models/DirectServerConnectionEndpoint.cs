using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace UnIRC.Shared.Models
{
    public class DirectServerConnectionEndpoint : IConnectionEndpoint
    {
        public Dictionary<int, DirectServerConnection> Connections { get; set; }
            = new Dictionary<int, DirectServerConnection>();

        public bool IsAvailable { get; } = true;

        public void CreateConnection(int connectionId)
        {
            if (Connections.ContainsKey(connectionId))
                throw new ArgumentException($@"Duplicate connection key {connectionId} exists.");

            Connections.Add(connectionId, new DirectServerConnection {Id = connectionId});
        }

        public async Task ConnectAsync(int connectionId, string hostname, int port)
        {
            await Connections[connectionId].ConnectAsync(hostname, port);
        }

        public async Task DisconnectAsync(int connectionId)
        {
            await Connections[connectionId].DisconnectAsync();
        }

        public async Task<string> ReadStringAsync(int connectionId)
        {
            return await Connections[connectionId].ReadStringAsync();
        }

        public async Task SendStringAsync(int connectionId, string data)
        {
            await Connections[connectionId].SendStringAsync(data);
        }

        public HostName GetLocalAddress(int connectionId)
        {
            return Connections[connectionId].LocalAddress;
        }
    }
}
