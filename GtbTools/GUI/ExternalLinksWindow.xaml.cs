using ExternalLinkControl;
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
    /// Interaction logic for ExternalLinksControl.xaml
    /// </summary>
    public partial class ExternalLinksWindow : Window
    {
        public ExternalLinkToolViewModel ExternalLinkToolViewModel { get; set; }
        ExternalLinkTool _externalLinkTool;
        public ExternalLinksWindow(ExternalLinkTool externalLinkTool)
        {
            _externalLinkTool = externalLinkTool;
            ExternalLinkToolViewModel = externalLinkTool.ExternalLinkToolViewModel;
            this.DataContext = ExternalLinkToolViewModel;
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch handleSearch = WindowHandleSearch.MainWindowHandle;
            handleSearch.SetAsOwner(this);
        }

        private void BtnClick_ViewControl(object sender, RoutedEventArgs e)
        {
            if(TabControl.SelectedIndex == 0)
            {
                Button button = (Button)sender;
                RevitLinkViewModel sendingClass = (RevitLinkViewModel)button.DataContext;
                sendingClass.UpdateModel();
                RvtLinkViewsWindow window = new RvtLinkViewsWindow(this, sendingClass, _externalLinkTool);
                window.ShowDialog();
            }

            if (TabControl.SelectedIndex == 1)
            {
                Button button = (Button)sender;
                CadLinkViewModel sendingClass = (CadLinkViewModel)button.DataContext;
                sendingClass.UpdateModel();
                CadLinkViewsWindow window = new CadLinkViewsWindow(this, sendingClass, _externalLinkTool);
                window.ShowDialog();
            }
        }
    }
}
