using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MRL.SSL.GameDefinitions
{
    public class RobotData
    {
        public double vx = 0;
        public double vy = 0;
        public double w = 0;
    }
    public static class GlobalOptimizationCommands
    {
        public static bool Calculating = false;
        public static bool Initialized = false;
        public static List<RobotData> visionData = new List<RobotData>();
        public static List<RobotData> idealData = new List<RobotData>();
        
        public static MathMatrix A_old = new MathMatrix(3, 3);
        public static MathMatrix delta = new MathMatrix(3, 3);
        public static MathMatrix Anew = new MathMatrix(3, 3);
        

        private static bool saveto = false;
        private static MathMatrix visionVector, idealVector, visionMatrix, idealMatrix;
        private static MathMatrix errorMatrix;

        public static void Initialize()
        {
            if (!Initialized)
            {
                A_old = MathMatrix.IdentityMatrix(3, 3);
                Anew = A_old;
                Initialized = true;
            }
        }

        public static void Calculate()
        {
            if (visionData.Count < 2 || idealData.Count <2)
                return;
            Calculating = true;
            Initialize();
         
            visionMatrix = new MathMatrix(3, visionData.Count);
            idealMatrix = new MathMatrix(3, idealData.Count);
            for (int i = 0; i < visionData.ToList().Count; i++)
            {
                //visionVector[i, 0] = visionData[i].vx;
                //visionVector[visionData.Count+i, 0] = visionData[i].vy;
                //visionVector[2*visionData.Count+i, 0] = visionData[i].w;

                visionMatrix[0, i] = visionData[i].vx;
                visionMatrix[1, i] = visionData[i].vy;
                visionMatrix[2, i] = visionData[i].w;

                //idealVector[i, 0] = idealData[i].vx;
                //idealVector[idealData.Count+i, 0] = idealData[i].vy;
                //idealVector[2*idealData.Count+i, 0] = idealData[i].w;
              
                idealMatrix[0, i] = idealData[i].vx;
                idealMatrix[1, i] = idealData[i].vy;
                idealMatrix[2, i] = idealData[i].w;
            }


            errorMatrix = idealMatrix - visionMatrix;

            MathMatrix amghezi = MathMatrix.IdentityMatrix(3, 3) & visionMatrix.Transpose;
            MathMatrix amgheziT = amghezi.Transpose;
            MathMatrix amgheziTam = amgheziT * amghezi;
            if (Math.Abs(Det.det(amgheziTam)) < 0.001)
                delta = MathMatrix.NullMatrix(3, 3);
            else
            {
                MathMatrix amghezi2 = Inverse.invert(amgheziTam) * amgheziT * errorMatrix.Transpose.Vectorize;
                MathMatrix amghezi3 = new MathMatrix(3, 3);
                for (int i = 0; i < amghezi2.Rows; i++)
                    amghezi3[i % 3, (int)(i / 3)] = amghezi2[i, 0];
                delta = amghezi3.Transpose;
            }
            Anew = A_old * (MathMatrix.IdentityMatrix(3, 3) + delta);
            A_old = Anew;
            if (saveto)
                SaveDataToFile();
            idealData.Clear();
            visionData.Clear();

            Calculating = false;
        }


        private static void SaveDataToFile()
        {
            //---------------
            //MemoryStream mstime = new MemoryStream();
            //StreamWriter swtime = new StreamWriter(mstime);
            //swtime.WriteLine("vx\tvy\tw");
            //foreach (var item in visionData.ToList())
            //{
            //    swtime.Write(item.vx+ "\t");
            //    swtime.Write(item.vy + "\t");
            //    swtime.WriteLine(item.w);
            //}
            //FileStream fstime = new FileStream(@"d:\AdhamiData\visiondata.txt", FileMode.Create);
            //fstime.Write(mstime.ToArray(), 0, (int)mstime.Length);
            //fstime.Close();
            //swtime.Close();

            ////---------------
            //mstime = new MemoryStream();
            //swtime = new StreamWriter(mstime);
            //swtime.WriteLine("vx\tvy\tw");
            //foreach (var item in idealData.ToList())
            //{
            //    swtime.Write(item.vx + "\t");
            //    swtime.Write(item.vy + "\t");
            //    swtime.WriteLine(item.w);
            //}
            //fstime = new FileStream(@"d:\AdhamiData\idealdata.txt", FileMode.Create);
            //fstime.Write(mstime.ToArray(), 0, (int)mstime.Length);
            //fstime.Close();
            //swtime.Close();

            //----------------
            MemoryStream vismat = new MemoryStream();
            StreamWriter swvismat = new StreamWriter(vismat);
            for (int i = 0; i < visionMatrix.Rows; i++)
            {
                for (int j = 0; j < visionMatrix.Cols; j++)
                {
                    swvismat.Write(visionMatrix[i, j].ToString("f5") + "\t");
                    swvismat.Flush();
                }
                swvismat.Write("\n");
            }
            FileStream fsvismat = new FileStream(@"d:\AdhamiData\visionMatrix.txt", FileMode.Create);
            fsvismat.Write(vismat.ToArray(), 0, (int)vismat.Length);
            swvismat.Flush();
            fsvismat.Close();
            swvismat.Close();

            //----------------
            MemoryStream msimat = new MemoryStream();
            StreamWriter swimat = new StreamWriter(msimat);
            for (int i = 0; i < idealMatrix.Rows; i++)
            {
                for (int j = 0; j < idealMatrix.Cols; j++)
                {
                    swimat.Write(idealMatrix[i, j].ToString("f5") + "\t");
                    swimat.Flush();
                }
                swimat.Write("\n");
            }
            FileStream fstime = new FileStream(@"d:\AdhamiData\idealMatrix.txt", FileMode.Create);
            fstime.Write(msimat.ToArray(), 0, (int)msimat.Length);
            swimat.Flush();
            swimat.Close();
            fstime.Close();
            //---------------
            //mstime = new MemoryStream();
            //swtime = new StreamWriter(mstime);
            //swtime.Write("---Dnew---\n");
            //for (int i = 0; i < Dnew.Rows; i++)
            //{
            //    for (int j = 0; j < Dnew.Cols; j++)
            //    {
            //        swtime.Write(Dnew[i, j].ToString() + "\t");
            //    }
            //    swtime.Write("\n");
            //}
            //swtime.Write("---delta_Dnew_invers---\n");
            //for (int i = 0; i < delta_Dnew_invers.Rows; i++)
            //{
            //    for (int j = 0; j < delta_Dnew_invers.Cols; j++)
            //    {
            //        swtime.Write(delta_Dnew_invers[i, j].ToString() + "\t");
            //    }
            //    swtime.Write("\n");
            //}
            //fstime = new FileStream(@"d:\AdhamiData\result.txt", FileMode.Create);
            //fstime.Write(mstime.ToArray(), 0, (int)mstime.Length);
            //fstime.Close();
            //swtime.Close();

        }
    }
}
