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
using MRL.SSL.Visualizer.UserControls;
using MRL.SSL.GameDefinitions.Visualizer_Classes;
using Visualizer.Classes;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Visualizer.Classes;
using MRL.SSL.GameDefinitions;
using messages_robocup_ssl_wrapper;
using System.Threading;
using System.Threading.Tasks;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for VisionCalibrate.xaml
    /// </summary>
    public partial class VisionCalibrate : Window
    {
        Thread reciveDataThread;
        List<VisionDataTemplate> listToView = new List<VisionDataTemplate>(); 
        VisionDataTemplate selectedItem = new VisionDataTemplate();
        string namesToShow = "";
        MergerParameters m = new MergerParameters();
        public static SSL_WrapperPacket packet0 = new SSL_WrapperPacket();
        public static SSL_WrapperPacket packet1 = new SSL_WrapperPacket();
        public static SSL_WrapperPacket packet2 = new SSL_WrapperPacket();
        public static SSL_WrapperPacket packet3 = new SSL_WrapperPacket();
        private static bool isAuto = false;
        object lockObject = new object();

        public VisionCalibrate()
        {
            reciveDataThread = new Thread(new ThreadStart(RecivePacket));
            InitializeComponent();
            Load();
            //DataReciever.DataRecieved += new DataReciever.DataRecievedEventHandler(DataReciever_DataRecieved);
        }
        //void DataReciever_DataRecieved(object sender, System.IO.MemoryStream Data)
        //{
        //    var r = DataReciever.CurrentWrapper.Model;
        //}

        //void LoD

        void DataRefresh()
        {
            visionDataListView.ItemsSource = listToView.ToList();
            visionDataListView.Items.Refresh();
            txt1.Text = namesToShow;
        }
        void Load()
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

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            //data.Add(new MergerCalibrationData());
            listToView.Add(new VisionDataTemplate());
            DataRefresh();
        }

        private void SetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (visionDataListView.SelectedItem != null)
            {
                VisionDataTemplate temp = (VisionDataTemplate)visionDataListView.SelectedItem;
            }
            else
            {
                MessageBox.Show("Please Select a row!!");
            }
            MergerParameters.Save();
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            btnMode.Content = "Enable";
            isAuto = false;
            reciveDataThread.Abort();
            List<MergerCalibrationData> temp = MergerParameters.MergerCalibData;
            MergerParameters.MergerCalibData = new List<MergerCalibrationData>();
            foreach (var item in listToView)
            {
                MergerCalibrationData MCD = new MergerCalibrationData();
                item.MakeList();
                MCD.RealData = new Position2D(double.Parse(item.xReal), double.Parse(item.yReal));
                foreach (var cameraData in item.Cam)
                {
                    MCD.AddCameraData(cameraData.Key, cameraData.Value);
                }
                MergerParameters.MergerCalibData.Add(MCD);
            }
            MergerParameters.Save();
            Load();
        }

        //private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (visionDataListView.SelectedItem == null)
        //    {
        //        MessageBox.Show("Please Select A Row.");
        //        DataRefresh();
        //    }
        //    else
        //    {
        //        TextBox txtBox = (TextBox)sender;
        //        if (System.Text.RegularExpressions.Regex.IsMatch(txtBox.Text, "[^0-9]."))
        //        {
        //            MessageBox.Show("Please enter only numbers.");
        //            DataRefresh();
        //        }
        //    }
        //}

        private void Txt_KeyDown(object sender, KeyEventArgs e)
        {
            int keyCode = (int)e.Key;
            TextBox txt = (TextBox)sender;
            if (visionDataListView.SelectedItem == null)
            {
                MessageBox.Show("Please Select A Row.");
                DataRefresh();
                e.Handled = true;
            }
            else
            {
                if (e.Key == Key.Decimal && txt.Text.IndexOf('.') != -1)
                {
                    e.Handled = true;
                    return;
                }

                if (!((keyCode > 33 && keyCode < 44) || (keyCode > 73 && keyCode < 84)) && e.Key != Key.Decimal && e.Key != Key.OemMinus && e.Key != Key.Subtract)
                    e.Handled = true;
            }
            SendData();
        }

        private void SendData()
        {
            DataSender.CurrentWrapper.SendData.Add("FourCamMergerSetting");
            DataSender.SendOn.Set();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<char> idList = txt1.Text.ToList();
            List<int> ids = new List<int>();
            foreach (var item in idList)
                ids.Add(int.Parse(item.ToString()));
            MergerParameters.AvailableCamIds = ids;
            MergerParameters.Save();
            SendData();
        }

        private void btnMode_Click(object sender, RoutedEventArgs e)
        {
            if (visionDataListView.SelectedItem == null)
            {
                MessageBox.Show("Please Select A Row.");
                DataRefresh();
            }
            else
            {
                if (!isAuto)
                {
                    btnMode.Content = "Disable";
                    isAuto = true;
                    if (!reciveDataThread.IsAlive)
                    {
                        reciveDataThread = new Thread(new ThreadStart(RecivePacket));
                        reciveDataThread.Start();
                    }
                }
                else
                {
                    btnMode.Content = "Enable";
                    isAuto = false;
                    reciveDataThread.Abort();
                }

            }
        }

        private void visionDataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnMode.Content = "Enable";
            isAuto = false;
            reciveDataThread.Abort();
            selectedItem = (VisionDataTemplate)visionDataListView.SelectedItem;
        }

        void RecivePacket()
        {
            while (true)
            {
                if (packet0.detection.balls.Count == 1)
                {
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().xCam0 = packet0.detection.balls[0].x.ToString();
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().yCam0 = packet0.detection.balls[0].y.ToString();
                }
                if (packet1.detection.balls.Count == 1)
                {
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().xCam1 = packet1.detection.balls[0].x.ToString();
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().yCam1 = packet1.detection.balls[0].y.ToString();
                }
                if (packet2.detection.balls.Count == 1)
                {
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().xCam2 = packet2.detection.balls[0].x.ToString();
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().yCam2 = packet2.detection.balls[0].y.ToString();
                }
                if (packet3.detection.balls.Count == 1)
                {
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().xCam3 = packet3.detection.balls[0].x.ToString();
                    listToView.Where(t => t.xReal == selectedItem.xReal && t.yReal == selectedItem.yReal).ToList().First().yCam3 = packet3.detection.balls[0].y.ToString();
                }

                this.Dispatcher.Invoke((Action)(() =>
                {
                    DataRefresh();
                }));
                Thread.Sleep(2000);
            }
        }
    }
}