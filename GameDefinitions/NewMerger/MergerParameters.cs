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
        static Dictionary<int, MathMatrix>   coefMatrix = new Dictionary<int, MathMatrix>();
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

                for (int i = 0; i < StaticVariables.CameraCount; i++)
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
        private static int cameraId2KeyByOrder(params int[] camIds) {
            if (camIds.Length == 0)
                return -1;
            int res = 0;
            foreach (var item in camIds)
            {
                res = res * 10 + item;
            }
            return res;
        }
        public static void SetMatrix()
        {
            coefMatrix = new Dictionary<int, MathMatrix>();
            Dictionary<int, MathMatrix> cams = new Dictionary<int, MathMatrix>();
            Dictionary<int, MathMatrix> reals = new Dictionary<int, MathMatrix>();
            List<Position2D> tempCam = new List<Position2D>();
            List<Position2D> tempReal = new List<Position2D>();
            List<int> cameraIds = new List<int> ();

            for (int i = 0; i < StaticVariables.CameraCount; i++)
            {
                cameraIds.Add(i);
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
            for (int i = 0; i < StaticVariables.CameraCount - 1; i++)
            {
                List<int> neighbors = new List<int>();
                if (i % 2 == 0)
                {
                    neighbors.Add(i + 1);
                }
                if (i + 2 < StaticVariables.CameraCount)
                {
                    neighbors.Add(i + 2);
                }
                foreach (var j in neighbors)
                {
                    var data = mergerCalibData.Where(w => w.CameraData.Keys.Contains(i) && w.CameraData.Keys.Contains(j));
                    int size = data.Count();
                    MathMatrix c = new MathMatrix(size, 5);
                    MathMatrix r = new MathMatrix(size, 3);
                    int count = 0;
                    foreach (var item in data)
                    {
                        c[count, 0] = item.CameraData[j].X;
                        c[count, 1] = item.CameraData[j].Y;
                        c[count, 2] = item.CameraData[i].X;
                        c[count, 3] = item.CameraData[i].Y;
                        c[count, 4] = 1;
                          
                        r[count, 0] = item.RealData.X;
                        r[count, 1] = item.RealData.Y;
                        r[count, 2] = 1;
                        count++;
                    }
                    int id = cameraId2KeyByOrder(j, i);
                    cams.Add(id, c);
                    reals.Add(id, r);
                    cameraIds.Add(id);
                }
            }


            foreach (var item in cameraIds)
            {
                MathMatrix cam = cams[item].Transpose;
                MathMatrix real = reals[item].Transpose;
                MathMatrix a = real * cam.Transpose * Inverse.invert(cam * cam.Transpose);
                coefMatrix.Add(item, a);
            }
            for (int i = 0; i  < StaticVariables.CameraCount - 3; i += 2)
            {
                Dictionary<int, Position2D> camera = mergerCalibData.Where(t => t.CameraData.Count == 4 
                    && t.CameraData.ContainsKey(i) &&  t.CameraData.ContainsKey(i + 2)).FirstOrDefault().CameraData;
                List<Position2D> temp = new List<Position2D>();
                int cameraID = -1;
                
                temp.Add(camera[i + 2]);
                temp.Add(camera[i + 1]);

                cameraID = cameraId2KeyByOrder(i + 2, i + 1);
                coefMatrix.Add(cameraID, centerMat(temp));

                temp = new List<Position2D>();
                temp.Add(camera[i + 3]);
                temp.Add(camera[i]);

                cameraID = cameraId2KeyByOrder(i + 3, i);
                coefMatrix.Add(cameraID, centerMat(temp));
                
              
                temp = new List<Position2D>();
                temp.Add(camera[i]);
                temp.Add(camera[i + 1]);
                temp.Add(camera[i + 3]);

                cameraID = cameraId2KeyByOrder(i + 3, i + 1, i);
                coefMatrix.Add(cameraID, centerMat(temp));
                
                temp = new List<Position2D>();
                temp.Add(camera[i + 1]);
                temp.Add(camera[i + 3]);
                temp.Add(camera[i + 2]);

                cameraID = cameraId2KeyByOrder(i + 3, i + 2, i + 1);
                coefMatrix.Add(cameraID, centerMat(temp));
                
                temp = new List<Position2D>();
                temp.Add(camera[i + 3]);
                temp.Add(camera[i + 2]);
                temp.Add(camera[i]);

                cameraID = cameraId2KeyByOrder(i + 3, i + 2, i);
                coefMatrix.Add(cameraID, centerMat(temp));
                
                temp = new List<Position2D>();
                temp.Add(camera[i + 2]);
                temp.Add(camera[i]);
                temp.Add(camera[i + 1]);

                cameraID = cameraId2KeyByOrder(i + 2, i + 1, i);
                coefMatrix.Add(cameraID, centerMat(temp));
                
                temp = new List<Position2D>();
                temp.Add(camera[i + 3]);
                temp.Add(camera[i + 2]);
                temp.Add(camera[i + 1]);
                temp.Add(camera[i]);

                cameraID = cameraId2KeyByOrder(i + 3, i + 2, i + 1, i);
                coefMatrix.Add(cameraID, centerMat(temp));
            }
            
            

           
            

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
                ret[0, 0] = 0.3333;
                ret[0, 1] = 0;
                ret[0, 2] = 0.3333;
                ret[0, 3] = 0;
                ret[0, 4] = 0.3333;
                ret[0, 5] = 0;
                ret[0, 6] = (-cam[0].X - cam[1].X - cam[2].X) / 3;
                ret[1, 0] = 0;
                ret[1, 1] = 0.3333;
                ret[1, 2] = 0;
                ret[1, 3] = 0.333;
                ret[1, 4] = 0;
                ret[1, 5] = 0.333;
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
