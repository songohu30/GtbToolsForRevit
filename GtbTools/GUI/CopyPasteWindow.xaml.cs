using Autodesk.Revit.DB;
using Functions;
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
    /// Interaction logic for CopyPasteWindow.xaml
    /// </summary>
    public partial class CopyPasteWindow : Window
    {
        public CopyParameterFromHost CopyParameterFromHost { get; set; }
        
        public CopyPasteWindow(CopyParameterFromHost copyParameterFromHost)
        {
            SetOwner();
            CopyParameterFromHost = copyParameterFromHost;
            this.DataContext = this;
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch handle = WindowHandleSearch.MainWindowHandle;
            handle.SetAsOwner(this);
        }

        private void BtnFromHost_Click(object sender, RoutedEventArgs e)
        {
            CopyParameterFromHost._hostClicked = true;
            CopyParameterFromHost._selfClicked = false;
            CopyParameterFromHost.HostCopySelectedSymbol = CopyParameterFromHost.GenericSymbols[ComBoxFromHost.SelectedIndex];
            CopyParameterFromHost.SetOpeningInstancesHost();
            CopyParameterFromHost.SetParameterNames(TxtBoxSourceHost.Text, TxtBoxDestinationHost.Text, IsTypeHost.IsChecked);
            CopyParameterFromHost.InitializeEvent.Raise();
        }

        private void BtnFromMe_Click(object sender, RoutedEventArgs e)
        {
            CopyParameterFromHost._hostClicked = false;
            CopyParameterFromHost._selfClicked = true;
            CopyParameterFromHost.SelfCopySelectedSymbol = CopyParameterFromHost.GenericSymbols[ComBoxFromMe.SelectedIndex];
            CopyParameterFromHost.SetOpeningInstancesSelf();
            CopyParameterFromHost.SetParameterNames(TxtBoxSourceMe.Text, TxtBoxDestinationMe.Text, null);
            CopyParameterFromHost.InitializeEvent.Raise();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CopyParameterFromHost.IsInitialized = false;
        }
    }
}
