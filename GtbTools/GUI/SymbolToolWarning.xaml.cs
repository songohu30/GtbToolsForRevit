using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OpeningSymbol;
using OwnerSearch;
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
    /// Interaction logic for SymbolToolWarning.xaml
    /// </summary>
    public partial class SymbolToolWarning : Window
    {
        public WarningWindowResult WarningWindowResult { get; set; }
        public UIView UIView { get; set; }
        public List<string> Info { get; set; }
        public SymbolToolWarning(UIView uIView)
        {
            SetOwner();
            UIView = uIView;
            DataContext = this;
            InitializeComponent();          
        }

        private void SetOwner()
        {
            WindowHandleSearch windowHandleSearch = WindowHandleSearch.MainWindowHandle;
            windowHandleSearch.SetAsOwner(this);
        }

        public void DisplayWindow(List<string> info)
        {
            Info = info;
            MyListBox.ItemsSource = Info;
            ShowDialog();
        }
        private void Btn_Click_Yes(object sender, RoutedEventArgs e)
        {
            WarningWindowResult = WarningWindowResult.Yes;
            Close();
        }

        private void Btn_Click_No(object sender, RoutedEventArgs e)
        {
            WarningWindowResult = WarningWindowResult.No;
            Close();
        }

        private void Btn_Click_ZoomIn(object sender, RoutedEventArgs e)
        {
            UIView.Zoom(2);
        }

        private void Btn_Click_ZoomOut(object sender, RoutedEventArgs e)
        {
            UIView.Zoom(0.5);
        }

        private void Btn_Click_YesToAll(object sender, RoutedEventArgs e)
        {
            WarningWindowResult = WarningWindowResult.YesToAll;
            Close();
        }

        private void Btn_Click_NoToAll(object sender, RoutedEventArgs e)
        {
            WarningWindowResult = WarningWindowResult.NoToAll;
            Close();
        }
    }
}
