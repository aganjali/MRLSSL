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

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for TacticWindow.xaml
    /// </summary>
    public partial class TacticWindow : Window
    {
        bool initialed = false;
        public TacticWindow()
        {
            
            if (GameSettings.Default.Tactic == null)
                GameSettings.Default.Tactic = new MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, int>();
            initialed = false;
            InitializeComponent();
            initialed = true;
            initialize();
            DataSender.CurrentWrapper.SendData.Add("Tactic");
            DataSender.SendOn.Set();
            
            
        }

        #region ourKickOff
        private void ourKickOffAutoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurKickOff"] = (int)OurKickOff.Auto;
                DataSender.SendOn.Set();
            }
        }

        private void ourkickoffdirectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurKickOff"] = (int)OurKickOff.Direct;
                DataSender.SendOn.Set();
            }
        }

        private void ourkickoffinsideRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurKickOff"] = (int)OurKickOff.SidePassing;
                DataSender.SendOn.Set();
            }
        }
        #endregion

        #region ourCornelKick
        private void ourCornelKickautoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurCornelKick"] = (int)OurCornerKick.Auto;
                DataSender.SendOn.Set();
            }
        }

        private void ourcornalCompressRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurCornelKick"] = (int)OurCornerKick.Compress;
                DataSender.SendOn.Set();
            }
        }

        private void ourCornalheadingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurCornelKick"] = (int)OurCornerKick.Heading;
                DataSender.SendOn.Set();
            }
        }

        private void ourcornelHeavyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurCornelKick"] = (int)OurCornerKick.Heavy;
                DataSender.SendOn.Set();
            }
        }

        private void ourcornelnormalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurCornelKick"] = (int)OurCornerKick.Normal;
                DataSender.SendOn.Set();
            }
        }

        private void ourcornelnearRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurCornelKick"] = (int)OurCornerKick.Near;
                DataSender.SendOn.Set();
            }
        }
        #endregion

        #region penaltygoaller
        private void penaltygoallermovebyangleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["PenaltyGoaller"] = (int)OurPenaltyGoaller.MoveByOppAngle;
                DataSender.SendOn.Set();
            }
        }

        private void penaltygoallerrandomlyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["PenaltyGoaller"] = (int)OurPenaltyGoaller.RandomlyMove;
                DataSender.SendOn.Set();
            }
        }
        #endregion

        #region normalgame

        private void halfbackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["NormalGame"] = (int)OurNormalGame.HalfBack;
                DataSender.SendOn.Set();
            }
        }

        private void normalgameautoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["NormalGame"] = (int)OurNormalGame.Attack;
                DataSender.SendOn.Set();
            }
        }

        private void normalgameattackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["NormalGame"] = (int)OurNormalGame.Attack;
                DataSender.SendOn.Set();
            }
        }

        private void normalgameNormalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["NormalGame"] = (int)OurNormalGame.Normal;
                DataSender.SendOn.Set();
            }
        }

        private void normalgamedefenceRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["NormalGame"] = (int)OurNormalGame.Defence;
                DataSender.SendOn.Set();
            }
        }

        private void normalgamedirectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["NormalGame"] = (int)OurNormalGame.Direct;
                DataSender.SendOn.Set();
            }
        }
        #endregion

        #region FKOur
        private void fkourautoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            DataSender.CurrentWrapper.SendData.Add("Tactic");
            GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.Auto;
            DataSender.SendOn.Set();
        }

        private void fkournormalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.Normal;
                DataSender.SendOn.Set();
            }
        }

        private void fkourheavyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.Heavy;
                DataSender.SendOn.Set();
            }
        }

        private void fkourheadingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.Heading;
                DataSender.SendOn.Set();
            }
        }

        private void fkourcompressRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.Compress;
                DataSender.SendOn.Set();
            }
        }

        private void fkournearfreeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.NearFree;
                DataSender.SendOn.Set();
            }
        }
        #endregion

        #region FKMiddle
        private void fkmiddleautoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKMiddle"] = (int)OurIndirectFreeKick.Auto;
                DataSender.SendOn.Set();
            }
        }

        private void fkmiddlenormalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKMiddle"] = (int)OurIndirectFreeKick.Normal;
                DataSender.SendOn.Set();
            }
        }

        private void fkmiddleheavyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKMiddle"] = (int)OurIndirectFreeKick.Heavy;
                DataSender.SendOn.Set();
            }
            
        }

        private void fkmiddleheadingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKMiddle"] = (int)OurIndirectFreeKick.Heading;
                DataSender.SendOn.Set();
            }
        }

        private void fkmiddlecompressRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKMiddle"] = (int)OurIndirectFreeKick.Compress;
                DataSender.SendOn.Set();
            }
        }

        private void fkmiddlenearfreeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKMiddle"] = (int)OurIndirectFreeKick.NearFree;
                DataSender.SendOn.Set();
            }
        }
        #endregion

        #region FKOpp
        private void fkoppautoRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOpp"] = (int)OurIndirectFreeKick.Auto;
                DataSender.SendOn.Set();
            }
        }

        private void fkoppnormalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOpp"] = (int)OurIndirectFreeKick.Normal;
                DataSender.SendOn.Set();
            }
        }

        private void fkoppheavyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOpp"] = (int)OurIndirectFreeKick.Heavy;
                DataSender.SendOn.Set();
            }
        }

        private void fkoppheadingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOpp"] = (int)OurIndirectFreeKick.Heading;
                DataSender.SendOn.Set();
            }
        }

        private void fkoppcompressRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOpp"] = (int)OurIndirectFreeKick.Compress;
                DataSender.SendOn.Set();
            }
        }

        private void fkoppnearfreeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOpp"] = (int)OurIndirectFreeKick.NearFree;
                DataSender.SendOn.Set();
            }
        }
        #endregion

        void initialize()
        {
            if (GameSettings.Default.Tactic.ContainsKey("OurKickOff"))
            {
                switch ((OurKickOff)GameSettings.Default.Tactic["OurKickOff"])
                {
                    case OurKickOff.Auto: ourKickOffAutoRadioButton.IsChecked = true; break;
                    case OurKickOff.Direct: ourkickoffdirectRadioButton.IsChecked = true; break;
                    case OurKickOff.SidePassing: ourkickoffinsideRadioButton.IsChecked = true; break;
                    case OurKickOff.ChipPass: chippassRadioButton.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("OurKickOff", (int)OurKickOff.SidePassing);

            if (GameSettings.Default.Tactic.ContainsKey("OurCornelKick"))
            {
                switch ((OurCornerKick)GameSettings.Default.Tactic["OurCornelKick"])
                {
                    case OurCornerKick.Auto: ourCornelKickautoRadioButton.IsChecked = true; break;
                    case OurCornerKick.Compress: ourcornalCompressRadioButton.IsChecked = true; break;
                    case OurCornerKick.Heading: ourCornalheadingRadioButton.IsChecked = true; break;
                    case OurCornerKick.Heavy: ourcornelHeavyRadioButton.IsChecked = true; break;
                    case OurCornerKick.Normal: ourcornelnormalRadioButton.IsChecked = true; break;
                    case OurCornerKick.Near: ourcornelnearRadioButton.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("OurCornelKick", (int)OurCornerKick.Heading);

            if (GameSettings.Default.Tactic.ContainsKey("PenaltyGoaller"))
            {
                switch ((OurPenaltyGoaller)GameSettings.Default.Tactic["PenaltyGoaller"])
                {
                    case OurPenaltyGoaller.MoveByOppAngle: penaltygoallermovebyangleRadioButton.IsChecked = true; break;
                    case OurPenaltyGoaller.RandomlyMove: penaltygoallerrandomlyRadioButton.IsChecked = true; break;
                    case OurPenaltyGoaller.Dive: panaltygoallerChangRadioButton.IsChecked = true; break;
                    case OurPenaltyGoaller.DiveByChangeAngle: divebychangeRadioButton.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("PenaltyGoaller", (int)OurPenaltyGoaller.MoveByOppAngle);

            if (GameSettings.Default.Tactic.ContainsKey("NormalGame"))
            {
                switch ((OurNormalGame)GameSettings.Default.Tactic["NormalGame"])
                {
                    case OurNormalGame.Auto: normalgameautoRadioButton.IsChecked = true; break;
                    case OurNormalGame.Attack: normalgameattackRadioButton.IsChecked = true; break;
                    case OurNormalGame.Normal: normalgameNormalRadioButton.IsChecked = true; break;
                    case OurNormalGame.Defence: normalgamedefenceRadioButton.IsChecked = true; break;
                    case OurNormalGame.Direct: normalgamedirectRadioButton.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("NormalGame", (int)OurNormalGame.Normal);


            if (GameSettings.Default.Tactic.ContainsKey("FKOur"))
            {
                switch ((OurIndirectFreeKick)GameSettings.Default.Tactic["FKOur"])
                {
                    case OurIndirectFreeKick.Auto: fkourautoRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Compress: fkourcompressRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Heading: fkourheadingRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Heavy: fkourheavyRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.NearFree: fkournearfreeRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Normal: fkournormalRadioButton.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("FKOur", (int)OurIndirectFreeKick.Normal);


            if (GameSettings.Default.Tactic.ContainsKey("FKMiddle"))
            {
                switch ((OurIndirectFreeKick)GameSettings.Default.Tactic["FKMiddle"])
                {
                    case OurIndirectFreeKick.Auto: fkmiddleautoRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Compress: fkmiddlecompressRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Heading: fkmiddleheadingRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Heavy: fkmiddleheavyRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.NearFree: fkmiddlenearfreeRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Normal: fkmiddlenormalRadioButton.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("FKMiddle", (int)OurIndirectFreeKick.Normal);


            if (GameSettings.Default.Tactic.ContainsKey("FKOpp"))
            {
                switch ((OurIndirectFreeKick)GameSettings.Default.Tactic["FKOpp"])
                {
                    case OurIndirectFreeKick.Auto: fkoppautoRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Compress: fkoppcompressRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Heading: fkoppheadingRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Heavy: fkoppheavyRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.NearFree: fkoppnearfreeRadioButton.IsChecked = true; break;
                    case OurIndirectFreeKick.Normal: fkoppnormalRadioButton.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("FKOpp", (int)OurIndirectFreeKick.Normal);

            if (GameSettings.Default.Tactic.ContainsKey("Penalty"))
            {
                switch ((PenaltyShoter)GameSettings.Default.Tactic["Penalty"])
                {
                    case PenaltyShoter.Corner: penaltyCorner.IsChecked = true; break;
                    case PenaltyShoter.Center: penaltyCenter.IsChecked = true; break;
                }
            }
            else
                GameSettings.Default.Tactic.Add("Penalty", (int)PenaltyShoter.Center);

            DataSender.CurrentWrapper.SendData.Add("Tactic");
            DataSender.SendOn.Set();
        }

        private void veryheavyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.VeryHeavy;
                DataSender.SendOn.Set();
            }
        }

        private void penaltyCorner_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["Penalty"] = (int)PenaltyShoter.Corner;
                DataSender.SendOn.Set();
            }

        }

        private void penaltyCenter_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["Penalty"] = (int)PenaltyShoter.Center;
                DataSender.SendOn.Set();
            }
        }

        private void panaltygoallerChangButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["PenaltyGoaller"] = (int)OurPenaltyGoaller.Dive;
                DataSender.SendOn.Set();
            }
        }

        private void divebychangeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["PenaltyGoaller"] = (int)OurPenaltyGoaller.DiveByChangeAngle;
                DataSender.SendOn.Set();
            }
        }

        private void chippassRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["OurKickOff"] = (int)OurKickOff.ChipPass;
                DataSender.SendOn.Set();
            }
        }

        private void doubleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (initialed)
            {
                DataSender.CurrentWrapper.SendData.Add("Tactic");
                GameSettings.Default.Tactic["FKOur"] = (int)OurIndirectFreeKick.Double;
                DataSender.SendOn.Set();
            }
        }
    }
}
