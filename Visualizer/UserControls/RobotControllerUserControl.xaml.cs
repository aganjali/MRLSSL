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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MRL.SSL.CommonClasses.MathLibrary;

namespace Visualizer.UserControls
{
    /// <summary>
    /// Interaction logic for RobotControllerUserControl.xaml
    /// </summary>
    public partial class RobotControllerUserControl : UserControl
    {
        int _robotID;
        public int RobotID
        {
            get { return _robotID; }
            set 
            {
                _robotID = value;
                idLabel.Content = value;
            }
        }
        
        bool _chipKick;
        public bool ChipKick
        {
            get { return _chipKick; }
            set { _chipKick = value; }
        }

        bool _kick;
        public bool Kick
        {
            get { return _kick; }
            set { _kick = value; }
        }

        bool _spinBack;
        public bool SpinBack
        {
            get { return _spinBack; }
            set { _spinBack = value; }
        }

        bool _hasDelay;
        public bool HasDelay
        {
            get { return _hasDelay; }
            set { _hasDelay = value; }
        }

        Vector2D _velocity;
        public Vector2D Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        double _w;
        public double W
        {
            get { return _w; }
            set { _w = value; }
        }

        byte _kickPower;
        public byte KickPower
        {
            get { return _kickPower; }
            set { _kickPower = value; }
        }

        bool _backSensore;

        public bool BackSensore
        {
            get { return _backSensore; }
            set { _backSensore = value; }
        }

        public RobotControllerUserControl()
        {
            InitializeComponent();
        }

        private void spinbackButton_Checked(object sender, RoutedEventArgs e)
        {
            _spinBack = true;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void spinbackButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _spinBack = false;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void chipkickButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!kickEnabled)
            {
                chipkickButton.IsChecked = false;
                return;
            }
            _chipKick = true;
            _kick = false;
            
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void chipkickButton_Unchecked(object sender, RoutedEventArgs e)
        {
            
            _chipKick = false;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void hasdelayButton_Checked(object sender, RoutedEventArgs e)
        {
            _hasDelay = true;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void hasdelayButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _hasDelay = false;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void kickButton_Checked(object sender, RoutedEventArgs e)
        {
            _kick = true;
            _chipKick = false;
            chipkickButton.IsChecked = false;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void kickButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _kick = false;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void vxTextBox_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _velocity.X = (double)vxTextBox.Value;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void vyTextBox_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _velocity.Y = (double)vyTextBox.Value;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void wTextBox_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _w = (double)wTextBox.Value;
            if (CommandChanged != null)
                CommandChanged(null);
        }
        bool kickEnabled = false;
        private void kickpowerTextBox_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!kickEnabled) return;
            _kickPower = (byte)kickpowerTextBox.Value;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            vxTextBox.Value = 0;
            vyTextBox.Value = 0;
            wTextBox.Value = 0;
            kickpowerTextBox.Value = 0;
            SpinBack = false;
            BackSensore = false;
            HasDelay = false;
            Kick = false;

            chipkickButton.IsChecked = false;
            spinbackButton.IsChecked = false;
            hasdelayButton.IsChecked = false;
            spinbackButton.IsChecked = false;
            kickEnableToggleButton.IsChecked = false;
            backSensoreToggleButton.IsChecked = false;

            if (CommandChanged != null)
                CommandChanged(null);
        }

        public delegate void CommandChangedEventHandler(object sender);
        public static event CommandChangedEventHandler CommandChanged;

        private void kickEnableToggleButton_Checked(object sender, RoutedEventArgs e)
        {

            _kickPower = (byte)kickpowerTextBox.Value;
            kickEnabled = true;
            if (chipkickButton.IsChecked.Value)
            {
                _chipKick = true;
                _kick = false;
            }
            if (CommandChanged != null)
                CommandChanged(null);
        }

       

        private void kickEnableToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            kickEnabled = false;
            _kickPower = 0;
            _chipKick = false;
            _kick = false;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void backSensoreToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            _backSensore = true;
            if (CommandChanged != null)
                CommandChanged(null);
        }

        private void backSensoreToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _backSensore = false;
            if (CommandChanged != null)
                CommandChanged(null);
        }

      
    }
}
