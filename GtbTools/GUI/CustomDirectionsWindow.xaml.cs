using PipeFlowTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CustomDirectionsWindow.xaml
    /// </summary>
    public partial class CustomDirectionsWindow : Window
    {
        public List<string> Options { get; set; }
        public DefaultDirections DefaultDirections { get; set; }
        public CustomDirectionsWindow(DefaultDirections defaultDirections, Window owner)
        {
            DefaultDirections = defaultDirections;
            SetOptions();
            InitializeComponent();
            SetDefault();
            Owner = owner;
            DataContext = this;
        }

        private void SetOptions()
        {
            Options = new List<string>() { "Up", "Down" };
        }

        private void SetDefault()
        {
            cb1.SelectedIndex = (int)DefaultDirections.SAN_Schmutzwasser;
            cb2.SelectedIndex = (int)DefaultDirections.SAN_Ventilation;
            cb3.SelectedIndex = (int)DefaultDirections.SAN_Regenwasser;
            cb4.SelectedIndex = (int)DefaultDirections.SAN_Kaltwasser;
            cb5.SelectedIndex = (int)DefaultDirections.SAN_Warmwasser;
            cb6.SelectedIndex = (int)DefaultDirections.SAN_Zirkulation;
            cb7.SelectedIndex = (int)DefaultDirections.HZG_Vorlauf;
            cb8.SelectedIndex = (int)DefaultDirections.HZG_Rucklauf;
            cb9.SelectedIndex = (int)DefaultDirections.KAE_Vorlauf;
            cb10.SelectedIndex = (int)DefaultDirections.KAE_Rucklauf;
        }

        private void Btn_Click_Apply(object sender, RoutedEventArgs e)
        {
            DefaultDirections.SAN_Schmutzwasser = (PipeFlowTool.FlowDirection)cb1.SelectedIndex;
            DefaultDirections.SAN_Ventilation = (PipeFlowTool.FlowDirection)cb2.SelectedIndex;
            DefaultDirections.SAN_Regenwasser = (PipeFlowTool.FlowDirection)cb3.SelectedIndex;
            DefaultDirections.SAN_Kaltwasser = (PipeFlowTool.FlowDirection)cb4.SelectedIndex;
            DefaultDirections.SAN_Warmwasser = (PipeFlowTool.FlowDirection)cb5.SelectedIndex;
            DefaultDirections.SAN_Zirkulation = (PipeFlowTool.FlowDirection)cb6.SelectedIndex;
            DefaultDirections.HZG_Vorlauf = (PipeFlowTool.FlowDirection)cb7.SelectedIndex;
            DefaultDirections.HZG_Rucklauf = (PipeFlowTool.FlowDirection)cb8.SelectedIndex;
            DefaultDirections.KAE_Vorlauf = (PipeFlowTool.FlowDirection)cb9.SelectedIndex;
            DefaultDirections.KAE_Rucklauf = (PipeFlowTool.FlowDirection)cb10.SelectedIndex;
            Close();
        }

        private void Btn_Clic_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
