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
using MRL.SSL.GameDefinitions;
using MRL.SSL.Visualizer.Extentions;
using MRL.SSL.Visualizer.Classes;
namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for ActiveRoleParametersWindow.xaml
    /// </summary>
    public partial class ActiveRoleParametersWindow : Window
    {

        bool init = false;
        public ActiveRoleParametersWindow()
        {
            InitializeComponent();
            Initialize();
            coefDataGrid.CellEditEnding += new EventHandler<DataGridCellEditEndingEventArgs>(coefDataGrid_CellEditEnding);
            coefDataGrid.CurrentCellChanged += new EventHandler<EventArgs>(coefDataGrid_CurrentCellChanged);
        }

        void coefDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            // var i = coefDataGrid.CurrentCell.Item.ToString();
        }

        void coefDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!e.Cancel)
            {
                var text = ((TextBox)e.EditingElement).Text;
                ActiveParameters.RobotMotionCoefs[e.Row.GetIndex(), e.Column.DisplayIndex] = double.Parse(text);
                SendData();
            }
        }

        public void Initialize()
        {
            ActiveParameters.Load();

            playmodeComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.PlayMode)).ToList();
            conflictmodeComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.ConflictMode)).ToList();
            kickdefaultComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.KickDefult)).ToList();
            clearComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.NonClearMode)).ToList();
            sweepmodeComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.SweepDefult)).ToList();

            playmodeComboBox.SelectedIndex = (int)ActiveParameters.playMode;
            sweepmodeComboBox.SelectedIndex = (int)ActiveParameters.sweepMode;
            conflictmodeComboBox.SelectedIndex = (int)ActiveParameters.conflictMode;
            kickdefaultComboBox.SelectedIndex = (int)ActiveParameters.kickDefult;
            clearComboBox.SelectedIndex = (int)ActiveParameters.clearMode;


            newPlaymodeComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.NewActiveParameters.PlayMode)).ToList();
            newConflictmodeComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.NewActiveParameters.ConflictMode)).ToList();
            newkickdefaultComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.NewActiveParameters.KickDefult)).ToList();
            newClearComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.NewActiveParameters.NonClearMode)).ToList();
            newSweepmodeComboBox.ItemsSource = Enum.GetNames(typeof(ActiveParameters.NewActiveParameters.SweepDefult)).ToList();

            newPlaymodeComboBox.SelectedIndex = (int)ActiveParameters.NewActiveParameters.playMode;
            newSweepmodeComboBox.SelectedIndex = (int)ActiveParameters.NewActiveParameters.sweepMode;
            newConflictmodeComboBox.SelectedIndex = (int)ActiveParameters.NewActiveParameters.conflictMode;
            newkickdefaultComboBox.SelectedIndex = (int)ActiveParameters.NewActiveParameters.kickDefult;
            newClearComboBox.SelectedIndex = (int)ActiveParameters.NewActiveParameters.clearMode;

            newUsespacedribleCheckBox.IsChecked = ActiveParameters.NewActiveParameters.UseSpaceDrible;
            newkickinregionCheckBox.IsChecked = ActiveParameters.NewActiveParameters.KickInRegion;

            clearzonerobotTextBox.Text = ActiveParameters.clearRobotZone.ToString();
            confilictzoneTextBox.Text = ActiveParameters.ConfilictZone.ToString();
            incommingbackballTextBox.Text = ActiveParameters.IncomingBackBall.ToString();
            incommingballdistTextBox.Text = ActiveParameters.incomingBallDistanceTresh.ToString();
            kdbackxTextBox.Text = ActiveParameters.KdBackX.ToString();
            kdbackyTextBox.Text = ActiveParameters.KdBackY.ToString();
            kdsidexTextBox.Text = ActiveParameters.KdSideX.ToString();
            kdsideyTextBox.Text = ActiveParameters.KdSideY.ToString();
            kibackxTextBox.Text = ActiveParameters.KiBackX.ToString();
            kibackyTextBox.Text = ActiveParameters.KiBackY.ToString();
            kickaccuarcyTextBox.Text = ActiveParameters.kickAcuercy.ToString();
            kickanywayTextBox.Text = ActiveParameters.kickAnyWayRegion.ToString();
            kicktousballspeedTextBox.Text = ActiveParameters.kickedToUsBallSpeedTresh.ToString();
            kicktousradiTextBox.Text = ActiveParameters.kickedToUsRadi.ToString();
            kickinregionCheckBox.IsChecked = ActiveParameters.KickInRegion;
            kickregionaccTextBox.Text = ActiveParameters.kickRegionAcuercy.ToString();
            kisidexTextBox.Text = ActiveParameters.KiSideX.ToString();
            kisideyTextBox.Text = ActiveParameters.KiSideY.ToString();
            kpbackxTextBox.Text = ActiveParameters.KpBackX.ToString();
            kpbackyTextBox.Text = ActiveParameters.KpBackY.ToString();
            kpsidexTextBox.Text = ActiveParameters.KpSideX.ToString();
            kpsideyTextBox.Text = ActiveParameters.KpSideY.ToString();
            kptotalvybackTextBox.Text = ActiveParameters.KpTotalVyBack.ToString();
            kptotalvysideTextBox.Text = ActiveParameters.KpTotalVySide.ToString();
            kpxvxbackTextBox.Text = ActiveParameters.KpxVxBack.ToString();
            kpxvxsideTextBox.Text = ActiveParameters.KpxVxSide.ToString();
            kpxvybackTextBox.Text = ActiveParameters.KpxVyBack.ToString();
            kpxvysideTextBox.Text = ActiveParameters.KpxVySide.ToString();
            kpyvybackTextBox.Text = ActiveParameters.KpyVyBack.ToString();
            kpyvysideTextBox.Text = ActiveParameters.KpyVySide.ToString();
            mingoodness.Text = ActiveParameters.minGoodness.ToString();
            nearballspeedtreshTextBox.Text = ActiveParameters.nearBallSpeedTresh.ToString();
            nearincommTextBox.Text = ActiveParameters.nearIncomingRadi.ToString();
            outgoingsideTextBox.Text = ActiveParameters.outgoingSideAngleTresh.ToString();
            staticBallSpeedTreshTextBox.Text = ActiveParameters.staticBallSpeedTresh.ToString();
            sweepzoneTextBox.Text = ActiveParameters.sweepZone.ToString();
            usechipdribleCheckBox.IsChecked = ActiveParameters.UseChipDrible;
            usespacedribleCheckBox.IsChecked = ActiveParameters.UseSpaceDrible;
            vyoffsetbackTextBox.Text = ActiveParameters.vyOffsetBack.ToString();
            vyoffsetTextBox.Text = ActiveParameters.vyOffsetSide.ToString();

            newmingoodness.Text = ActiveParameters.NewActiveParameters.minGoodness.ToString();
            newsweepzoneTextBox.Text = ActiveParameters.NewActiveParameters.sweepZone.ToString();
            newclearzonerobotTextBox.Text = ActiveParameters.NewActiveParameters.clearRobotZone.ToString();
            newconfilictzoneTextBox.Text = ActiveParameters.NewActiveParameters.ConfilictZone.ToString();
            newkickaccuarcyTextBox.Text = ActiveParameters.NewActiveParameters.kickAccuracy.ToString();
            newkickanywayTextBox.Text = ActiveParameters.NewActiveParameters.kickAnyWayRegion.ToString();
            newkickinregionCheckBox.IsChecked = ActiveParameters.NewActiveParameters.KickInRegion;
            newkickregionaccTextBox.Text = ActiveParameters.NewActiveParameters.KickInRegionAcc.ToString();

            //ActiveParameters.Init();
            coefDataGrid.ItemsSource2D = ActiveParameters.RobotMotionCoefs;
            init = true;
        }

        private void sweepzoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.sweepZone = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kickanywayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.kickAnyWayRegion = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kickaccuarcyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.kickAcuercy = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kickregionaccTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.kickRegionAcuercy = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void mingoodness_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.minGoodness = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void nearincommTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.nearIncomingRadi = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void nearballspeedtreshTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.nearBallSpeedTresh = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void incommingballdistTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.incomingBallDistanceTresh = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void outgoingsideTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.outgoingSideAngleTresh = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kicktousballspeedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.kickedToUsBallSpeedTresh = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kicktousradiTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.kickedToUsRadi = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void clearzonerobotTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.clearRobotZone = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void confilictzoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.ConfilictZone = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void incommingbackballTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.IncomingBackBall = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpsideyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpSideY = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kisideyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KiSideY = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kdsideyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KdSideY = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpsidexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpSideX = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kisidexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KiSideX = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kdsidexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KdSideX = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void vyoffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.vyOffsetSide = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpxvysideTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpxVySide = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpyvysideTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpyVySide = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kptotalvysideTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpTotalVySide = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpxvxsideTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpxVxSide = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpbackyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpBackY = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kibackyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KiBackY = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kdbackyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KdBackY = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpbackxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpBackX = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kibackxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KiBackX = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kdbackxTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KdBackX = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpyvybackTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpyVyBack = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpxvybackTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpxVyBack = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void vyoffsetbackTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.vyOffsetBack = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kptotalvybackTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpTotalVyBack = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kptotalvybackTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpTotalVyBack = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void kpxvxbackTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.KpxVxBack = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void staticBallSpeedTreshTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.staticBallSpeedTresh = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void defaultButton_Click(object sender, RoutedEventArgs e)
        {
            ActiveParameters.Init();
            coefDataGrid.ItemsSource2D = null;
            coefDataGrid.ItemsSource2D = ActiveParameters.RobotMotionCoefs;
            ActiveParameters.Save();
        }

        private void SendData()
        {
            DataSender.CurrentWrapper.SendData.Add("ActiveSettings");
            DataSender.SendOn.Set();

        }

        private void kickdefaultComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.kickDefult = (ActiveParameters.KickDefult)kickdefaultComboBox.SelectedIndex;
            SendData();
        }

        private void playmodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.playMode = (ActiveParameters.PlayMode)playmodeComboBox.SelectedIndex;
            SendData();
        }

        private void sweepmodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.sweepMode = (ActiveParameters.SweepDefult)sweepmodeComboBox.SelectedIndex;
            SendData();
        }

        private void conflictmodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.conflictMode = (ActiveParameters.ConflictMode)conflictmodeComboBox.SelectedIndex;
            SendData();
        }

        private void clearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.clearMode = (ActiveParameters.NonClearMode)clearComboBox.SelectedIndex;
            SendData();
        }


        private void usechipdribleCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (usechipdribleCheckBox.IsChecked.HasValue)
            {
                ActiveParameters.UseChipDrible = usechipdribleCheckBox.IsChecked.Value;
                SendData();
            }
        }
        private void usespacedribleCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (usespacedribleCheckBox.IsChecked.HasValue)
            {
                ActiveParameters.UseSpaceDrible = usespacedribleCheckBox.IsChecked.Value;
                SendData();
            }
        }
        private void kickinregionCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (kickinregionCheckBox.IsChecked.HasValue)
            {
                ActiveParameters.KickInRegion = kickinregionCheckBox.IsChecked.Value;
                SendData();
            }

        }

        private void newkickdefaultComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.NewActiveParameters.kickDefult = (ActiveParameters.NewActiveParameters.KickDefult)newkickdefaultComboBox.SelectedIndex;
            SendData();
        }
        private void newSweepmodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.NewActiveParameters.sweepMode = (ActiveParameters.NewActiveParameters.SweepDefult)newSweepmodeComboBox.SelectedIndex;
            SendData();
        }
        private void newPlaymodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.NewActiveParameters.playMode = (ActiveParameters.NewActiveParameters.PlayMode)newPlaymodeComboBox.SelectedIndex;
            SendData();
        }
        private void newConflictmodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.NewActiveParameters.conflictMode = (ActiveParameters.NewActiveParameters.ConflictMode)newConflictmodeComboBox.SelectedIndex;
            SendData();
        }
        private void newClearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActiveParameters.NewActiveParameters.clearMode = (ActiveParameters.NewActiveParameters.NonClearMode)newClearComboBox.SelectedIndex;
            SendData();
        }

        private void newkickinregionCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (newkickinregionCheckBox.IsChecked.HasValue)
            {
                ActiveParameters.NewActiveParameters.KickInRegion = newkickinregionCheckBox.IsChecked.Value;
                SendData();
            }
        }
        private void newusespacedribleCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (newUsespacedribleCheckBox.IsChecked.HasValue)
            {
                ActiveParameters.NewActiveParameters.UseSpaceDrible = newUsespacedribleCheckBox.IsChecked.Value;
                SendData();
            }
        }

        private void newsweepzoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.NewActiveParameters.sweepZone = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void newkickanywayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.NewActiveParameters.kickAnyWayRegion = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void newkickaccuarcyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.NewActiveParameters.kickAccuracy = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void newkickregionaccTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.NewActiveParameters.KickInRegionAcc = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void newmingoodness_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.NewActiveParameters.minGoodness = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void newclearzonerobotTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.NewActiveParameters.clearRobotZone = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }

        private void newconfilictzoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            ActiveParameters.NewActiveParameters.ConfilictZone = double.Parse(sender.As<TextBox>().Text);
            SendData();
        }


    }
}

