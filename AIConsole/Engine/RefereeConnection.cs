using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MRL.SSL.GameDefinitions;
using MRL.SSL.GameDefinitions.General_Settings;
using System.IO;
using Enterprise;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Engine
{
    public class RefereeConnection
    {
        private Socket _fromReferee;

        UdpClient _refereeSocket;

        private Thread _listeningThread;
        private GoogleSerializer _gSerilizer = new GoogleSerializer();
        bool oldConnection = true;
        public RefereeConnection(bool useOld)
        {
            oldConnection = useOld;
            InitialConnections(useOld);
            if (useOld)
                _listeningThread = new Thread(new ThreadStart(refreeConnectionRun));
            else
                _listeningThread = new Thread(new ThreadStart(refreePacketRun));
            _listeningThread.Start();
        }

        public void InitialConnections(bool useOld)
        {
            if (useOld)
            {
                if (_fromReferee != null)
                    _fromReferee.Close();
                _fromReferee = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _fromReferee.Bind(new IPEndPoint(IPAddress.Any, AISettings.Default.RefPort));
                _fromReferee.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(AISettings.Default.RefIP), IPAddress.Any));
            }
            else
            {
                if (_refereeSocket != null)
                    _refereeSocket.Close();
                _refereeSocket = new UdpClient();
                _refereeSocket.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _refereeSocket.Client.Bind(new IPEndPoint(IPAddress.Any, AISettings.Default.RefPort));
                _refereeSocket.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(AISettings.Default.RefIP), IPAddress.Any));
            }
        }

        private bool _ignoreRefereeBox = false;
        public bool IgnoreRefereeBox
        {
            get { return _ignoreRefereeBox; }
            set { _ignoreRefereeBox = value; }
        }

        private void refreeConnectionRun()
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] b = new byte[10000];
            byte counter = 137;
            while (true)
            {
                //if (AISettings.NetworkSettingChanged)
                //    InitialConnections();
                int le = _fromReferee.Receive(b);
                //for (int i = 0; i < le; i += 6)
                //{
                //    if (b[i + 1] == counter)
                //        continue;
                //    counter = b[i + 1];
                //    char[] c = ascii.GetChars(b, i, 1);
                //    if (!_ignoreRefereeBox)
                //    {
                //        EngineManager.Manager.EnqueueCommand(c[0]);
                //    }
                //}
                if (!_ignoreRefereeBox)
                {
                    if (b[1] == counter)
                        continue;
                    counter = b[1];
                    char[] c = ascii.GetChars(b, 0, 6);
                    byte[] f = new byte[2];
                    f[0] = (byte)c[5];
                    f[1] = (byte)c[4];
                    EngineManager.Manager.FillEvents(new GameEvents() { BlueScore = (int)c[2], YellowScore = (int)c[3], TimeOfstage = (int)BitConverter.ToUInt16(f, 0) });
                    EngineManager.Manager.EnqueueCommand(c[0]);
                }
            }
        }
        private void refreePacketRun()
        {
            uint counter = 137;
            while (true)
            {
                var stream = RecieveData();
                referee.SSL_Referee sslReferee = null;
                if (stream != null)
                    sslReferee = _gSerilizer.DeserializeSSLRefereePacket(stream);
                if (!_ignoreRefereeBox && sslReferee != null)
                {
                    if (sslReferee.command_counter == counter)
                        continue;
                    counter = sslReferee.command_counter;
                    int bS = (sslReferee.blue != null) ? (int)sslReferee.blue.score : lastEvent.BlueScore;
                    int yS = (sslReferee.yellow != null) ? (int)sslReferee.yellow.score : lastEvent.YellowScore;
                    int tS = (int)(sslReferee.stage_time_left / 1000000f);
                    int bG = (sslReferee.blue != null) ? (int)sslReferee.blue.goalie : lastEvent.BlueGoalie;
                    int yG = (sslReferee.yellow != null) ? (int)sslReferee.yellow.goalie : lastEvent.YellowGoalie;
                    //TODO: receive ballplacement position
                    if (sslReferee.designated_position != null && sslReferee.designated_position.x != null)
                    {
                        Position2D bP = new Position2D(sslReferee.designated_position.x, sslReferee.designated_position.y);
                        lastEvent = new GameEvents() { BlueScore = bS, YellowScore = yS, TimeOfstage = tS, BlueGoalie = bG, YellowGoalie = yG, BallPlacementPosition = bP };
                    }
                    else
                    {
                        lastEvent = new GameEvents() { BlueScore = bS, YellowScore = yS, TimeOfstage = tS, BlueGoalie = bG, YellowGoalie = yG, BallPlacementPosition = null };

                    }

                    EngineManager.Manager.FillEvents(lastEvent);
                    EngineManager.Manager.EnqueueCommand(ProtoCommand2Char(sslReferee));
                }
            }
        }

        private char ProtoCommand2Char(referee.SSL_Referee sslReferee)
        {
            referee.SSL_Referee.Command command = sslReferee.command;
            if (command == referee.SSL_Referee.Command.DIRECT_FREE_BLUE)
                return 'F';
            if (command == referee.SSL_Referee.Command.DIRECT_FREE_YELLOW)
                return 'f';
            if (command == referee.SSL_Referee.Command.FORCE_START)
                return 's';
            if (command == referee.SSL_Referee.Command.GOAL_BLUE)
                return 'G';
            if (command == referee.SSL_Referee.Command.GOAL_YELLOW)
                return 'g';
            if (command == referee.SSL_Referee.Command.HALT)
                return 'H';
            if (command == referee.SSL_Referee.Command.INDIRECT_FREE_BLUE)
                return 'I';
            if (command == referee.SSL_Referee.Command.INDIRECT_FREE_YELLOW)
                return 'i';
            if (command == referee.SSL_Referee.Command.NORMAL_START)
                return ' ';
            if (command == referee.SSL_Referee.Command.PREPARE_KICKOFF_BLUE)
                return 'K';
            if (command == referee.SSL_Referee.Command.PREPARE_KICKOFF_YELLOW)
                return 'k';
            if (command == referee.SSL_Referee.Command.PREPARE_PENALTY_BLUE)
                return 'P';
            if (command == referee.SSL_Referee.Command.PREPARE_PENALTY_YELLOW)
                return 'p';
            if (command == referee.SSL_Referee.Command.STOP)
                return 'S';
            if (command == referee.SSL_Referee.Command.TIMEOUT_BLUE)
                return 'T';
            if (command == referee.SSL_Referee.Command.TIMEOUT_YELLOW)
                return 't';
            if (command == referee.SSL_Referee.Command.BALL_PLACEMENT_BLUE)
            {
                StaticVariables.ballPlacementPos = new Position2D(sslReferee.designated_position.x, sslReferee.designated_position.y);
                return 'B';
            }
            if (command == referee.SSL_Referee.Command.BALL_PLACEMENT_YELLOW)
            {
                StaticVariables.ballPlacementPos = new Position2D(sslReferee.designated_position.x, sslReferee.designated_position.y);
                return 'b';
            }
            return '$';
        }
        GameEvents lastEvent = new GameEvents();
        public MemoryStream RecieveData()
        {
            //
            try
            {
                if (_refereeSocket == null)
                    return null;
                _refereeSocket.Client.ReceiveTimeout = 5000;
                IPEndPoint temp = null;
                byte[] b = _refereeSocket.Receive(ref temp);
                var stream = new MemoryStream(b);
                return stream;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex);
                return null;
            }
        }

        public void Dispose()
        {
            _listeningThread.Abort();
        }
    }
}
