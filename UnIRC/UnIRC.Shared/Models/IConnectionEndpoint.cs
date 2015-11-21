using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;

namespace UnIRC.Shared.Models
{
    public interface IConnectionEndpoint
    {
        bool IsAvailable { get; }

        void CreateConnection(int id);
        Task ConnectAsync(int connectionId, string hostname, int port);
        Task DisconnectAsync(int connectionId);
        Task<string> ReadStringAsync(int connectionId);
        Task SendStringAsync(int connectionId, string data);
        HostName GetLocalAddress(int connectionId);
    }
}
