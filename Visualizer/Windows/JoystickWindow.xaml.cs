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
using Visualizer.Classes;
using MRL.SSL.Visualizer.Classes;
using SlimDX.DirectInput;
using System.Globalization;
using SlimDX;
using MRL.SSL.Visualizer.Extentions;
using MRL.SSL.GameDefinitions;
using Enterprise;
using MRL.SSL.Visualizer.Windows;

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for JoystickWindow.xaml
    /// </summary>
    public partial class JoystickWindow : Window
    {


        private float _vX;
        private float _vY;
        private float _w;
        public event RoutedEventHandler RaiseCustomEvent;
        public float VX
        {
            get
            {
                return _vX;
            }
            set
            {
                _vX = value;
            }
        }
        public float VY
        {
            get
            {
                return _vY;
            }
            set
            {
                _vY = value;
            }
        }
        public float W
        {
            get
            {
                return _w;
            }
            set
            {
                _w = value;
            }
        }

        Joystick joystick;

        JoystickState state = new JoystickState();

        public IntPtr Handle
        {
            get { return (new System.Windows.Interop.WindowInteropHelper(this)).Handle; }
        }
        void CreateDeviceList()
        {
            // make sure that DirectInput has been initialized
            DirectInput dinput = new DirectInput();
            // search for devices
            joystickComboBox.Items.Clear();
            joystickComboBox.DisplayMemberPath = "InstanceName";
            foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                // create the device
                try
                {
                    joystickComboBox.Items.Add(device);
                    break;
                }
                catch (DirectInputException ex)
                {
                    Enterprise.Logger.Write(Enterprise.LogType.Exception, ex.ToString());
                }
            }

            if (joystickComboBox.Items.Count == 0)
            {
                MessageBox.Show("There are no joysticks attached to the system.");
                return;
            }

            //foreach (DeviceObjectInstance deviceObject in joystick.GetDeviceObjects())
            //{
            //    if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
            //        joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);

            //    UpdateControl(deviceObject);
            //}
        }
        float lastx = 0, lasty = 0, lastw = 0;
        void ReadImmediateData()
        {
            try
            {
                if (joystick.Acquire().IsFailure)
                    return;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                return;
            }
            try
            {
                if (joystick.Poll().IsFailure)
                    return;
            }
            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
                return;
            }
            state = joystick.GetCurrentState();
            if (Result.Last.IsFailure)
                return;

            Dispatcher.Invoke((Action)(() => UpdateUI()));
        }
        string command = "H";
        void ReleaseDevice()
        {
            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
            joystick = null;
        }

        public JoystickWindow()
        {
            InitializeComponent();
            joystick = null;
            refreshJoys();
            DataReciever.DataRecieved += new DataReciever.DataRecievedEventHandler(DataReciever_DataRecieved);

        }

        void DataReciever_DataRecieved(object sender, System.IO.MemoryStream Data)
        {
            if (joystick != null)
            {
                ReadImmediateData();
            }
        }

        private void refreshJoys()
        {
            // if (joystick == null)
            CreateDeviceList();
        }
        bool mustrefresh = false;
        void UpdateUI()
        {
            #region Speed
            if (DataReciever.CurrentWrapper.Model.Status == GameStatus.ComponetsTest)
            {

                double ofsetlow = 27000;
                double ofsethigh = 40000;
                if (state.Y > ofsethigh)
                {
                    upImage.Opacity = 0.7;
                    downImage.Opacity = 1;
                    VY = ((((float)state.Y - 32703f) * 3.99f) / (65535f - 32703f)) * -1;


                }
                else if (state.Y < ofsetlow)
                {
                    upImage.Opacity = 1;
                    downImage.Opacity = 0.7;
                    VY = (((((float)state.Y * 3.99f) / 32703f) - 3.99f)) * -1;


                }
                else if (state.Y > ofsetlow || state.Y < ofsethigh)
                {
                    upImage.Opacity = 0.7;
                    downImage.Opacity = 0.7;
                    VY = 0;//(((((float)state.Y * 3.99f) / 32703f) - 3.99f)) * -1;

                }

                if (state.X > ofsethigh)
                {
                    leftImage.Opacity = 0.7;
                    rightImage.Opacity = 1;
                    VX = ((((float)state.X - 32703f) * 3.99f) / (65535f - 32703f));


                }
                else if (state.X < ofsetlow)
                {
                    leftImage.Opacity = 1;
                    rightImage.Opacity = 0.7;
                    VX = (((((float)state.X * 3.99f) / 32703f) - 3.99f));

                }
                else if (state.X > ofsetlow || state.X < ofsethigh)
                {
                    leftImage.Opacity = 0.7;
                    rightImage.Opacity = 0.7;
                    VX = 0;//(((((float)state.X * 3.99f) / 32703f) - 3.99f));

                }
                W = 0;
                if (state.Z < 27000)
                {
                    //left

                    turnrightImage.Opacity = 1;
                    turnleftImage.Opacity = 0.7;
                    W = (((float)state.Z - 36928f) * 15 / (65535f - 36928f)) * -1f;
                }
                else if (state.Z > 40000)
                {
                    turnrightImage.Opacity = 0.7;
                    turnleftImage.Opacity = 1;
                    W = (((float)state.Z * 15 / 32767f) - 15) * -1f;
                    //right


                }
                else if (state.Z <= 40000 && state.Z >= 27000)
                {
                    turnrightImage.Opacity = 0.7;
                    turnleftImage.Opacity = 0.7;
                    W = 0;
                }

                rLabel.Content = "R : " + W;
                xLable.Content = "X : " + VX;
                yLabel.Content = "Y : " + VY;
                if (W != lastw || VY != lasty || VX != lastx)
                {
                    if (robotidComboBox.SelectedItem != null)
                    {
                        lastw = W;
                        lastx = VX;
                        lasty = VY;
                        int robotId = int.Parse(robotidComboBox.SelectedItem.As<ComboBoxItem>().Content.ToString());
                        SingleWirelessCommand swc = new SingleWirelessCommand();
                        swc._kickPower = 0;
                        swc._kickPowerByte = 0;
                        swc.isChipKick = false;
                        swc.isDelayedKick = false;
                        swc.SpinBack = 0;
                        swc.Vx = VX;
                        swc.Vy = VY;
                        swc.W = W;
                        if (RobotComponentsController.RobotCommands == null)
                            RobotComponentsController.RobotCommands = new Dictionary<int, SingleWirelessCommand>();
                        RobotComponentsController.RobotCommands[robotId] = swc;
                        DataSender.CurrentWrapper.SendData.Add("Command");
                        DataSender.SendOn.Set();
                    }
                }
            }
            #endregion


            bool[] buttons = state.GetButtons();

            if (buttons[0])
            {
                mustrefresh = true;
                command = "S";
            }
            if (buttons[1])
            {
                mustrefresh = true;
                command = "s";
            }
            if (buttons[2])
            {
                mustrefresh = true;
                command = "I";
            }
            if (buttons[3])
            {
                mustrefresh = true;
                command = "i";
            }
            if (buttons[4])
            {
                RaiseCustomEvent(this, new RoutedEventArgs());
            }
            if (buttons[5])
            {
                mustrefresh = true;
                command = "H";
            }
            if (buttons[7])
            {
                mustrefresh = true;
                command = "v";
            }
            if (buttons[6])
            {
                mustrefresh = true;
                command = "a";
            }
            if (mustrefresh)
            {
                if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                    DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
                DataSender.SendOn.Set();
                DataSender.CurrentWrapper.RefreeCommand = command;
                mustrefresh = false;
            }

            //if (joystick == null)
            //    createDeviceButton.Text = "Create Device";
            //else
            //    createDeviceButton.Text = "Release Device";

            //string strText = null;

            //label_X.Text = state.X.ToString(CultureInfo.CurrentCulture);
            //label_Y.Text = state.Y.ToString(CultureInfo.CurrentCulture);
            //label_Z.Text = state.Z.ToString(CultureInfo.CurrentCulture);

            //label_XRot.Text = state.RotationX.ToString(CultureInfo.CurrentCulture);
            //label_YRot.Text = state.RotationY.ToString(CultureInfo.CurrentCulture);
            //label_ZRot.Text = state.RotationZ.ToString(CultureInfo.CurrentCulture);

            //int[] slider = state.GetSliders();

            //label_S0.Text = slider[0].ToString(CultureInfo.CurrentCulture);
            //label_S1.Text = slider[1].ToString(CultureInfo.CurrentCulture);

            //int[] pov = state.GetPointOfViewControllers();

            //label_P0.Text = pov[0].ToString(CultureInfo.CurrentCulture);
            //label_P1.Text = pov[1].ToString(CultureInfo.CurrentCulture);
            //label_P2.Text = pov[2].ToString(CultureInfo.CurrentCulture);
            //label_P3.Text = pov[3].ToString(CultureInfo.CurrentCulture);

            //bool[] buttons = state.GetButtons();

            //for (int b = 0; b < buttons.Length; b++)
            //{
            //    if (buttons[b])
            //        strText += b.ToString("00 ", CultureInfo.CurrentCulture);
            //}
            //label_ButtonList.Text = strText;
        }

        void UpdateControl(DeviceObjectInstance d)
        {
            //if (ObjectGuid.XAxis == d.ObjectTypeGuid)
            //{
            //    //label_XAxis.Enabled = true;
            //    //label_X.Enabled = true;
            //}
            //if (ObjectGuid.YAxis == d.ObjectTypeGuid)
            //{
            //    //label_YAxis.Enabled = true;
            //    //label_Y.Enabled = true;
            //}
            //if (ObjectGuid.ZAxis == d.ObjectTypeGuid)
            //{
            //    //label_ZAxis.Enabled = true;
            //    //label_Z.Enabled = true;
            //}
            //if (ObjectGuid.RotationalXAxis == d.ObjectTypeGuid)
            //{
            //    //label_XRotation.Enabled = true;
            //    //label_XRot.Enabled = true;
            //}
            //if (ObjectGuid.RotationalYAxis == d.ObjectTypeGuid)
            //{
            //    //label_YRotation.Enabled = true;
            //    //label_YRot.Enabled = true;
            //}
            //if (ObjectGuid.RotationalZAxis == d.ObjectTypeGuid)
            //{
            //    //label_ZRotation.Enabled = true;
            //    //label_ZRot.Enabled = true;
            //}

            //if (ObjectGuid.Slider == d.ObjectTypeGuid)
            //{
            //    switch (SliderCount++)
            //    {
            //        case 0:
            //            label_Slider0.Enabled = true;
            //            label_S0.Enabled = true;
            //            break;

            //        case 1:
            //            label_Slider1.Enabled = true;
            //            label_S1.Enabled = true;
            //            break;
            //    }
            //}

            //if (ObjectGuid.PovController == d.ObjectTypeGuid)
            //{
            //    switch (numPOVs++)
            //    {
            //        case 0:
            //            label_POV0.Enabled = true;
            //            label_P0.Enabled = true;
            //            break;

            //        case 1:
            //            label_POV1.Enabled = true;
            //            label_P1.Enabled = true;
            //            break;

            //        case 2:
            //            label_POV2.Enabled = true;
            //            label_P2.Enabled = true;
            //            break;

            //        case 3:
            //            label_POV3.Enabled = true;
            //            label_P3.Enabled = true;
            //            break;
            //    }
            //}
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void SelectDevice(DeviceInstance device)
        {
            DirectInput dinput = new DirectInput();
            joystick = new Joystick(dinput, device.InstanceGuid);
            //joystick.SetCooperativeLevel(this.Handle, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);
            joystick.Acquire();
        }

        private void joystickComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReleaseDevice();
            SelectDevice(joystickComboBox.SelectedItem.As<DeviceInstance>());
        }
    }
}
