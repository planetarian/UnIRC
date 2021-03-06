﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        private readonly object _readDataLock = new object();
        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1);

        private DateTime _lastSocketActivity = DateTime.MinValue;
#if WINDOWS_UWP
        private CancellationTokenSource _cts;
        private int _ctsId;
#endif


#if WINDOWS_UWP
        public StreamSocket Socket { get; set; }
        public DataReader Reader { get; set; }
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
                    if (IsDisconnecting)
                        throw new InvalidOperationException("Currently disconnecting.");
                    IsConnecting = true;
                }
            }

            var targetHostname = new HostName(hostname);


            SocketActivityInformation socketInformation;
            foreach (KeyValuePair<string, SocketActivityInformation> socket in SocketActivityInformation.AllSockets)
            {
                
            }
            Socket = new StreamSocket();
            //Socket.EnableTransferOwnership();

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

            _lastSocketActivity = DateTime.Now;

            Reader = new DataReader(Socket.InputStream)
            {
                InputStreamOptions = InputStreamOptions.Partial,
                UnicodeEncoding = UnicodeEncoding.Utf8
            };

            IsConnected = true;
            IsConnecting = false;

            _ctsId++;
            _cts = new CancellationTokenSource();
            // ReSharper disable once UnusedVariable
            Task task = CheckCloseSocket(_ctsId);
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
                    }//*/
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
                IsDisconnecting = false;
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
                lock (_readDataLock)
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
                }

                uint bytesRead;
                try
                {
                    bytesRead = await Reader.LoadAsync(bufferLength).AsTask(_cts.Token);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new OperationCanceledException("Socket disconnected by user.", ex);
                }
                catch (COMException ex)
                {
                    throw new OperationCanceledException("Socket disconnected by user.", ex);
                }
                catch (TaskCanceledException ex)
                {
                    if (IsDisconnecting)
                        throw new OperationCanceledException("Socket disconnected by user.", ex);

                    await DisconnectAsync();
                    throw new TimeoutException("Connect timed out.", ex);
                }
                catch (Exception ex) when (
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.SoftwareCausedConnectionAbort ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.OperationAborted ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.ConnectionResetByPeer ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.ConnectionTimedOut ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.HostIsDown ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.NetworkIsDown ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.NetworkDroppedConnectionOnReset ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.NetworkIsUnreachable ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown ||
                    SocketError.GetStatus(ex.HResult) == SocketErrorStatus.UnreachableHost)
                {
                    // We closed the connection and cleanly disposed the reader
                    throw new OperationCanceledException($"Socket disconnected. ({SocketError.GetStatus(ex.HResult)})",
                        ex);
                }

                _lastSocketActivity = DateTime.Now;

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
            if (!IsConnected || Socket == null)
                return;

            try
            {
                using (var writer = new DataWriter(Socket.OutputStream))
                {
                    writer.UnicodeEncoding = UnicodeEncoding.Utf8;
                    writer.WriteString(data + "\r\n");
                    await writer.StoreAsync().WithTimeout(_sendTimeoutSeconds);
                    await writer.FlushAsync();
                    writer.DetachStream();
                }
            }
            catch (TaskCanceledException)
            {
                if (IsDisconnecting)
                    throw new OperationCanceledException("Socket disconnected by user.");

                await DisconnectAsync();
                throw new TimeoutException("Connect timed out.");
            }
            catch (NullReferenceException)
            {
                // Race condition -- Socket was null-referenced right after we checked
            }

            _lastSocketActivity = DateTime.Now;
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

#if WINDOWS_UWP
        public async Task CheckCloseSocket(int ctsId)
        {
            while (IsConnected && _ctsId == ctsId)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                if (DateTime.Now > _lastSocketActivity.Add(TimeSpan.FromSeconds(_readTimeoutSeconds)))
                    break;
            }
            _cts.CancelAfter(1);
        }
#endif

    }
}