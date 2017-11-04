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
using MRL.SSL.GameDefinitions.General_Settings;
using Visualizer.Classes;
using System.Windows.Forms;
using System.IO;

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for LoggerPropertiesWindow.xaml
    /// </summary>
    public partial class LoggerPropertiesWindow : Window
    {
        bool isInialized = false;
        public LoggerPropertiesWindow()
        {
            InitializeComponent();
        }

        private void defaulnameRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isInialized)
                LogProssesor.UseDefaulName = true;
        }

        private void customnameRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isInialized)
            {
                LogProssesor.UseDefaulName = false;
                LogProssesor.UserFileName = (customnameTextBox.Text != "") ? customnameTextBox.Text : "usernamelog";
            }
        }

        private void customnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInialized)
                LogProssesor.UserFileName = customnameTextBox.Text;
        }

        private void defaultaddressRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isInialized)
                LogProssesor.UseDefaultAddress = true;
        }

        private void customaddressRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isInialized)
            {
                LogProssesor.UseDefaultAddress = false;
                LogProssesor.UserLogAddress = (customaddressTextBox.Text != null) ? customaddressTextBox.Text : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MRLLog");
            }
        }

        private void selectaddressButton_Click(object sender, RoutedEventArgs e)
        {

            FolderBrowserDialog fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.No) return;
            customaddressTextBox.Text = fd.SelectedPath;
            LogProssesor.UserLogAddress = fd.SelectedPath;

        }

        private void customaddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInialized)
                LogProssesor.UserLogAddress = customaddressTextBox.Text;
        }

        private void loggerDelayNum_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isInialized)
                LogProssesor.LoggerDelay = (int)loggerDelayNum.Value;
        }

        private void InitializeFromSetting()
        {
            if (LogProssesor.UseDefaulName)
            {
                defaulnameRadioButton.IsChecked = true;
                customnameTextBox.Text = "";
            }
            else
            {
                customnameRadioButton.IsChecked = true;
                customnameTextBox.Text = LogProssesor.UserFileName;
            }

            if (LogProssesor.UseDefaultAddress)
            {
                defaultaddressRadioButton.IsChecked = true;
                customaddressTextBox.Text = "";
            }
            else
            {
                customaddressRadioButton.IsChecked = true;
                customaddressTextBox.Text = LogProssesor.UserLogAddress;
            }
            loggerDelayNum.Value = LogProssesor.LoggerDelay;
            isInialized = true;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeFromSetting();
        }

        private void commentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogProssesor.Comment = commentTextBox.Text;
        }
    }
}
