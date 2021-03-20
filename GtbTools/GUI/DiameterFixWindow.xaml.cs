using Autodesk.Revit.DB;
using CuttingElementTool;
using Functions;
using OwnerSearch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for DiameterFixWindow.xaml
    /// </summary>
    public partial class DiameterFixWindow : Window
    {
        public CuttingElementSearch CuttingElementSearch { get; set; }

        public DiameterFixWindow(CuttingElementSearch cuttingElementSearch)
        {
            SetOwner();
            CuttingElementSearch = cuttingElementSearch;
            InitializeComponent();
            MainDataGrid.DataContext = CuttingElementSearch;
            DataGridNotFound.DataContext = CuttingElementSearch;
            DataGridOther.DataContext = CuttingElementSearch;
            ComBoxLinks.DataContext = CuttingElementSearch;
        }

        private void SetOwner()
        {
            WindowHandleSearch windowHandleSearch = WindowHandleSearch.MainWindowHandle;
            windowHandleSearch.SetAsOwner(this);
        }

        private void Btn_Click_Search(object sender, RoutedEventArgs e)
        {
            if (ComBoxLinks.SelectedItem == null) return;
            CuttingElementSearch.SelectedLink = ComBoxLinks.SelectedItem as RevitLinkInstance;
            CuttingElementSearch.ToolAction = CutElementToolAction.SearchLinks;
            CuttingElementSearch.TheEvent.Raise();
            //wait to finish external event
            CuttingElementSearch.SignalEvent.WaitOne();
            CuttingElementSearch.SignalEvent.Reset();
            MainDataGrid.Items.Refresh();
            DataGridNotFound.Items.Refresh();
            DataGridOther.Items.Refresh();
        }

        private void Btn_Click_FixSelected(object sender, RoutedEventArgs e)
        {
            var selectedItems = MainDataGrid.SelectedItems;
            CuttingElementSearch.Selection = new List<CutSearchOpeningModel>();
            foreach (var item in selectedItems)
            {
                CutSearchOpeningModel model = item as CutSearchOpeningModel;
                CuttingElementSearch.Selection.Add(model);
            }
            CuttingElementSearch.ToolAction = CutElementToolAction.FixSelected;
            CuttingElementSearch.TheEvent.Raise();
            CuttingElementSearch.SignalEvent.WaitOne();
            CuttingElementSearch.SignalEvent.Reset();
            MainDataGrid.Items.Refresh();
            DataGridNotFound.Items.Refresh();
            DataGridOther.Items.Refresh();
            CuttingElementSearch.OpeningModels = CuttingElementSearch.OpeningModels.Where(x => !selectedItems.Contains(x)).ToList();
        }

        private void Btn_Click_SelectItems(object sender, RoutedEventArgs e)
        {
            CuttingElementSearch.Selection = new List<CutSearchOpeningModel>();
            if (MyTabControl.SelectedIndex == 0)
            {
                var selectedItems = MainDataGrid.SelectedItems;
                foreach (var item in selectedItems)
                {
                    CutSearchOpeningModel model = item as CutSearchOpeningModel;
                    CuttingElementSearch.Selection.Add(model);
                }
            }
            if (MyTabControl.SelectedIndex == 1)
            {
                var selectedItems = DataGridNotFound.SelectedItems;
                foreach (var item in selectedItems)
                {
                    CutSearchOpeningModel model = item as CutSearchOpeningModel;
                    CuttingElementSearch.Selection.Add(model);
                }
            }
            if (MyTabControl.SelectedIndex == 2)
            {
                var selectedItems = DataGridOther.SelectedItems;
                foreach (var item in selectedItems)
                {
                    CutSearchOpeningModel model = item as CutSearchOpeningModel;
                    CuttingElementSearch.Selection.Add(model);
                }
            }
            CuttingElementSearch.ToolAction = CutElementToolAction.SelectItems;
            CuttingElementSearch.TheEvent.Raise();
        }

        private void Btn_Click_RemoveFromList(object sender, RoutedEventArgs e)
        {
            if(MyTabControl.SelectedIndex == 0)
            {
                var selectedItems = MainDataGrid.SelectedItems;
                CuttingElementSearch.OpeningModels = CuttingElementSearch.OpeningModels.Where(x => !selectedItems.Contains(x)).ToList();
            }
            if (MyTabControl.SelectedIndex == 1)
            {
                var selectedItems = DataGridNotFound.SelectedItems;
                CuttingElementSearch.NotFoundModels = CuttingElementSearch.NotFoundModels.Where(x => !selectedItems.Contains(x)).ToList();
            }
            if (MyTabControl.SelectedIndex == 2)
            {
                var selectedItems = DataGridOther.SelectedItems;
                CuttingElementSearch.OtherModels = CuttingElementSearch.OtherModels.Where(x => !selectedItems.Contains(x)).ToList();
            }
        }

        private void MyTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(MyTabControl.SelectedIndex == 0)
            {
                BtnFixSelected.IsEnabled = true;
            }
            else
            {
                BtnFixSelected.IsEnabled = false;
            }
        }
    }
}
