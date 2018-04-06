using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.IO;

namespace MRL.SSL.Planning.GPUDirect
{
    public static class GPPlanner
    {
        [DllImport("GPUPlanner.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Start(float maxRobotAccel, float maxRobotSpeed, float ballDecel, int maxPathCount, int maxRRTCount, int maxRobotCount, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] DataScoreX, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] DataScoreY, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] eachRegionCount, int RegionCount, int maxSampleCount, float sigmaX, float sigmaY);

        [DllImport("GPUPlanner.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShutDown();

        [DllImport("GPUPlanner.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern float ForceTree([MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] Path, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)]int[] eachPathCount,
            int RobotCount, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] avoid, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] finalPath, int SmoothingCount,
            [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] Obstacles, int ObstacleCount, float Kspring, float Kspring2, int n, int stopBall);

        [DllImport("GPUPlanner.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GPlannerBallState([MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] robots, int RobotCounts, int N, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] ball, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] Heads, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)]float[] Tails, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] TimeHeads, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] TimeTails, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)]int[] histo);

        [DllImport("GPUPlanner.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GPlannerScore([MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] Robots, int RobotCount, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] Phi, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] Kdx, [MarshalAsAttribute(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)]float[] Kdy);

        private static double sigmax;
        public static double SigmaX
        {
            get { return GPPlanner.sigmax; }
            set { GPPlanner.sigmax = value; }
        }

        private static double sigmay;
        public static double SigmaY
        {
            get { return GPPlanner.sigmay; }
            set { GPPlanner.sigmay = value; }
        }

        private static double sigmaxt;
        public static double SigmaXt
        {
            get { return GPPlanner.sigmaxt; }
            set { GPPlanner.sigmaxt = value; }
        }
        
        private static double sigmayt;
        public static double SigmaYt
        {
            get { return GPPlanner.sigmayt; }
            set { GPPlanner.sigmayt = value; }
        }

        private static Dictionary<int, List<Score>> data = new Dictionary<int, List<Score>>();

        public static Dictionary<int, List<Score>> Data
        {
            get { return GPPlanner.data; }
            set { GPPlanner.data = value; }
        }
        private static int maxCount = int.MinValue;
        private static float[] eachDataCount;
        private static float[] DataX, DataY;

        public static void Initilize()
        {
            ReadScoreSamples();
            Start(GPUParams.maxRobotAccel, GPUParams.maxRobotSpeed, GPUParams.ballDecel, GPUParams.maxPathCount, GPUParams.maxRRTCount, GPUParams.maxRobotCount, DataX, DataY, eachDataCount, data.Count, maxCount, (float)sigmax, (float)sigmay);
        }
        public static void ReadScoreSamples()
        {
            //RegionScore.Load("ALL.txt");
            //sigmax = RegionScore.Default.SigmaX;
            //sigmay = RegionScore.Default.SigmaY;
            //data = new Dictionary<int, List<Score>>();

            RegionScore.Load("ALL.txt");
            sigmax = RegionScore.Default.SigmaX;
            sigmay = RegionScore.Default.SigmaY;
            data = new Dictionary<int, List<Score>>();
            #region load new scoring
            string scorePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Score.txt");
            List<Score> scoreList = new List<Score>();
            if (File.Exists(scorePath))
            {
                int counter = 0;
                string line;

                // Read the file and display it line by line.
                System.IO.StreamReader file = new System.IO.StreamReader(scorePath);
                while ((line = file.ReadLine()) != null)
                {
                    string[] bits = line.Split('\t');
                    if (counter > 0)
                    {
                        Score s = new Score();
                        s.Robot = new Position2D(double.Parse(bits[0]), double.Parse(bits[1]));
                        s.PosScore = double.Parse(bits[2]);
                        s.Region = int.Parse(bits[3]);
                        scoreList.Add(s);
                    }
                    counter++;
                }
                file.Close();
            }
            if (scoreList.Count > 0)
            {
                RegionScore.Default.Data = scoreList;
            }
            #endregion

            RegionScore.Default.Data.ForEach(f => data[f.Region] = RegionScore.Default.Data.Where(w => w.Region == f.Region).ToList());

            foreach (var item in data)
            {
                if (maxCount < item.Value.Count)
                    maxCount = item.Value.Count;
            }
            maxCount = Math.Max(maxCount, 1);

            DataX = new float[data.Count * maxCount];
            DataY = new float[data.Count * maxCount];
            eachDataCount = new float[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                eachDataCount[i] = data[i + 1].Count;
                int j = 0;
                foreach (var item in data[i + 1])
                {
                    DataX[i * maxCount + j] = (float)item.Robot.X;
                    DataY[i * maxCount + j] = (float)item.Robot.Y;
                    j++;
                }
            }
            sigmaxt = RegionScore.Default.SigmaXt;
            sigmayt = RegionScore.Default.SigmaYt;
        }
    }
}
