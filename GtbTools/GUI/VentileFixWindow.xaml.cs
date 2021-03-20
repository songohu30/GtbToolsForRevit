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
using VentileSizeFix;

namespace GUI
{
    /// <summary>
    /// Interaction logic for VentileFixWindow.xaml
    /// </summary>
    public partial class VentileFixWindow : Window
    {
        public VentileFix VentileFix { get; set; }

        public VentileFixWindow(VentileFix ventileFix)
        {
            SetOwner();
            VentileFix = ventileFix;
            this.DataContext = this;
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch whs = WindowHandleSearch.MainWindowHandle;
            whs.SetAsOwner(this);
        }

        private void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VentileFixViewModel item = MyDataGrid.SelectedItem as VentileFixViewModel;
            if (item == null) return;
            VentileFix.SelectedItem = item.ChangedValve.ElementId;
            VentileFix.Action = VentileFixAction.Show;
            VentileFix.TheEvent.Raise();
        }

        private void Btn_Click_Refresh(object sender, RoutedEventArgs e)
        {
            VentileFix.Action = VentileFixAction.Refresh;
            VentileFix.TheEvent.Raise();
            VentileFix.SignalEvent.WaitOne();
            VentileFix.SignalEvent.Reset();
            MyDataGrid.ItemsSource = VentileFix.VentileFixViewModels;
        }

        private void Btn_Click_ShowInProject(object sender, RoutedEventArgs e)
        {
            VentileFixViewModel item = MyDataGrid.SelectedItem as VentileFixViewModel;
            if (item == null) return;
            VentileFix.SelectedItem = item.ChangedValve.ElementId;
            VentileFix.Action = VentileFixAction.Zoom;
            VentileFix.TheEvent.Raise();
        }

        private void Btn_Click_FixAll(object sender, RoutedEventArgs e)
        {
            VentileFix.Action = VentileFixAction.FixAll;
            VentileFix.TheEvent.Raise();
        }

        private void Btn_Click_FixSelected(object sender, RoutedEventArgs e)
        {
            var etwas = MyDataGrid.SelectedItems;
            VentileFix.SelectedValves = new List<ChangedValve>();
            foreach (VentileFixViewModel item in etwas)
            {
                VentileFix.SelectedValves.Add(item.ChangedValve);
            }
            VentileFix.Action = VentileFixAction.FixSelected;
            VentileFix.TheEvent.Raise();
        }
    }
}
