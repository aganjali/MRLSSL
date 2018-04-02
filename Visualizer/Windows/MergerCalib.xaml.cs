using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Visualizer.Classes;
using System.Reflection;
using Enterprise;
using MRL.SSL.GameDefinitions.Visualizer_Classes;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for MergerCalib.xaml
    /// </summary>
    public partial class MergerCalib : Window
    {
        List<VisionDataTemplate> listToView = new List<VisionDataTemplate>();
        string namesToShow = "";

        Dictionary<int, MergerCalibrationData> points = new Dictionary<int, MergerCalibrationData>();
        Dictionary<int, Position2D> pointsToDraw = new Dictionary<int, Position2D>();
        int? selectedPosKey = null;
        static bool getVision = false;
        public MergerCalib()
        {
            InitializeComponent();
            mainField.MouseMove += new System.Windows.Forms.MouseEventHandler(mainField_MouseMove);
            mainField.MergerCalibMode = true;
            mainField.CurrentWrapper = new AiToVisualizerWrapper();
            mainField.Model = new WorldModel();
            mainField.Model.OurRobots = new Dictionary<int, SingleObjectState>();
            mainField.Model.Opponents = new Dictionary<int, SingleObjectState>();
            mainField.Model.BallState = new SingleObjectState();
            mainField.CurrentWrapper.AllBalls = new Dictionary<int, Position2D>();
            mainField.MouseClick += new System.Windows.Forms.MouseEventHandler(mainField_MouseClick);
            DataReciever.PacketRecieved += new DataReciever.PacketRecievedEventHandler(DataReciever_PacketRecieved);
            Load();
        }
        #region NewMethod
        void Load()
        {
            int count = 0;
            MergerParameters.Load();
            points = new Dictionary<int, MergerCalibrationData>();
            foreach (var item in MergerParameters.MergerCalibData)
            {
                points.Add(count, item);
                count++;
            }
            Load_old();
            UIRefresh();
        }

        void MainFieldRefresh()
        {
            pointsToDraw = points.ToDictionary(t => t.Key, t => new Position2D(t.Value.RealData.X / 1000, t.Value.RealData.Y / 1000));
            mainField.MergerPoint = pointsToDraw;
            if (selectedPosKey.HasValue)
            {
                mainField.DrawCircle(new Circle(pointsToDraw[selectedPosKey.Value], 0.05, new System.Drawing.Pen(System.Drawing.Color.Red, 0.05f)));
            }
            else
            {
                mainField.DrawCircle(new Circle(new Position2D(100, 100), 0.001));
            }
        }

        void UIRefresh()
        {
            MainFieldRefresh();
            txtRealX.Text = "";
            txtRealY.Text = "";
            txtCam0XStat.Text = "";
            txtCam0YStat.Text = "";
            txtCam1XStat.Text = "";
            txtCam1YStat.Text = "";
            txtCam2XStat.Text = "";
            txtCam2YStat.Text = "";
            txtCam3XStat.Text = "";
            txtCam3YStat.Text = "";
            if (selectedPosKey.HasValue)
            {
                txtRealX.Text = points[selectedPosKey.Value].RealData.X.ToString();
                txtRealY.Text = points[selectedPosKey.Value].RealData.Y.ToString();
                foreach (var item in points[selectedPosKey.Value].CameraData)
                {
                    switch (item.Key)
                    {
                        case 0:
                            txtCam0XStat.Text = item.Value.X.ToString();
                            txtCam0YStat.Text = item.Value.Y.ToString();
                            break;
                        case 1:
                            txtCam1XStat.Text = item.Value.X.ToString();
                            txtCam1YStat.Text = item.Value.Y.ToString();
                            break;
                        case 2:
                            txtCam2XStat.Text = item.Value.X.ToString();
                            txtCam2YStat.Text = item.Value.Y.ToString();
                            break;
                        case 3:
                            txtCam3XStat.Text = item.Value.X.ToString();
                            txtCam3YStat.Text = item.Value.Y.ToString();
                            break;
                    }
                }

            }
        }

        void mainField_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Position2D mousePos = PixelToMetric(e.Location);
            lbl1.Content = mousePos.toString();
        }

        void mainField_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Position2D mousePos = PixelToMetric(e.Location);
                selectedPosKey = SelectedPos(mousePos);
                if (selectedPosKey.HasValue)
                {
                    UIRefresh();
                }
            }
        }

        void DataReciever_PacketRecieved(object sender, messages_robocup_ssl_wrapper.SSL_WrapperPacket Packet)
        {
            if (getVision && selectedPosKey != null)
            {
                try
                {
                    if (Packet.detection.balls.Count > 0)
                    {
                        Dispatcher.Invoke((Action)(() =>
                                {
                                    switch ((int)Packet.detection.camera_id)
                                    {
                                        case 0:
                                            txtCam0X.Text = Packet.detection.balls[0].x.ToString();
                                            txtCam0Y.Text = Packet.detection.balls[0].y.ToString();
                                            break;
                                        case 1:
                                            txtCam1X.Text = Packet.detection.balls[0].x.ToString();
                                            txtCam1Y.Text = Packet.detection.balls[0].y.ToString();
                                            break;
                                        case 2:
                                            txtCam2X.Text = Packet.detection.balls[0].x.ToString();
                                            txtCam2Y.Text = Packet.detection.balls[0].y.ToString();
                                            break;
                                        case 3:
                                            txtCam3X.Text = Packet.detection.balls[0].x.ToString();
                                            txtCam3Y.Text = Packet.detection.balls[0].y.ToString();
                                            break;
                                    }
                                }));
                        mainField.CurrentWrapper = new AiToVisualizerWrapper() { AllBalls = new Dictionary<int, Position2D>() { { 0, new Position2D { X = Packet.detection.balls[0].x / 1000, Y = Packet.detection.balls[0].y / 1000 } } } };
                    }
                }
                catch (TargetInvocationException ex)
                {
                    Logger.WriteError("----Packet Reciver For Merger Reciever----\n" + ex.ToString());
                }
            }
        }

        int? SelectedPos(Position2D p)
        {
            int? ret = null;
            if (pointsToDraw != null)
            {
                foreach (int key in pointsToDraw.Keys)
                {
                    Position2D loc = pointsToDraw[key];
                    if (Math.Pow(p.X - loc.X, 2) + Math.Pow(p.Y - loc.Y, 2) < Math.Pow(GameParameters.BallDiameter / 2, 2))
                        ret = key;
                }
            }
            return ret;
        }

        private Position2D PixelToMetric(System.Drawing.Point MouseLocation)
        {
            if (mainField.Transform == null) return new Position2D();
            double X = MouseLocation.X;
            double Y = MouseLocation.Y;
            X = (X - mainField.Transform.Value.M31) / mainField.Transform.Value.M21;
            Y = (Y - mainField.Transform.Value.M32) / mainField.Transform.Value.M12;
            return new Position2D(Y, X);
        }

        private void SendData()
        {
            DataSender.CurrentWrapper.SendData.Add("FourCamMergerSetting");
            DataSender.SendOn.Set();
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedPosKey.HasValue)
            {
                MessageBox.Show("Please Select Ball");
            }
            else
            {
                Dictionary<int, Position2D> tempCam = new Dictionary<int, Position2D>();
                bool flag = false;
                if (txtCam0X.Text != "" && txtCam0Y.Text != "")
                {
                    tempCam.Add(0, new Position2D(double.Parse(txtCam0X.Text), double.Parse(txtCam0Y.Text)));
                    flag = true;
                }
                if (txtCam1X.Text != "" && txtCam1Y.Text != "")
                {
                    tempCam.Add(1, new Position2D(double.Parse(txtCam1X.Text), double.Parse(txtCam1Y.Text)));
                    flag = true;
                }
                if (txtCam2X.Text != "" && txtCam2Y.Text != "")
                {
                    tempCam.Add(2, new Position2D(double.Parse(txtCam2X.Text), double.Parse(txtCam2Y.Text)));
                    flag = true;
                }
                if (txtCam3X.Text != "" && txtCam3Y.Text != "")
                {
                    tempCam.Add(3, new Position2D(double.Parse(txtCam3X.Text), double.Parse(txtCam3Y.Text)));
                    flag = true;
                }
                if (!flag)
                {
                    MessageBox.Show("Camera Data Sets are Empty!!");
                }
                else
                    points[selectedPosKey.Value].CameraData = tempCam;
                UIRefresh();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtRealX.Text != "" && txtRealY.Text != "")
            {
                double x = double.Parse(txtRealX.Text);
                double y = double.Parse(txtRealY.Text);
                bool containFlag = false;
                foreach (var item in points)
                {
                    if (item.Value.RealData.X == x && item.Value.RealData.Y == y)
                        containFlag = true;
                }
                if (containFlag)
                {
                    MessageBox.Show("This point already has been added!!!");
                }
                else
                {
                    points.Add(points.Count, new MergerCalibrationData(new Position2D(x, y), new Dictionary<int, Position2D>()));
                    UIRefresh();
                }
            }
            else
                MessageBox.Show("Please enter RealX and RealY to add a new Point!!");
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedPosKey.HasValue)
            {
                MessageBox.Show("Please Select Ball to Remove!!");
            }
            else
            {
                points.Remove(selectedPosKey.Value);
                points = DicToDic(points);
                selectedPosKey = null;
                UIRefresh();
            }
        }

        private void btnVision_Checked(object sender, RoutedEventArgs e)
        {
            getVision = true;
        }

        private void btnVision_Unchecked(object sender, RoutedEventArgs e)
        {
            getVision = false;
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            int keyCode = (int)e.Key;
            TextBox txt = (TextBox)sender;

            if (e.Key == Key.Decimal && txt.Text.IndexOf('.') != -1)
            {
                e.Handled = true;
                return;
            }

            if (!((keyCode > 33 && keyCode < 44) || (keyCode > 73 && keyCode < 84)) && e.Key != Key.Decimal && e.Key != Key.OemMinus && e.Key != Key.Subtract)
                e.Handled = true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtRealY.Text = "";
            txtRealX.Text = "";
            txtCam0X.Text = "";
            txtCam1X.Text = "";
            txtCam2X.Text = "";
            txtCam3X.Text = "";
            txtCam0Y.Text = "";
            txtCam1Y.Text = "";
            txtCam2Y.Text = "";
            txtCam3Y.Text = "";
            txtCam0XStat.Text = "";
            txtCam1XStat.Text = "";
            txtCam2XStat.Text = "";
            txtCam3XStat.Text = "";
            txtCam0YStat.Text = "";
            txtCam1YStat.Text = "";
            txtCam2YStat.Text = "";
            txtCam3YStat.Text = "";
            selectedPosKey = null;
            UIRefresh();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            MergerParameters.MergerCalibData = new List<MergerCalibrationData>();
            foreach (var item in points)
            {
                if (item.Value.CameraData.Count > 0)
                {
                    MergerParameters.MergerCalibData.Add(item.Value);
                }
            }
            MergerParameters.Save();
            SendData();
            Load();
        }

        Dictionary<int, MergerCalibrationData> DicToDic(Dictionary<int, MergerCalibrationData> input)
        {
            Dictionary<int, MergerCalibrationData> output = new Dictionary<int, MergerCalibrationData>();
            int count = 0;
            foreach (var item in input)
            {
                output.Add(count, item.Value);
                count++;
            }
            return output;
        }
        #endregion

        #region OldMethod
        void Load_old()
        {
            listToView = new List<VisionDataTemplate>();

            foreach (var item in MergerParameters.MergerCalibData)
            {
                VisionDataTemplate temp = new VisionDataTemplate();
                temp.xReal = item.RealData.X.ToString();
                temp.yReal = item.RealData.Y.ToString();
                foreach (var cameraItem in item.CameraData)
                {
                    switch (cameraItem.Key)
                    {
                        case 0:
                            temp.xCam0 = cameraItem.Value.X.ToString();
                            temp.yCam0 = cameraItem.Value.Y.ToString();
                            break;
                        case 1:
                            temp.xCam1 = cameraItem.Value.X.ToString();
                            temp.yCam1 = cameraItem.Value.Y.ToString();
                            break;
                        case 2:
                            temp.xCam2 = cameraItem.Value.X.ToString();
                            temp.yCam2 = cameraItem.Value.Y.ToString();
                            break;
                        case 3:
                            temp.xCam3 = cameraItem.Value.X.ToString();
                            temp.yCam3 = cameraItem.Value.Y.ToString();
                            break;
                    }
                }
                listToView.Add(temp);
            }
            namesToShow = "";
            foreach (var item in MergerParameters.AvailableCamIds)
            {
                namesToShow = namesToShow + item.ToString();
            }
            DataRefresh();
            SendData();
        }
        void DataRefresh()
        {
            visionDataListView.ItemsSource = listToView.ToList();
            visionDataListView.Items.Refresh();
            txt1.Text = namesToShow;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<char> idList = txt1.Text.ToList();
            List<int> ids = new List<int>();
            int res;
            foreach (var item in idList)
            {
                if (!int.TryParse(item.ToString(), out res) || res < 0 || res >= StaticVariables.CameraCount)
                {
                    ids = new List<int>();
                    for (int i = 0; i < StaticVariables.CameraCount; i++)
                    {
                        ids.Add(i);
                    }
                    txt1.Text = namesToShow;
                    e.Handled = true;
                }
                else
                    ids.Add(res);

            }
            MergerParameters.AvailableCamIds = ids;
            //MergerParameters.Save();
            SendData();
        }
        #endregion
    }
}
