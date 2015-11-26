using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnIRC.IrcEvents;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.IrcEvents;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
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
        private int _connectTimeoutSeconds = 20;
        private int _readTimeoutSeconds = 60*5;
        private int _sendTimeoutSeconds = 60;

        public async Task ConnectAsync(string hostname, int port)
        {
#if WINDOWS_UWP
            ConnectionOpen = true;
            var targetHostname = new HostName(hostname);
            Socket = new StreamSocket();

            try
            {
                await Socket.ConnectAsync(targetHostname, port.ToString())
                    .WithTimeout(_connectTimeoutSeconds);
            }
            catch (TaskCanceledException)
            {
                Socket.Dispose();
                throw new TimeoutException("Socket connection timed out.");
            }

            Reader = new DataReader(Socket.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial,
                UnicodeEncoding = UnicodeEncoding.Utf8
            };
            Writer = new DataWriter(Socket.OutputStream);
#else
            throw new NotImplementedException();
#endif
        }

        public async Task DisconnectAsync()
        {
#if WINDOWS_UWP
            try
            {
                if (Socket != null)
                {
                    try
                    {
                        await Socket.CancelIOAsync();
                    }
                    catch (Exception ex)
                    {
                        // object already closed
                    }
                    if (Writer != null)
                    {
                        Writer.DetachStream();
                        Writer.Dispose();
                    }
                    if (Reader != null)
                    {
                        Reader.DetachStream();
                        Reader.Dispose();
                    }
                    Socket.Dispose();
                    Socket = null;
                }
                CurrentReadData = "";
                ConnectionOpen = false;
            }
            catch (Exception ex)
            {
                
            }
            await Task.Delay(10);
#else
            throw new NotImplementedException();
#endif
        }

        public async Task<IrcEvent> WaitForEventAsync()
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
                    IrcEvent ev = IrcEvent.GetEvent(message);
                    ev.EventType = IrcEventType.FromServer;
                    return ev;
                }

                uint bytesRead;
                try
                {
                    bytesRead = await Reader.LoadAsync(bufferLength).WithTimeout(_readTimeoutSeconds);
                }
                catch (TaskCanceledException)
                {
                    throw new TimeoutException("Socket connection timed out.");
                }

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

                await Task.Delay(10);
            }
#else
            throw new NotImplementedException();
#endif
        }

        public async Task SendStringAsync(string data)
        {
#if WINDOWS_UWP
            Writer.WriteString(data + "\r\n");
            try
            {
                await Writer.StoreAsync().WithTimeout(_sendTimeoutSeconds);
            }
            catch (TaskCanceledException) {
                throw new TimeoutException("Socket connection timed out.");
            }
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