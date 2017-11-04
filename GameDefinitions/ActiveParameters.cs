using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace MRL.SSL.GameDefinitions
{
    public class ActiveParameters
    {
        public class NewActiveParameters
        {
            public enum PlayMode
            {
                Pass = 0,
                Chip = 1,
                Direct = 2,
                PassAndDribble = 3,
                Force = 4,
                //RotateAndPass = 5
            }
            public enum KickDefult
            {
                Center = 0,
                Rear = 1
            }
            public enum SweepDefult
            {
                Direct = 0,
                Chip = 1
            }
            public enum ConflictMode
            {
                Direct = 0,
                Stop = 1,
                Rotate = 2,
                SpinRotate = 3
            }
            public enum NonClearMode
            {
                Direct = 0,
                Chip = 1
            }
            
            static public bool UseSpaceDrible = false;
            static public bool KickInRegion = false;

            public static SweepDefult sweepMode;
            public static KickDefult kickDefult;
            public static PlayMode playMode;
            public static ConflictMode conflictMode;
            public static NonClearMode clearMode;

            public static double KickInRegionAcc;
            public static double ConfilictZone;
            public static double kickAnyWayRegion;
            public static double sweepZone;
            public static double minGoodness;
            public static double clearRobotZone;
            public static double kickAccuracy;
        }
        public ActiveParameters()
        {
            Init();
        }
        static public void Init()
        {
            //RobotMotionCoefs[0, 0] = 0.5812;
            //RobotMotionCoefs[1, 0] = 1.594;
            //RobotMotionCoefs[0, 1] = -0.2455;
            //RobotMotionCoefs[2, 0] = -1.235;
            //RobotMotionCoefs[1, 1] = 0.6928;
            //RobotMotionCoefs[0, 2] = 0.04398;
            //RobotMotionCoefs[3, 0] = 0.6731;
            //RobotMotionCoefs[2, 1] = -0.5466;
            //RobotMotionCoefs[1, 2] = 0.1363;
            //RobotMotionCoefs[0, 3] = -0.01865;
            //RobotMotionCoefs[4, 0] = -0.1718;
            //RobotMotionCoefs[3, 1] = 0.1718;
            //RobotMotionCoefs[2, 2] = -0.09003;
            //RobotMotionCoefs[1, 3] = 0.00783;
            //RobotMotionCoefs[0, 4] = -0.01988;
            //RobotMotionCoefs[5, 0] = 0.01573;
            //RobotMotionCoefs[4, 1] = -0.01933;
            //RobotMotionCoefs[3, 2] = 0.01344;
            //RobotMotionCoefs[2, 3] = -6.642E-005;
            //RobotMotionCoefs[1, 4] = 0.005733;
            //RobotMotionCoefs[0, 5] = -0.0001515;
            RobotMotionCoefs = new double[6, 6];

            RobotMotionCoefs[0, 0] = 20.96;
            RobotMotionCoefs[1, 0] = 54.03;
            RobotMotionCoefs[0, 1] = 7.46;
            RobotMotionCoefs[2, 0] = -3.922;
            RobotMotionCoefs[1, 1] = -8.494;
            RobotMotionCoefs[0, 2] = -9.659;
            RobotMotionCoefs[3, 0] = -2.033;

            RobotMotionCoefs[2, 1] = -13.15;
            RobotMotionCoefs[1, 2] = 25.39;
            RobotMotionCoefs[0, 3] = 5.274;

            RobotMotionCoefs[3, 1] = 6.487;
            RobotMotionCoefs[2, 2] = -1.137;
            RobotMotionCoefs[1, 3] = -10.24;
            RobotMotionCoefs[0, 4] = -1.969;
            RobotMotionCoefs[0, 5] = 0.3372;

            //RobotMotionCoefs[4, 1] = 0.3022;
            RobotMotionCoefs[3, 2] = -1.562;
            RobotMotionCoefs[2, 3] = 1.209;
            RobotMotionCoefs[1, 4] = 1.051;
            //RobotMotionCoefs[0, 5] = 0.05445;
        }

        public enum PlayMode
        {
            Pass = 0,
            Chip = 1,
            Direct = 2,
            Force = 3,
        }
        public enum KickDefult
        {
            Center = 0,
            Rear = 1
        }
        public enum SweepDefult
        {
            Direct = 0,
            Chip = 1
        }
        public enum ConflictMode
        {
            Direct = 0,
            Stop = 1,
        }
        public enum NonClearMode
        {
            Direct = 0,
            Chip = 1
        }

        static public SweepDefult sweepMode;
        static public KickDefult kickDefult;
        static public PlayMode playMode;
        static public ConflictMode conflictMode { get; set; }
        static public NonClearMode clearMode { get; set; }
        
        static public bool UseChipDrible = false;
        static public bool UseSpaceDrible = false;
        static public bool KickInRegion = false;

        static public double sweepZone = 0;
        static public double kickAnyWayRegion = -2.2;
        static public double kickAcuercy = 0.05;
        static public double kickRegionAcuercy = 0.02;
        static public double minGoodness = 0.1;
        static public double nearIncomingRadi = 0.5;
        static public double nearBallSpeedTresh = 2;
        static public double incomingBallDistanceTresh = 1.5;
        static public double outgoingSideAngleTresh = 120;
        static public double kickedToUsRadi = 3.5;
        static public double kickedToUsBallSpeedTresh = 0.5;

        static public double staticBallSpeedTresh = 0.2;

        static public double clearRobotZone = 0.5;
        static public double ConfilictZone = 0.2;
        
        static public double KpSideY = 8;
        static public double KiSideY = 0;
        static public double KdSideY = 0.5;

        static public double IncomingBackBall = 0.09;

        static public double vyOffsetSide = 1.2;
        static public double KpxVySide = 1;
        static public double KpyVySide = 2;
        static public double KpTotalVySide = 0.1;
        
        static public double KpSideX = 9;
        static public double KiSideX = 0;
        static public double KdSideX = 0.5;
        
        static public double KpBackY = 8;
        static public double KiBackY = 0;
        static public double KdBackY = 0;
        
        static public double KpxVxSide = 1.4;
        
        static public double KpBackX = 9;
        static public double KiBackX = 0;
        static public double KdBackX = 0.5;

        static public double KpyVyBack = 0.8;
        static public double KpxVyBack = 0.5;
        static public double vyOffsetBack = 1;
        static public double KpTotalVyBack = 2.5;


        static public double KpxVxBack = 1.4;




        static public double[,] RobotMotionCoefs = new double[6, 6];

        #region xmlSerializing

        private static void writeValue(XmlTextWriter xml, string tagName, object value)
        {
            xml.WriteStartElement(tagName);
            xml.WriteValue(value);
            xml.WriteEndElement();
        }

        private static object readValue(XmlTextReader xml, string tagName)
        {
            xml.ReadStartElement(tagName);
            var ret = xml.ReadContentAsObject();
            xml.ReadEndElement();
            return ret;
        }

        public static void Save()
        {

            string fileName = "ActiveParameters.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.Default))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("ActiveParameters");

                writeValue(writer, "sweepZone", sweepZone);
                writeValue(writer, "kickAnyWayRegion", kickAnyWayRegion);
                writeValue(writer, "kickAcuercy", kickAcuercy);
                writeValue(writer, "kickRegionAcuercy", kickRegionAcuercy);
                writeValue(writer, "minGoodness", minGoodness);
                writeValue(writer, "nearIncomingRadi", nearIncomingRadi);
                writeValue(writer, "nearBallSpeedTresh", nearBallSpeedTresh);
                writeValue(writer, "incomingBallDistanceTresh", incomingBallDistanceTresh);
                writeValue(writer, "outgoingSideAngleTresh", outgoingSideAngleTresh);
                writeValue(writer, "kickedToUsRadi", kickedToUsRadi);
                writeValue(writer, "kickedToUsBallSpeedT", kickedToUsBallSpeedTresh);
                writeValue(writer, "clearRobotZone", clearRobotZone);
                writeValue(writer, "ConfilictZone", ConfilictZone);
                writeValue(writer, "IncomingBackBall", IncomingBackBall);
                writeValue(writer, "KpSideY", KpSideY);
                writeValue(writer, "KdSideY", KdSideY);
                writeValue(writer, "KiSideY", KiSideY);
                writeValue(writer, "KpSideX", KpSideX);
                writeValue(writer, "KiSideX", KiSideX);
                writeValue(writer, "KdSideX", KdSideX);
                writeValue(writer, "vyOffsetSide", vyOffsetSide);
                writeValue(writer, "KpxVySide", KpxVySide);
                writeValue(writer, "KpyVySide", KpyVySide);
                writeValue(writer, "KpTotalVySide", KpTotalVySide);
                writeValue(writer, "KpxVxSide", KpxVxSide);
                writeValue(writer, "KpBackY", KpBackY);
                writeValue(writer, "KiBackY", KiBackY);
                writeValue(writer, "KdBackY", KdBackY);
                writeValue(writer, "KpBackX", KpBackX);
                writeValue(writer, "KiBackX", KiBackX);
                writeValue(writer, "KdBackX", KdBackX);
                writeValue(writer, "KpyVyBack", KpyVyBack);
                writeValue(writer, "KpyVyBack", KpyVyBack);
                writeValue(writer, "KpxVyBack", KpxVyBack);
                writeValue(writer, "vyOffsetBack", vyOffsetBack);
                writeValue(writer, "KpTotalVyBack", KpTotalVyBack);
                writeValue(writer, "KpTotalVyBack", KpTotalVyBack);
                writeValue(writer, "KpxVxBack", KpxVxBack);

                writeValue(writer, "UseChipDrible", UseChipDrible);
                writeValue(writer, "UseSpaceDrible", UseSpaceDrible);
                writeValue(writer, "KickInRegion", KickInRegion);

                writeValue(writer, "sweepMode", (int)sweepMode);
                writeValue(writer, "kickDefult", (int)kickDefult);
                writeValue(writer, "playMode", (int)playMode);
                writeValue(writer, "conflictMode", (int)conflictMode);
                writeValue(writer, "clearMode", (int)clearMode);


                writer.WriteStartElement("NewActiveParameters");

                writeValue(writer, "UseSpaceDrible", NewActiveParameters.UseSpaceDrible);
                writeValue(writer, "KickInRegion", NewActiveParameters.KickInRegion);

                writeValue(writer, "sweepMode", (int)NewActiveParameters.sweepMode);
                writeValue(writer, "kickDefult", (int)NewActiveParameters.kickDefult);
                writeValue(writer, "playMode", (int)NewActiveParameters.playMode);
                writeValue(writer, "conflictMode", (int)NewActiveParameters.conflictMode);
                writeValue(writer, "clearMode", (int)NewActiveParameters.clearMode);


                writeValue(writer, "kickRegionAcuercy", NewActiveParameters.KickInRegionAcc);
                writeValue(writer, "ConfilictZone", NewActiveParameters.ConfilictZone);
                writeValue(writer, "kickAnyWayRegion", NewActiveParameters.kickAnyWayRegion);
                writeValue(writer, "sweepZone", NewActiveParameters.sweepZone);
                writeValue(writer, "minGoodness", NewActiveParameters.minGoodness);
                writeValue(writer, "clearRobotZone", NewActiveParameters.clearRobotZone);
                writeValue(writer, "kickAcuercy", NewActiveParameters.kickAccuracy);

                writer.WriteEndElement();


                writer.WriteStartElement("Coefs");
                
                for (int i = 0; i < 6; i++)
                    for (int j = 0; j < 6; j++)
                        writeValue(writer, string.Format("_{0}.{1}", i.ToString(), j.ToString()), RobotMotionCoefs[i, j]);

                writer.WriteEndElement();


                writer.WriteFullEndElement();
            }
        }

        public static void Load()
        {
            string fileName = "ActiveParameters.xml";
            fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            if (!File.Exists(fileName))
                return;
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                reader.ReadStartElement();

                sweepZone = double.Parse(readValue(reader, "sweepZone").ToString());
                kickAnyWayRegion = double.Parse(readValue(reader, "kickAnyWayRegion").ToString());
                kickAcuercy = double.Parse(readValue(reader, "kickAcuercy").ToString());
                kickRegionAcuercy = double.Parse(readValue(reader, "kickRegionAcuercy").ToString());
                minGoodness = double.Parse(readValue(reader, "minGoodness").ToString());
                nearIncomingRadi = double.Parse(readValue(reader, "nearIncomingRadi").ToString());
                nearBallSpeedTresh = double.Parse(readValue(reader, "nearBallSpeedTresh").ToString());
                incomingBallDistanceTresh = double.Parse(readValue(reader, "incomingBallDistanceTresh").ToString());

                outgoingSideAngleTresh = double.Parse(readValue(reader, "outgoingSideAngleTresh").ToString());
                kickedToUsRadi = double.Parse(readValue(reader, "kickedToUsRadi").ToString());
                kickedToUsBallSpeedTresh = double.Parse(readValue(reader, "kickedToUsBallSpeedT").ToString());
                clearRobotZone = double.Parse(readValue(reader, "clearRobotZone").ToString());
                ConfilictZone = double.Parse(readValue(reader, "ConfilictZone").ToString());
                IncomingBackBall = double.Parse(readValue(reader, "IncomingBackBall").ToString());
                KpSideY = double.Parse(readValue(reader, "KpSideY").ToString());
                KdSideY = double.Parse(readValue(reader, "KdSideY").ToString());
                KiSideY = double.Parse(readValue(reader, "KiSideY").ToString());
                KpSideX = double.Parse(readValue(reader, "KpSideX").ToString());
                KiSideX = double.Parse(readValue(reader, "KiSideX").ToString());
                KdSideX = double.Parse(readValue(reader, "KdSideX").ToString());
                vyOffsetSide = double.Parse(readValue(reader, "vyOffsetSide").ToString());
                KpxVySide = double.Parse(readValue(reader, "KpxVySide").ToString());
                KpyVySide = double.Parse(readValue(reader, "KpyVySide").ToString());
                KpTotalVySide = double.Parse(readValue(reader, "KpTotalVySide").ToString());
                KpxVxSide = double.Parse(readValue(reader, "KpxVxSide").ToString());
                KpBackY = double.Parse(readValue(reader, "KpBackY").ToString());
                KiBackY = double.Parse(readValue(reader, "KiBackY").ToString());
                KdBackY = double.Parse(readValue(reader, "KdBackY").ToString());
                KpBackX = double.Parse(readValue(reader, "KpBackX").ToString());
                KiBackX = double.Parse(readValue(reader, "KiBackX").ToString());
                KdBackX = double.Parse(readValue(reader, "KdBackX").ToString());
                KpyVyBack = double.Parse(readValue(reader, "KpyVyBack").ToString());
                KpyVyBack = double.Parse(readValue(reader, "KpyVyBack").ToString());
                KpxVyBack = double.Parse(readValue(reader, "KpxVyBack").ToString());
                vyOffsetBack = double.Parse(readValue(reader, "vyOffsetBack").ToString());
                KpTotalVyBack = double.Parse(readValue(reader, "KpTotalVyBack").ToString());
                KpTotalVyBack = double.Parse(readValue(reader, "KpTotalVyBack").ToString());
                KpxVxBack = double.Parse(readValue(reader, "KpxVxBack").ToString());

                UseChipDrible = bool.Parse(readValue(reader, "UseChipDrible").ToString());
                UseSpaceDrible = bool.Parse(readValue(reader, "UseSpaceDrible").ToString());
                KickInRegion = bool.Parse(readValue(reader, "KickInRegion").ToString());
     
                sweepMode = (SweepDefult)int.Parse(readValue(reader, "sweepMode").ToString());
                kickDefult = (KickDefult)int.Parse(readValue(reader, "kickDefult").ToString());
                playMode = (PlayMode)int.Parse(readValue(reader, "playMode").ToString());
                conflictMode = (ConflictMode)int.Parse(readValue(reader, "conflictMode").ToString());
                clearMode = (NonClearMode)int.Parse(readValue(reader, "clearMode").ToString());

                reader.ReadStartElement("NewActiveParameters");

                NewActiveParameters.UseSpaceDrible = bool.Parse(readValue(reader, "UseSpaceDrible").ToString());
                NewActiveParameters.KickInRegion = bool.Parse(readValue(reader, "KickInRegion").ToString());
         
                NewActiveParameters.sweepMode = (NewActiveParameters.SweepDefult)int.Parse(readValue(reader, "sweepMode").ToString());
                NewActiveParameters.kickDefult = (NewActiveParameters.KickDefult)int.Parse(readValue(reader, "kickDefult").ToString());
                NewActiveParameters.playMode = (NewActiveParameters.PlayMode)int.Parse(readValue(reader, "playMode").ToString());
                NewActiveParameters.conflictMode = (NewActiveParameters.ConflictMode)int.Parse(readValue(reader, "conflictMode").ToString());
                NewActiveParameters.clearMode = (NewActiveParameters.NonClearMode)int.Parse(readValue(reader, "clearMode").ToString());

                NewActiveParameters.KickInRegionAcc = double.Parse(readValue(reader, "kickRegionAcuercy").ToString());
                NewActiveParameters.ConfilictZone = double.Parse(readValue(reader, "ConfilictZone").ToString());
                NewActiveParameters.kickAnyWayRegion = double.Parse(readValue(reader, "kickAnyWayRegion").ToString());
                NewActiveParameters.sweepZone = double.Parse(readValue(reader, "sweepZone").ToString());
                NewActiveParameters.minGoodness = double.Parse(readValue(reader, "minGoodness").ToString());
                NewActiveParameters.clearRobotZone = double.Parse(readValue(reader, "clearRobotZone").ToString());
                NewActiveParameters.kickAccuracy = double.Parse(readValue(reader, "kickAcuercy").ToString());

                reader.ReadEndElement();
                
                reader.ReadStartElement("Coefs");
                for (int i = 0; i < 6; i++)
                    for (int j = 0; j < 6; j++)
                        RobotMotionCoefs[i, j] = double.Parse(readValue(reader, string.Format("_{0}.{1}", i.ToString(), j.ToString())).ToString());
                reader.ReadEndElement();

                reader.ReadEndElement();
            }
        }
        #endregion
    }
}
