using ExternalLinkControl;
using Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CadLinkViewsWindow.xaml
    /// </summary>
    public partial class CadLinkViewsWindow : Window
    {
        public CadLinkViewModel CadLinkViewModel { get; set; }
        public bool ApplyChanges = false;
        ExternalLinkTool _externalLinkTool;
        bool _allowChckboxEvent = true;

        public CadLinkViewsWindow(Window owner, CadLinkViewModel cadLinkViewModel, ExternalLinkTool externalLinkTool)
        {
            _externalLinkTool = externalLinkTool;
            Owner = owner;
            CadLinkViewModel = cadLinkViewModel;
            this.DataContext = CadLinkViewModel;
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (!comboBox.IsDropDownOpen) return;
            int selectedIndex = comboBox.SelectedIndex;
            foreach (CadViewModel model in DataGridViews.SelectedItems)
            {
                model.SelectedIndex = selectedIndex;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_allowChckboxEvent == false) return;
            foreach (CadViewModel model in DataGridViews.SelectedItems)
            {
                if (!model.IsVisible) model.IsVisible = true;
            }
            _allowChckboxEvent = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_allowChckboxEvent == false) return;
            foreach (CadViewModel model in DataGridViews.SelectedItems)
            {
                if (model.IsVisible) model.IsVisible = false;
            }
            _allowChckboxEvent = false;
        }

        private void BtnClick_ClearAll(object sender, RoutedEventArgs e)
        {
            foreach (CadViewModel model in DataGridViews.Items)
            {
                if (model.IsVisible) model.IsVisible = false;
            }
        }

        private void BtnClick_SelectAll(object sender, RoutedEventArgs e)
        {
            foreach (CadViewModel model in DataGridViews.Items)
            {
                if (!model.IsVisible) model.IsVisible = true;
            }
        }

        private void BtnClick_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnClick_Apply(object sender, RoutedEventArgs e)
        {
            _externalLinkTool.ExternalLinkToolViewModel.EditedCadLinkViewModel = CadLinkViewModel;
            _externalLinkTool.Action = ExternalLinkToolAction.ModifyCadLink;
            _externalLinkTool.TheEvent.Raise();
            _externalLinkTool.SignalEvent.WaitOne();
            _externalLinkTool.SignalEvent.Reset();
            CadLinkViewModel.UpdateModel();
        }

        private void BtnClick_OK(object sender, RoutedEventArgs e)
        {
            _externalLinkTool.ExternalLinkToolViewModel.EditedCadLinkViewModel = CadLinkViewModel;
            _externalLinkTool.Action = ExternalLinkToolAction.ModifyCadLink;
            _externalLinkTool.TheEvent.Raise();
            _externalLinkTool.SignalEvent.WaitOne();
            _externalLinkTool.SignalEvent.Reset();
            this.Close();
        }

        private void CheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _allowChckboxEvent = true;
        }

        private void TxtBoxSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtBoxSearch.Text == "Filtern")
            {
                TxtBoxSearch.Clear();
                TxtBoxSearch.Foreground = Brushes.Black;
            }
        }

        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TxtBoxSearch.IsFocused)
            {
                MainGrid.Focus();
                if (TxtBoxSearch.Text == "")
                {
                    TxtBoxSearch.Text = "Filtern";
                    TxtBoxSearch.Foreground = Brushes.Gray;
                }
                else
                {
                    TxtBoxSearch.Foreground = Brushes.Black;
                }
            }
        }

        private void TxtBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = TxtBoxSearch.Text;
            if (filter != "Filtern")
            {
                var myFilter = new Predicate<object>(item => ((CadViewModel)item).View.Name.ToUpper().Contains(filter.ToUpper()));
                DataGridViews.Items.Filter = myFilter;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataGridViews.Items.Filter = null;
        }
    }
}
