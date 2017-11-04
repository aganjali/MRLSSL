using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class MergerParameters
    {
        static List<MergerCalibrationData> mergerCalibData = new List<MergerCalibrationData>();
        static Dictionary<int, MathMatrix> coefMatrix = new Dictionary<int, MathMatrix>();
        public static Dictionary<int, MathMatrix> CoefMatrix
        {
            get { return MergerParameters.coefMatrix; }
            set { MergerParameters.coefMatrix = value; }
        }
        static Dictionary<int, MathMatrix> midCoefMat = new Dictionary<int, MathMatrix>();
        public static Dictionary<int, MathMatrix> MidCoefMat
        {
            get { return MergerParameters.midCoefMat; }
            set { MergerParameters.midCoefMat = value; }
        }

        static List<int> availableCamIds = new List<int>();

        public static List<int> AvailableCamIds
        {
            get { return MergerParameters.availableCamIds; }
            set { MergerParameters.availableCamIds = value; }
        }

        public MergerParameters()
        {
            Load();
        }

        public static List<MergerCalibrationData> MergerCalibData
        {
            get { return MergerParameters.mergerCalibData; }
            set { MergerParameters.mergerCalibData = value; }
        }

        public static void Save()
        {
            string fileName = "MergerCalibData.xml";
            fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Data");
            xmlDoc.AppendChild(rootNode);

            string namesToSave = "";
            foreach (var item in availableCamIds)
                namesToSave = namesToSave + item.ToString();

            XmlNode availableCamNode = xmlDoc.CreateElement("AvailableCam");
            XmlAttribute availableCamAttribute = xmlDoc.CreateAttribute("Names");
            availableCamAttribute.Value = namesToSave;
            availableCamNode.Attributes.Append(availableCamAttribute);
            rootNode.AppendChild(availableCamNode);

            foreach (var item in mergerCalibData)
            {
                XmlNode realDataNode = xmlDoc.CreateElement("RealData");
                XmlAttribute attribute = xmlDoc.CreateAttribute("X");
                attribute.Value = item.RealData.X.ToString();
                realDataNode.Attributes.Append(attribute);
                attribute = xmlDoc.CreateAttribute("Y");
                attribute.Value = item.RealData.Y.ToString();
                realDataNode.Attributes.Append(attribute);
                foreach (var cameraData in item.CameraData)
                {
                    XmlNode cameraDataNode = xmlDoc.CreateElement("CameraData");
                    XmlAttribute camAttribute = xmlDoc.CreateAttribute("cameraID");
                    camAttribute.Value = cameraData.Key.ToString();
                    cameraDataNode.Attributes.Append(camAttribute);
                    camAttribute = xmlDoc.CreateAttribute("cameraX");
                    camAttribute.Value = cameraData.Value.X.ToString();
                    cameraDataNode.Attributes.Append(camAttribute);
                    camAttribute = xmlDoc.CreateAttribute("cameraY");
                    camAttribute.Value = cameraData.Value.Y.ToString();
                    cameraDataNode.Attributes.Append(camAttribute);
                    realDataNode.AppendChild(cameraDataNode);
                }
                rootNode.AppendChild(realDataNode);
            }
            xmlDoc.Save(fileName);
        }

        public static void Load()
        {
            mergerCalibData = new List<MergerCalibrationData>();
            string fileName = "MergerCalibData.xml";
            fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            if (!System.IO.File.Exists(fileName))
            {
                Save();
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);

                XmlNode camNode = xmlDoc.SelectSingleNode("//Data/AvailableCam");
                List<char> names = camNode.Attributes["Names"].Value.ToList();
                availableCamIds = new List<int>();
                //foreach (var item in names)
                //{
                //    availableCamIds.Add(int.Parse(item.ToString()));
                //}

                for (int i = 0; i < 4; i++)
                {
                    availableCamIds.Add(i);
                }

                XmlNodeList realDataNodes = xmlDoc.SelectNodes("//Data/RealData");
                foreach (XmlNode realDataNode in realDataNodes)
                {
                    MergerCalibrationData MCD = new MergerCalibrationData();
                    MCD.RealData = new Position2D(double.Parse(realDataNode.Attributes["X"].Value), double.Parse(realDataNode.Attributes["Y"].Value));
                    XmlNodeList cameraDataNodes = realDataNode.SelectNodes("CameraData");
                    foreach (XmlNode cameraDataNode in cameraDataNodes)
                    {
                        MCD.AddCameraData(int.Parse(cameraDataNode.Attributes["cameraID"].Value), new Position2D(double.Parse(cameraDataNode.Attributes["cameraX"].Value), double.Parse(cameraDataNode.Attributes["cameraY"].Value)));
                    }
                    mergerCalibData.Add(MCD);
                }
                if (mergerCalibData.Count > 0)
                {
                    SetMatrix();
                }
            }

        }

        public static void SetMatrix()
        {
            coefMatrix = new Dictionary<int, MathMatrix>();
            Dictionary<int, MathMatrix> cams = new Dictionary<int, MathMatrix>();
            Dictionary<int, MathMatrix> reals = new Dictionary<int, MathMatrix>();
            List<Position2D> tempCam = new List<Position2D>();
            List<Position2D> tempReal = new List<Position2D>();
            List<int> idx = new List<int> { 0, 1, 2, 3, 10, 20, 31, 32 };

            for (int i = 0; i < 4; i++)
            {
                foreach (var item in mergerCalibData)
                {
                    if (item.CameraData.Keys.Contains(i))
                    {
                        tempCam.Add(item.CameraData[i]);
                        tempReal.Add(item.RealData);
                    }
                }
                MathMatrix tempRealMat = new MathMatrix(tempReal.Count, 3);
                MathMatrix tempCamMat = new MathMatrix(tempCam.Count, 3);
                for (int j = 0; j < tempReal.Count; j++)
                {
                    tempCamMat[j, 0] = tempCam[j].X;
                    tempCamMat[j, 1] = tempCam[j].Y;
                    tempCamMat[j, 2] = 1;
                    tempRealMat[j, 0] = tempReal[j].X;
                    tempRealMat[j, 1] = tempReal[j].Y;
                    tempRealMat[j, 2] = 1;
                }
                cams.Add(i, tempCamMat);
                reals.Add(i, tempRealMat);
                tempCam = new List<Position2D>();
                tempReal = new List<Position2D>();
            }

            int count10 = 0;
            int count20 = 0;
            int count31 = 0;
            int count32 = 0;
            foreach (var item in mergerCalibData)
            {
                if (item.CameraData.Keys.Contains(1) && item.CameraData.Keys.Contains(0))
                {
                    count10++;
                }
                if (item.CameraData.Keys.Contains(2) && item.CameraData.Keys.Contains(0))
                {
                    count20++;
                }
                if (item.CameraData.Keys.Contains(3) && item.CameraData.Keys.Contains(1))
                {
                    count31++;
                }
                if (item.CameraData.Keys.Contains(3) && item.CameraData.Keys.Contains(2))
                {
                    count32++;
                }
            }

            MathMatrix cam10 = new MathMatrix(count10, 5);
            MathMatrix real10 = new MathMatrix(count10, 3);
            MathMatrix cam20 = new MathMatrix(count20, 5);
            MathMatrix real20 = new MathMatrix(count20, 3);
            MathMatrix cam31 = new MathMatrix(count31, 5);
            MathMatrix real31 = new MathMatrix(count31, 3);
            MathMatrix cam32 = new MathMatrix(count32, 5);
            MathMatrix real32 = new MathMatrix(count32, 3);
            count10 = 0;
            count20 = 0;
            count31 = 0;
            count32 = 0;

            foreach (var item in mergerCalibData)
            {
                if (item.CameraData.Keys.Contains(1) && item.CameraData.Keys.Contains(0))
                {
                    cam10[count10, 0] = item.CameraData[1].X;
                    cam10[count10, 1] = item.CameraData[1].Y;
                    cam10[count10, 2] = item.CameraData[0].X;
                    cam10[count10, 3] = item.CameraData[0].Y;
                    cam10[count10, 4] = 1;

                    real10[count10, 0] = item.RealData.X;
                    real10[count10, 1] = item.RealData.Y;
                    real10[count10, 2] = 1;
                    count10++;
                }
                if (item.CameraData.Keys.Contains(2) && item.CameraData.Keys.Contains(0))
                {
                    cam20[count20, 0] = item.CameraData[2].X;
                    cam20[count20, 1] = item.CameraData[2].Y;
                    cam20[count20, 2] = item.CameraData[0].X;
                    cam20[count20, 3] = item.CameraData[0].Y;
                    cam20[count20, 4] = 1;

                    real20[count20, 0] = item.RealData.X;
                    real20[count20, 1] = item.RealData.Y;
                    real20[count20, 2] = 1;
                    count20++;
                }
                if (item.CameraData.Keys.Contains(3) && item.CameraData.Keys.Contains(1))
                {
                    cam31[count31, 0] = item.CameraData[3].X;
                    cam31[count31, 1] = item.CameraData[3].Y;
                    cam31[count31, 2] = item.CameraData[1].X;
                    cam31[count31, 3] = item.CameraData[1].Y;
                    cam31[count31, 4] = 1;

                    real31[count31, 0] = item.RealData.X;
                    real31[count31, 1] = item.RealData.Y;
                    real31[count31, 2] = 1;
                    count31++;
                }
                if (item.CameraData.Keys.Contains(3) && item.CameraData.Keys.Contains(2))
                {
                    cam32[count32, 0] = item.CameraData[3].X;
                    cam32[count32, 1] = item.CameraData[3].Y;
                    cam32[count32, 2] = item.CameraData[2].X;
                    cam32[count32, 3] = item.CameraData[2].Y;
                    cam32[count32, 4] = 1;

                    real32[count32, 0] = item.RealData.X;
                    real32[count32, 1] = item.RealData.Y;
                    real32[count32, 2] = 1;
                    count32++;
                }
            }

            cams.Add(10, cam10);
            reals.Add(10, real10);
            cams.Add(20, cam20);
            reals.Add(20, real20);
            cams.Add(31, cam31);
            reals.Add(31, real31);
            cams.Add(32, cam32);
            reals.Add(32, real32);

            foreach (var item in idx)
            {
                MathMatrix cam = cams[item].Transpose;
                MathMatrix real = reals[item].Transpose;
                MathMatrix a = real * cam.Transpose * Inverse.invert(cam * cam.Transpose);
                coefMatrix.Add(item, a);
            }

            Dictionary<int, Position2D> camera = mergerCalibData.Where(t => t.RealData.X == 0 && t.RealData.Y == 0).FirstOrDefault().CameraData;
            List<Position2D> temp = new List<Position2D>();

            temp.Add(camera[2]);
            temp.Add(camera[1]);
            coefMatrix.Add(21, centerMat(temp));
            temp = new List<Position2D>();
            temp.Add(camera[3]);
            temp.Add(camera[0]);
            coefMatrix.Add(30, centerMat(temp));
            temp = new List<Position2D>();
            temp.Add(camera[0]);
            temp.Add(camera[1]);
            temp.Add(camera[3]);
            coefMatrix.Add(310, centerMat(temp));
            temp = new List<Position2D>();
            temp.Add(camera[1]);
            temp.Add(camera[3]);
            temp.Add(camera[2]);
            coefMatrix.Add(321, centerMat(temp));
            temp = new List<Position2D>();
            temp.Add(camera[3]);
            temp.Add(camera[2]);
            temp.Add(camera[0]);
            coefMatrix.Add(320, centerMat(temp));
            temp = new List<Position2D>();
            temp.Add(camera[2]);
            temp.Add(camera[0]);
            temp.Add(camera[1]);
            coefMatrix.Add(210, centerMat(temp));
            temp = new List<Position2D>();
            temp.Add(camera[3]);
            temp.Add(camera[2]);
            temp.Add(camera[1]);
            temp.Add(camera[0]);
            coefMatrix.Add(3210, centerMat(temp));

        }

        static MathMatrix centerMat(List<Position2D> cam)
        {
            MathMatrix ret = new MathMatrix(0, 0);
            if (cam.Count == 2)
            {
                ret = new MathMatrix(3, 5);
                ret[0, 0] = 0.5;
                ret[0, 1] = 0;
                ret[0, 2] = 0.5;
                ret[0, 3] = 0;
                ret[0, 4] = (-cam[0].X - cam[1].X) / 2;
                ret[1, 0] = 0;
                ret[1, 1] = 0.5;
                ret[1, 2] = 0;
                ret[1, 3] = 0.5;
                ret[1, 4] = (-cam[0].Y - cam[1].Y) / 2;
                ret[2, 0] = 0;
                ret[2, 1] = 0;
                ret[2, 2] = 0;
                ret[2, 3] = 0;
                ret[2, 4] = 1;
            }
            else if (cam.Count == 3)
            {
                ret = new MathMatrix(3, 7);
                ret[0, 0] = 0.3;
                ret[0, 1] = 0;
                ret[0, 2] = 0.3;
                ret[0, 3] = 0;
                ret[0, 4] = 0.3;
                ret[0, 5] = 0;
                ret[0, 6] = (-cam[0].X - cam[1].X - cam[2].X) / 3;
                ret[1, 0] = 0;
                ret[1, 1] = 0.3;
                ret[1, 2] = 0;
                ret[1, 3] = 0.3;
                ret[1, 4] = 0;
                ret[1, 5] = 0.3;
                ret[1, 6] = (-cam[0].Y - cam[1].Y - cam[2].Y) / 3;
                ret[2, 0] = 0;
                ret[2, 1] = 0;
                ret[2, 2] = 0;
                ret[2, 3] = 0;
                ret[2, 4] = 0;
                ret[2, 5] = 0;
                ret[2, 6] = 1;
            }
            else if (cam.Count == 4)
            {
                ret = new MathMatrix(3, 9);
                ret[0, 0] = 0.25;
                ret[0, 1] = 0;
                ret[0, 2] = 0.25;
                ret[0, 3] = 0;
                ret[0, 4] = 0.25;
                ret[0, 5] = 0;
                ret[0, 6] = 0.25;
                ret[0, 7] = 0;
                ret[0, 8] = (-cam[0].X - cam[1].X - cam[2].X - cam[3].X) / 4;
                ret[1, 0] = 0;
                ret[1, 1] = 0.25;
                ret[1, 2] = 0;
                ret[1, 3] = 0.25;
                ret[1, 4] = 0;
                ret[1, 5] = 0.25;
                ret[1, 6] = 0;
                ret[1, 7] = 0.25;
                ret[1, 8] = (-cam[0].Y - cam[1].Y - cam[2].Y - cam[3].Y) / 4;
                ret[2, 0] = 0;
                ret[2, 1] = 0;
                ret[2, 2] = 0;
                ret[2, 3] = 0;
                ret[2, 4] = 0;
                ret[2, 5] = 0;
                ret[2, 6] = 0;
                ret[2, 7] = 0;
                ret[2, 8] = 1;
            }
            return ret;
        }

        public static MathMatrix GetMatrix(List<int> ids)
        {
            ids = ids.OrderByDescending(p => p).ToList();
            int key = 0;
            foreach (var item in ids)
            {
                key *= 10;
                key += item;
            }
            if (ids.Count == 0)
                key = -1;
            //DrawingObjects.AddObject(new StringDraw(key.ToString(), new Position2D(-1,0)), "dsfdfs");
            if (coefMatrix.ContainsKey(key))
            {
                return coefMatrix[key];
            }
            return null;
        }

        public static bool commonData(Dictionary<Position2D, Position2D> firstCam, Dictionary<Position2D, Position2D> secondCam, ref MathMatrix mixMatrix, ref MathMatrix realMatrix)
        {
            int count = 0;
            bool centerFlag = false;
            foreach (var cam1 in firstCam)
            {
                if (!((cam1.Key.X == 0) && (cam1.Key.Y == 0)))
                    centerFlag = true;
                foreach (var cam2 in secondCam)
                {
                    if ((cam1.Key.X == cam2.Key.X) && (cam1.Key.Y == cam2.Key.Y))
                    {
                        count++;
                    }
                }

            }
            if ((centerFlag && count == 1) || (count == 0))
            {
                return false;
            }
            mixMatrix = new MathMatrix(count, 5);
            realMatrix = new MathMatrix(count, 3);
            count = 0;
            foreach (var cam1 in firstCam)
            {
                foreach (var cam2 in secondCam)
                {
                    if ((cam1.Key.X == cam2.Key.X) && (cam1.Key.Y == cam2.Key.Y))
                    {
                        mixMatrix[count, 0] = cam1.Value.X;
                        mixMatrix[count, 1] = cam1.Value.Y;
                        mixMatrix[count, 2] = cam2.Value.X;
                        mixMatrix[count, 3] = cam2.Value.Y;
                        mixMatrix[count, 4] = 1;
                        realMatrix[count, 0] = cam1.Key.X;
                        realMatrix[count, 1] = cam1.Key.Y;
                        realMatrix[count, 2] = 1;
                        count++;
                    }
                }
            }

            return true;
        }

        public static Dictionary<int, MathMatrix> CoefMat { get; set; }

        public static Dictionary<int, MathMatrix> coefMat { get; set; }
    }
}
