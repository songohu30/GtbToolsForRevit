using Functions;
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
    /// Interaction logic for CalculationSupportWindow.xaml
    /// </summary>
    public partial class CalculationSupportWindow : Window
    {
        public CalculationSupport CalculationSupport { get; set; }

        public CalculationSupportWindow(CalculationSupport calculationSupport)
        {
            CalculationSupport = calculationSupport;
            //datacontext
            InitializeComponent();
        }

        private void BtnClick_FindDisconnectedFitting(object sender, RoutedEventArgs e)
        {
            CalculationSupport.Action = Calculations.Action.FindDisconnected;
            CalculationSupport.TheEvent.Raise();
        }

        private void BtnClick_ConnectDisconnected(object sender, RoutedEventArgs e)
        {
            CalculationSupport.Action = Calculations.Action.ConnectDisconnected;
            CalculationSupport.TheEvent.Raise();
        }

        private void BtnClick_FittingWithReducers(object sender, RoutedEventArgs e)
        {
            CalculationSupport.Action = Calculations.Action.FindFittingsWithReducers;
            CalculationSupport.TheEvent.Raise();
        }

        private void BtnClick_ExtractFixtures(object sender, RoutedEventArgs e)
        {
            CalculationSupport.Action = Calculations.Action.ExtractPlumbingFixtures;
            CalculationSupport.TheEvent.Raise();
        }

        private void BtnClick_SelectFromClipboard(object sender, RoutedEventArgs e)
        {
            CalculationSupport.Action = Calculations.Action.SelectFromClipboard;
            CalculationSupport.TheEvent.Raise();
        }
    }
}
