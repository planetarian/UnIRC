using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnIRC.IrcEvents;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.IrcEvents;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
#endif

namespace UnIRC.Shared.Models
{
    public class DirectServerConnection
    {
        public int Id { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public bool IsConnected { get; set; }
        public bool IsConnecting { get; set; }
        public bool IsDisconnecting { get; set; }
        public string CurrentReadData { get; set; } = "";
        private const int _connectTimeoutSeconds = 20;
        private const int _readTimeoutSeconds = 60*5;
        private const int _sendTimeoutSeconds = 60;
        private readonly object _connectLock = new object();
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1);

#if WINDOWS_UWP
        public StreamSocket Socket { get; set; }
        public DataReader Reader { get; set; }
        public DataWriter Writer { get; set; }
#endif

#pragma warning disable 1998
        public async Task ConnectAsync(string hostname, int port)
#pragma warning restore 1998
        {
#if WINDOWS_UWP
            using (await _asyncLock.LockAsync())
            {
                lock (_connectLock)
                {
                    if (IsConnected)
                        throw new InvalidOperationException("Already connected.");
                    if (IsConnecting)
                        throw new InvalidOperationException("Already connecting.");
                    IsConnecting = true;
                }
            }

            var targetHostname = new HostName(hostname);
            Socket = new StreamSocket();

            try
            {
                await Socket.ConnectAsync(targetHostname, port.ToString())
                    .WithTimeout(_connectTimeoutSeconds);
            }
            catch (TaskCanceledException)
            {
                if (IsDisconnecting)
                    throw new OperationCanceledException("Connect cancelled.");

                await DisconnectAsync();
                throw new TimeoutException("Connect timed out.");
            }
            catch
            {
                // Nothin' doin'
            }

            Reader = new DataReader(Socket.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial,
                UnicodeEncoding = UnicodeEncoding.Utf8
            };
            Writer = new DataWriter(Socket.OutputStream);

            IsConnected = true;
            IsConnecting = false;
#else
            throw new NotImplementedException();
#endif
        }

#pragma warning disable 1998
        public async Task DisconnectAsync()
#pragma warning restore 1998
        {
#if WINDOWS_UWP
            using (await _asyncLock.LockAsync())
            {
                if (!IsConnecting && !IsConnected)
                    return;
                IsDisconnecting = true;

                if (Socket != null)
                {
                    try
                    {
                        await Socket.CancelIOAsync();
                    }
                    catch
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
                IsConnecting = false;
                IsConnected = false;
            }
#else
            throw new NotImplementedException();
#endif
        }

#pragma warning disable 1998
        public async Task<IrcEvent> WaitForEventAsync()
#pragma warning restore 1998
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
                catch
                {
                    throw;
                }

                if (bytesRead == 0)
                {
                    IsConnected = false;
                    throw new EndOfStreamException(CurrentReadData);
                }
                string readString = Reader.ReadString(bytesRead);
                CurrentReadData += readString;

                await Task.Delay(10);
            }
#else
            throw new NotImplementedException();
#endif
        }

#pragma warning disable 1998
        public async Task SendStringAsync(string data)
#pragma warning restore 1998
        {
#if WINDOWS_UWP
            if (!IsConnected)
                return;

            Writer.WriteString(data + "\r\n");
            try
            {
                await Writer.StoreAsync().WithTimeout(_sendTimeoutSeconds);
            }
            catch (TaskCanceledException)
            {
                if (!IsDisconnecting)
                    throw new TimeoutException("Socket connection timed out.");
            }
#else
            throw new NotImplementedException();
#endif
        }



#if WINDOWS_UWP
        public string LocalAddress => Socket?.Information.LocalAddress.ToString();
#else
        public string LocalAddress
        {
            get { throw new NotImplementedException(); }
        }
#endif
    }
}