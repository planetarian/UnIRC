using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace UnIRC.Shared.Models
{
    public class DirectServerConnection
    {
        public int Id { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }

#if WINDOWS_UWP
        public StreamSocket Socket { get; set; }
        public DataReader Reader { get; set; }
        public DataWriter Writer { get; set; }
#endif
        public bool ConnectionOpen { get; set; }
        public string CurrentReadData { get; set; } = "";

        public async Task ConnectAsync(string hostname, int port)
        {
#if WINDOWS_UWP
            ConnectionOpen = true;
            Socket = new StreamSocket();
            var targetHostname = new HostName(hostname);
            await Socket.ConnectAsync(targetHostname, port.ToString());
            Reader = new DataReader(Socket.InputStream);
            Reader.InputStreamOptions = InputStreamOptions.Partial;
            Writer = new DataWriter(Socket.OutputStream);
#else
            throw new NotImplementedException();
#endif
        }

        public async Task DisconnectAsync()
        {
#if WINDOWS_UWP
            await Socket.CancelIOAsync();
            Writer.DetachStream();
            Writer.Dispose();
            Reader.DetachStream();
            Reader.Dispose();
            Socket.Dispose();
            Socket = null;
            CurrentReadData = "";
            ConnectionOpen = false;
#else
            throw new NotImplementedException();
#endif
        }

        public async Task<string> ReadStringAsync()
        {
#if WINDOWS_UWP
            const int bufferLength = 250;
            const string nl = "\r\n";
            while (true)
            {
                int breakIndex = CurrentReadData.IndexOf(nl, StringComparison.Ordinal);
                if (breakIndex >= 0)
                {
                    string message = CurrentReadData.Substring(0, breakIndex);
                    CurrentReadData = CurrentReadData.Substring(breakIndex + nl.Length);
                    return message;
                }

                uint bytesRead = await Reader.LoadAsync(bufferLength);
                if (bytesRead == 0)
                {
                    if (CurrentReadData.Length > 0)
                    {
                        ConnectionOpen = false;
                        throw new EndOfStreamException(CurrentReadData);
                    }
                }
                string readString = Reader.ReadString(bytesRead);
                CurrentReadData += readString;

            }
#else
            throw new NotImplementedException();
#endif
        }

        public async Task SendStringAsync(string data)
        {
#if WINDOWS_UWP
            Writer.WriteString(data + "\r\n");
            await Writer.StoreAsync();
#else
            throw new NotImplementedException();
#endif
        }



#if WINDOWS_UWP
        public HostName LocalAddress => Socket?.Information.LocalAddress;
#else
        public HostName LocalAddress
        {
            get { throw new NotImplementedException(); }
        }
#endif
    }
}