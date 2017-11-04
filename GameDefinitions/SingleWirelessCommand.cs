using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions.General_Settings;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// A command that will send to robots in byte Formats
    /// </summary>
    [Serializable]
    public class SingleWirelessCommand
    {
        public SingleRotateCommand SRC = new SingleRotateCommand();
        public int RobotID;
        public short Motor1, Motor2, Motor3, Motor4;
        public byte _kickPowerByte;
        public byte Kind;
        public double _kickPower;
        public bool isChipKick;
        public bool isDelayedKick;
        public bool spinBackward;
        public bool statusRequest;
        public bool BackSensor;
        private System.Drawing.Color _color;
        public double _kickSpeed;
        public short Time;
        public int PacketID;

  
       //new direct lookup
        private static KickSpeedCoeff[] kickCoeff = new KickSpeedCoeff[12] { 
            //new KickSpeedCoeff(132.3,0,10.3,0,5.95,1), //0
            //new KickSpeedCoeff(1.995e+15,120.8,27.79,8.307,3.763,5.228), //1
            //new KickSpeedCoeff( 153.1,0,12.21,0,7.334,1), //2
            //new KickSpeedCoeff( 449.2,29.91,14.78,4.588,5.888,3.497), //3
            //new KickSpeedCoeff(false),//( 873.4,0,20.1,0,9.579,1), //4
            //new KickSpeedCoeff(717.5,0,22.73,0,11,1), //5
            //new KickSpeedCoeff(false),//(481.6,0,19.79,0,10.07,1), //6
            //new KickSpeedCoeff(1.324e+12,-7.928,238.9,1.34,39.54,0.9698), //7
            //new KickSpeedCoeff(129.1,0,11.22,0,6.811,1), //8
            //new KickSpeedCoeff(169.7,51.91,8.129,5.163,2.041,3.441), //9
            //new KickSpeedCoeff(false),//(2.159e+016,165.3,29.09,9.053,3.902,5.161), //10
            //new KickSpeedCoeff(146.2,0,11.91,0,7.035,1)};//11


            new KickSpeedCoeff(2.076e+16,54.17,97.08,5.231,15.64,3.823), //0
            new KickSpeedCoeff(8.208e+08,-1.812e+04,59.22,18.68,14.02,6.417), //1
            new KickSpeedCoeff(139.4,-49.48,6.854,5.44,4.099,2.566), //2
            new KickSpeedCoeff(1.092e+16,204.1,15.45,11.06,1.508,6.416), //3
            new KickSpeedCoeff(2.508e+15,180,10.61,9.191,0.7762,5.899), //4
            new KickSpeedCoeff(1.837e+16,103.3,72.87,6.674,11.46,4.648), //5
            new KickSpeedCoeff(246.8,4.313,13.34,5.142,7.847,0.2422), //6
            new KickSpeedCoeff(1.286e+16,161.9,22.11,10,2.558,6.121), //7
            new KickSpeedCoeff(89.27,-1.304,9.033,3.81,5.909,0.7313), //8
            new KickSpeedCoeff(244.3,21.38,10.23,3.215,4.691,2.689), //9
            new KickSpeedCoeff(6.504e+04,-6.921e+04,14.27,14.13,6.076,6.013), //10
            new KickSpeedCoeff(284.7,-21.43,12.97,6.011,7.851,1.935)};//11
       
        public System.Drawing.Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
            }
        }
        public SingleWirelessCommand()
        {
        }
        public SingleWirelessCommand(Vector2D V, double w, bool ChipKick, double kickPower, int spinBack, bool isdelayedkick, bool spinBackWard)
        {
            Vx = V.X;
            Vy = V.Y;
            W = w;
            isChipKick = ChipKick;
            _kickPower = kickPower;
            _kickPowerByte = (byte)(kickPower);
            SpinBack = spinBack;
            isDelayedKick = isdelayedkick;
            spinBackward = spinBackWard;
        }

        public SingleWirelessCommand(Vector2D V, double w, bool ChipKick, double kickPower, int spinBack, bool isdelayedkick, bool spinBackWard, bool statusReq)
        {
            Vx = V.X;
            Vy = V.Y;
            W = w;
            isChipKick = ChipKick;
            _kickPower = kickPower;
            _kickPowerByte = (byte)(kickPower);
            SpinBack = spinBack;
            isDelayedKick = isdelayedkick;
            spinBackward = spinBackWard;
            statusRequest = statusReq;
        }
        public SingleWirelessCommand(Vector2D V, double w, bool ChipKick, double kickPower, int spinBack, bool isdelayedkick, bool spinBackWard, bool statusReq, System.Drawing.Color color)
        {
            Vx = V.X;
            Vy = V.Y;
            W = w;
            isChipKick = ChipKick;
            _kickPower = kickPower;
            _kickPowerByte = (byte)(kickPower);
            SpinBack = spinBack;
            isDelayedKick = isdelayedkick;
            spinBackward = spinBackWard;
            statusRequest = statusReq;
            _color = color;
        }

        public void AddRotate(byte kind, double clockWise, int teta, int count, double vy, double omega, bool ChipKick, int spinBack = 0, bool isdelayedkick = false, bool spinBackWard = false, bool statusReq = false)
        {
            SRC = new SingleRotateCommand(clockWise, teta, count, vy, omega);
            Kind = kind;
            isChipKick = ChipKick;
            SpinBack = spinBack;
            isDelayedKick = isdelayedkick;
            spinBackward = spinBackWard;
            statusRequest = statusReq;
        }
        public void AddRotate(byte kind, SingleRotateCommand src, bool ChipKick, int spinBack = 0, bool isdelayedkick = false, bool spinBackWard = false, bool statusReq = false)
        {
            SRC = src;
            Kind = kind;
            isChipKick = ChipKick;
            SpinBack = spinBack;
            isDelayedKick = isdelayedkick;
            spinBackward = spinBackWard;
            statusRequest = statusReq;
        }
        /// <summary>
        /// Power of kick
        /// </summary>
        public double KickPower
        {
            get
            {
                return _kickPower;
            }
            set
            {
                _kickPower = (value > 255) ? 255 : value;
                //Todo: Read kick power from lookup table
                _kickPowerByte = (byte)(_kickPower * 1);
            }
        }

        public double KickSpeed
        {
            get
            {
                return _kickSpeed;
            }
            set
            {
                _kickSpeed = value;
                if (!isChipKick)
                {
                    //if (RobotID < GamePlannerInfo)
                    //{

                    //}
                    if (_kickSpeed == 0)
                    {
                        _kickPower = 0;
                        _kickPowerByte = (byte)_kickPower;
                    }
                    else
                    {
                        double a1 = 373.8;
                        double b1 = 19.7;
                        double c1 = 11.38;
                        double a2 = 0;
                        double b2 = 24.46;
                        double c2 = 12.19;
                        if (kickCoeff[RobotID].fill == true)
                        {
                            a1 = kickCoeff[RobotID].A1;
                            a2 = kickCoeff[RobotID].A2;
                            b1 = kickCoeff[RobotID].B1;
                            b2 = kickCoeff[RobotID].B2;
                            c1 = kickCoeff[RobotID].C1;
                            c2 = kickCoeff[RobotID].C2;
                        }
                        double val = Math.Min(value, StaticVariables.MaxKickSpeed);
                        double kik = (a1 * Math.Exp(-((val - b1) / c1) * (val - b1) / c1) + a2 * Math.Exp(-((val - b2) / c2) * (val - b2) / c2));
                        kik = (kik >= 255) ? 255 : (kik <= 0) ? 0 : kik;

                        _kickPower = kik;//* 0.75;
                        _kickPowerByte = (byte)_kickPower;
                    }
                }
                else
                {

                    _kickPower = LookupTable.GetChipPower(RobotID, _kickSpeed, 0, /*(SpinBack > 0) ? true :*/ false);
                    if (_kickSpeed == 0)
                        _kickPower = 0;
                    _kickPowerByte = (byte)_kickPower;
                }
            }
        }
        public double Vx, Vy, W, SpinBack;
        private struct KickSpeedCoeff
        {
            public double A1, B1, C1, A2, B2, C2;
            public bool fill;
            public KickSpeedCoeff(bool init)
            {
                A1 = 0;
                A2 = 0;
                C1 = 0;
                C2 = 0;
                B1 = 0;
                B2 = 0;
                fill = false;
            }
            public KickSpeedCoeff(double a1, double a2, double b1, double b2, double c1, double c2)
            {
                A1 = a1;
                B1 = b1;
                C1 = c1;
                A2 = a2;
                B2 = b2;
                C2 = c2;
                fill = true;
            }

        }
    }

}