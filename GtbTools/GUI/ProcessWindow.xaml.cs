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
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        public OperationStatus OperationStatus { get; set; }
        public ProcessWindow(OperationStatus operationStatus)
        {
            this.DataContext = this;
            SetOwner();
            OperationStatus = operationStatus;
            InitializeComponent();
        }
        private void SetOwner()
        {
            WindowHandleSearch revitHandleSearch = WindowHandleSearch.MainWindowHandle;
            revitHandleSearch.SetAsOwner(this);
        }

        private void Abort_Btn_Click(object sender, RoutedEventArgs e)
        {
            OperationStatus.UserAborted = true;
        }

        private void Close_Btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void OnOperationEnded(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OperationStatus.OperationEnded += OnOperationEnded;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OperationStatus.OperationEnded -= OnOperationEnded;
        }

        private void TextBoxInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxInfo.ScrollToEnd();
        }

    }
}
