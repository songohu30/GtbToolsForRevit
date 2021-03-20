using Autodesk.Revit.DB.Plumbing;
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
    /// Interaction logic for SystemTypeWindow.xaml
    /// </summary>
    public partial class SystemTypeWindow : Window
    {
        public SystemTypeChanger SystemTypeChanger { get; set; }

        public SystemTypeWindow(SystemTypeChanger systemTypeChanger)
        {
            SystemTypeChanger = systemTypeChanger;
            SetOwner();
            InitializeComponent();
            ComBoxMain.DataContext = SystemTypeChanger;
        }

        private void SetOwner()
        {
            OwnerSearch.WindowHandleSearch search = OwnerSearch.WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void BtnApply(object sender, RoutedEventArgs e)
        {
            SystemTypeChanger.SelectedSystemType = (PipingSystemType)ComBoxMain.SelectedItem;
            SystemTypeChanger.Action = Functions.Action.Apply;
            SystemTypeChanger.TheEvent.Raise();
            SystemTypeChanger.SignalEvent.WaitOne();
            SystemTypeChanger.SignalEvent.Reset();
            this.Close();
        }
    }
}
