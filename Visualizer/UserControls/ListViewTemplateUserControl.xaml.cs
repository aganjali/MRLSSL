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
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.Visualizer.Extentions;

namespace Visualizer.UserControls
{
    /// <summary>
    /// Interaction logic for ListViewTemplateUserControl.xaml
    /// </summary>
    public partial class ListViewTemplateUserControl : UserControl
    {
        int id = 0;
        public ListViewTemplateUserControl()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Tag")
            {
                id = (int)Tag;
                namesComboBox.ItemsSource = ActiveRoleSettings.Default.Parameters[id].propeties.Keys;
            }
        }

        private void namesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (namesComboBox.SelectedItem == null || id==0) return;
            string name = namesComboBox.SelectedItem.As<string>();
            valueTextBox.Text = ActiveRoleSettings.Default.Parameters[id].propeties[name].ToString();
        }
    }
}
