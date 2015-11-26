using System.Threading.Tasks;
using UnIRC.IrcEvents;

namespace UnIRC.Shared.Models
{
    public interface IConnectionEndpoint
    {
        bool IsAvailable { get; }

        void CreateConnection(int id);
        Task ConnectAsync(int connectionId, string hostname, int port);
        Task DisconnectAsync(int connectionId);
        Task<IrcEvent> WaitForEventAsync(int connectionId);
        Task SendStringAsync(int connectionId, string data);
        string GetLocalAddress(int connectionId);
    }
}
