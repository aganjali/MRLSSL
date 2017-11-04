using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using Enterprise;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Commuinication Control using Socket programming to connect to other computer like Visualizer PC,Referee or SSL Vision
    /// </summary>
    public class CommunicationController : IDisposable
    {
        /// <summary>
        /// Socket for send and recieve
        /// </summary>
        UdpClient _sendSocket, _recieveSocket;
        /// <summary>
        /// setting for ip name in reciving endpoint
        /// </summary>
        IPEndPoint _localEndPoint;
        /// <summary>
        /// setting for ip name in sending endpoint
        /// </summary>
        IPEndPoint _remoteEndPoint;
        /// <summary>
        /// setting for ip name in sending to analyzer endpoint
        /// </summary>
       // IPEndPoint _analyzerEndPoint;
        IPEndPoint _visualizerEndPoint;
        /// <summary>
        /// Initialize sockets and the endpoints
        /// </summary>
        /// <param name="LocalEndPoint">Recieving endpoint</param>
        /// <param name="RemoteEndPoint">Sendingendpoint</param>
        public CommunicationController(IPEndPoint LocalEndPoint, IPEndPoint RemoteEndPoint)
        {
            if (_sendSocket != null)
                return;
            _sendSocket = new UdpClient();
            if (LocalEndPoint != null)
                _recieveSocket = new UdpClient(LocalEndPoint);

            _localEndPoint = LocalEndPoint;
            _remoteEndPoint = RemoteEndPoint;
        }

        public CommunicationController(string VisionIP, int VisionPort)
        {
            if (_visionSocket != null)
                _visionSocket.Close();
            _visionSocket = new UdpClient();
            _visionSocket.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _visionSocket.Client.Bind(new IPEndPoint(IPAddress.Any, VisionPort));
            _visionSocket.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(VisionIP), IPAddress.Any));
        }
        public CommunicationController(int RemotePort, string RemoteComputerName)
        {
            IPAddress ipv4 = null;
          
            if (RemoteComputerName == "any")
            {

                try
                {
                    ipv4 = IPAddress.Any;
                    _remoteEndPoint = new IPEndPoint(ipv4, RemotePort);
                }
                catch (Exception ex)
                {
                    _remoteEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }

            }
            else
            {
                try
                {
                    IPAddress[] ips = System.Net.Dns.GetHostAddresses(RemoteComputerName);
                    foreach (IPAddress ip in ips)
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4 = ip;
                            break;
                        }
                    if (ipv4 != null)
                        _remoteEndPoint = new IPEndPoint(ipv4, RemotePort);
                }
                catch (Exception ex)
                {
                    _remoteEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }
            }

            if (_sendSocket != null)
                return;
            _sendSocket = new UdpClient();
          
        }
        /// <summary>
        /// Construct communication controller from names and ports, "any" for any Adress
        /// </summary>
        /// <param name="LocalComputerName">Recieving Computer Name</param>
        /// <param name="LocalPort">Recieving Computer Port</param>
        /// <param name="RemoteComputerName">Sending Computer Name</param>
        /// <param name="RemotePort">Sending Computer Port</param>
        public CommunicationController(string LocalComputerName, int LocalPort, string RemoteComputerName, int RemotePort)
        {
            IPAddress ipv4 = null;
            if (LocalComputerName == "any")
            {
                try
                {
                    ipv4 = IPAddress.Any;
                    _localEndPoint = new IPEndPoint(ipv4, LocalPort);
                }
                catch (Exception ex)
                {
                    _localEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }
            }
            else
            {
                try
                {
                    IPAddress[] ips = System.Net.Dns.GetHostAddresses(LocalComputerName);
                    foreach (IPAddress ip in ips)
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4 = ip;
                            break;
                        }
                    if (ipv4 != null)
                        _localEndPoint = new IPEndPoint(ipv4, LocalPort);
                }
                catch (Exception ex)
                {
                    _localEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }
            }
            if (RemoteComputerName == "any")
            {

                try
                {
                    ipv4 = IPAddress.Any;
                    _remoteEndPoint = new IPEndPoint(ipv4, RemotePort);
                }
                catch (Exception ex)
                {
                    _remoteEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }

            }
            else
            {
                try
                {
                    IPAddress[] ips = System.Net.Dns.GetHostAddresses(RemoteComputerName);
                    foreach (IPAddress ip in ips)
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4 = ip;
                            break;
                        }
                    if (ipv4 != null)
                        _remoteEndPoint = new IPEndPoint(ipv4, RemotePort);
                }
                catch (Exception ex)
                {
                    _remoteEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }
            }

            if (_sendSocket != null)
                return;
            _sendSocket = new UdpClient();
            if (_localEndPoint != null)
            {

                try
                {
                    _recieveSocket = new UdpClient(_localEndPoint);
                }
                catch (Exception ex)
                {
                    Logger.Write(LogType.Exception, ex);
                }


            }
        }
        /// <summary>
        /// Construct communication controller from names and ports, "any" for any Adress
        /// </summary>
        /// <param name="LocalComputerName">Recieving Computer Name</param>
        /// <param name="LocalPort">Recieving Computer Port</param>
        /// <param name="RemoteComputerName">Sending Computer Name</param>
        /// <param name="RemotePort">Sending Computer Port</param>
        /// <param name="recievetimeOut">recieve socket timeout</param>
        public CommunicationController(string LocalComputerName, int LocalPort, string RemoteComputerName, int RemotePort, int recievetimeOut)
        {
            IPAddress ipv4 = null;
            if (LocalComputerName == "any")
            {
                try
                {
                    ipv4 = IPAddress.Any;
                    _localEndPoint = new IPEndPoint(ipv4, LocalPort);
                }
                catch (Exception ex)
                {
                    _localEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }
            }
            else
            {
                try
                {
                    IPAddress[] ips = System.Net.Dns.GetHostAddresses(LocalComputerName);
                    foreach (IPAddress ip in ips)
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4 = ip;
                            break;
                        }
                    if (ipv4 != null)
                        _localEndPoint = new IPEndPoint(ipv4, LocalPort);
                }
                catch (Exception ex)
                {
                    _localEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }
            }
            if (RemoteComputerName == "any")
            {

                try
                {
                    ipv4 = IPAddress.Any;
                    _remoteEndPoint = new IPEndPoint(ipv4, RemotePort);
                }
                catch (Exception ex)
                {
                    _remoteEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }

            }
            else
            {
                try
                {
                    IPAddress[] ips = System.Net.Dns.GetHostAddresses(RemoteComputerName);
                    foreach (IPAddress ip in ips)
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4 = ip;
                            break;
                        }
                    if (ipv4 != null)
                        _remoteEndPoint = new IPEndPoint(ipv4, RemotePort);
                }
                catch (Exception ex)
                {
                    _remoteEndPoint = null;
                    Logger.Write(LogType.Exception, ex);
                }
            }

            if (_sendSocket != null)
                return;
            _sendSocket = new UdpClient();
            if (_localEndPoint != null)
            {
                _recieveSocket = new UdpClient(_localEndPoint);
                _recieveSocket.Client.ReceiveTimeout = recievetimeOut;
            }
        }
        /// <summary>
        /// Send a stram in remoteendpoint
        /// </summary>
        /// <param name="data">input stream for sending</param>
        public void SendData(MemoryStream data)
        {
            byte[] byteData = data.ToArray();
            try
            {
                //_sendSocket.
                _sendSocket.Send(byteData, byteData.Length, _remoteEndPoint);
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
            }
        }
        /// <summary>
        /// Reciving data from localendpoint 
        /// </summary>
        /// <returns>stram of recive data,</returns>
        public MemoryStream RecieveData()
        {
            try
            {
                IPEndPoint temp = null;
                byte[] b = _recieveSocket.Receive(ref temp);
        //        _recieveSocket.Client.ReceiveTimeout = 5000;
                var stream = new MemoryStream(b);
                return stream;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return null;
            }
        }
        UdpClient _visionSocket;

        public MemoryStream RecieveVisionData()
        {
            //
            try
            {
                if (_visionSocket == null)
                    return null;
                _visionSocket.Client.ReceiveTimeout = 5000;
                IPEndPoint temp = null;
                byte[] b = _visionSocket.Receive(ref temp);
                var stream = new MemoryStream(b);
                return stream;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return null;
            }
        }
        /// <summary>
        /// 
        /// closing socket in program
        /// </summary>
        public void Dispose()
        {
            if (_recieveSocket != null)
                _recieveSocket.Close();
            if (_sendSocket != null)
                _sendSocket.Close();
            if (_visionSocket != null)
                _visionSocket.Close();

        }

    }
}