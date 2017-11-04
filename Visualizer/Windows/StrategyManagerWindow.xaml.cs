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
using MRL.SSL.Visualizer.Classes;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Visualizer.Extentions;
using System.Threading.Tasks;
using System.Threading;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for StrategyManagerWindow.xaml
    /// </summary>
    public partial class StrategyManagerWindow : Window
    {
        public StrategyManagerWindow()
        {
            InitializeComponent();
            DataReciever.StrategyRecived += new DataReciever.StrategyRecievedEventHandler(DataReciever_StrategyRecived);
            statusComboBox.ItemsSource = Enum.GetNames(typeof(GameStatus)).ToList();
            refresh();

        }
        bool refreshing = false;
        void DataReciever_StrategyRecived(object sender)
        {
            refreshing = true;
            Dispatcher.Invoke((Action)(() => refresh()));
            StrategyInfo.Save();
            refreshing = false;
        }
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.RequestTable.Add("Strategies", true);
            DataSender.SendOn.Set();
        }

        private void enabledTechCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!refreshing)
            {
                string name = sender.As<CheckBox>().DataContext.As<StrategyInfo>().Name;
                StrategyInfo.Strategies[name].Enabled = true;
                SendData();
            }
        }

        private SerializableDictionary<string, StrategyInfo> CreatList()
        {
            var ret = new SerializableDictionary<string, StrategyInfo>();
            foreach (var item in techniquesListView.Items.Cast<StrategyInfo>().ToList())
                ret[item.Name] = item;
            return ret;

        }

        void refresh()
        {
            techniquesListView.ItemsSource = StrategyInfo.Strategies.Values.Cast<StrategyInfo>().ToList();

        }

        private void enabledTechCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!refreshing)
            {
                string name = sender.As<CheckBox>().DataContext.As<StrategyInfo>().Name;
                StrategyInfo.Strategies[name].Enabled = false;
                SendData();
            }
        }

        private void probabilityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!refreshing)
            {
                double p;
                if (double.TryParse(sender.As<TextBox>().Text, out p))
                {
                    string name = sender.As<TextBox>().DataContext.As<StrategyInfo>().Name;
                    StrategyInfo.Strategies[name].Probability = p;

                    SendData();
                }
            }
        }

        private void techniquesListView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void SendData()
        {
            StrategyInfo.Save();
            DataSender.CurrentWrapper.SendData.Add("Strategy");
            DataSender.SendOn.Set();

        }

        private void techniquesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (techniquesListView.SelectedItem == null) return;
            var selected = techniquesListView.SelectedItem.As<StrategyInfo>();
            vectorList.ItemsSource = selected.Region.ToList();
            if (selected.Status == null)
                selected.Status = new List<GameStatus>();
            gameStatusListView.ItemsSource = selected.Status.ToList();

            var name = selected.Name;
            if (StrategyInfo.Strategies.ContainsKey(name))
            {

            //    mainField.strategyviewer = true;
                var draw = StrategyInfo.Strategies[name].DrawingInfo;
                if (draw != null)
                {
                  //  Animation(draw);
                }
            }
        }
        public void Animation(List<StrategyDrawingInfo> data)
        {
            Task t = new Task((Action)(() =>
            {
                while (true)
                {
                    foreach (var item in data)
                    {
                        mainField.Model = item.Model;
                        //mainField.Darwcollection(item.Drawing);
                        Thread.Sleep(1000);
                    }
                }
            }
            ));
            t.Start();
        }

        private void addvecButton_Click(object sender, RoutedEventArgs e)
        {
            if (techniquesListView.SelectedItem == null) return;
            var selected = techniquesListView.SelectedItem.As<StrategyInfo>();
            var win = new AddVectorWindow();
            var res = win.ShowDialog();
            if (res.HasValue && res.Value)
            {
                double x = 0, y = 0;
                double.TryParse(win.xTextBox.Text, out x);
                double.TryParse(win.yTextBox.Text, out y);
                if (selected.Region == null)
                    selected.Region = new List<Vector2D>();
                selected.Region.Add(new Vector2D(x, y));
            }
            vectorList.ItemsSource = selected.Region.ToList();
            StrategyInfo.Strategies[selected.Name] = selected;
            SendData();
        }

        private void removevecButton_Click(object sender, RoutedEventArgs e)
        {
            if (techniquesListView.SelectedItem == null) return;
            var selected = techniquesListView.SelectedItem.As<StrategyInfo>();
            if (vectorList.SelectedItem == null) return;
            selected.Region.RemoveAt(vectorList.SelectedIndex);
            vectorList.ItemsSource = selected.Region.ToList();
            StrategyInfo.Strategies[selected.Name] = selected;
            SendData();
        }

        private void addStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (techniquesListView.SelectedItem == null) return;
            var selected = techniquesListView.SelectedItem.As<StrategyInfo>();
            var s = statusComboBox.SelectedIndex;
            if (selected.Status == null)
                selected.Status = new List<GameStatus>();
            if (!selected.Status.Contains((GameStatus)s))
                selected.Status.Add((GameStatus)s);
            gameStatusListView.ItemsSource = selected.Status.ToList();
            StrategyInfo.Strategies[selected.Name] = selected;
            SendData();
        }

        private void removestatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (techniquesListView.SelectedItem == null) return;
            var selected = techniquesListView.SelectedItem.As<StrategyInfo>();
            if (gameStatusListView.SelectedItem == null) return;

            selected.Status.Remove(selected.Status.Single(s => s.ToString() == gameStatusListView.SelectedItem.ToString()));
            gameStatusListView.ItemsSource = selected.Status.ToList();
            StrategyInfo.Strategies[selected.Name] = selected;
            SendData();
        }
    }
}
