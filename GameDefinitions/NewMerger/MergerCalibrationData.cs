using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class MergerCalibrationData
    {
        Position2D realData = new Position2D();

        public Position2D RealData
        {
            get { return realData; }
            set { realData = value; }
        }
        Dictionary<int, Position2D> cameraData = new Dictionary<int, Position2D>();

        public Dictionary<int, Position2D> CameraData
        {
            get { return cameraData; }
            set { cameraData = value; }
        }

        public MergerCalibrationData()
        {
            realData = new Position2D();
            cameraData = new Dictionary<int, Position2D>();
        }

        public MergerCalibrationData(Position2D RealData)
        {
            realData = RealData;
        }

        public MergerCalibrationData(Position2D RealData, Dictionary<int, Position2D> CameraData)
        {
            realData = RealData;
            cameraData = CameraData;
        }

        public void AddCameraData(int cameraID, Position2D cameraPos)
        {
            cameraData.Add(cameraID, cameraPos);
        }
    }
}
