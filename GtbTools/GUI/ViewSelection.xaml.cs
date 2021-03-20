using GtbTools;
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
    /// Interaction logic for ViewSelection.xaml
    /// </summary>
    public partial class ViewSelection : Window, INotifyPropertyChanged
    {
        public List<ModelView> PlanViews { get; set; }
        public List<ModelView> _sortedViews;
        public List<ModelView> SortedViews
        {
            get => _sortedViews;
            set
            {
                if (_sortedViews != value)
                {
                    _sortedViews = value;
                    OnPropertyChanged(nameof(SortedViews));
                }
            }
        }
        GtbWindowResult result;
        public event PropertyChangedEventHandler PropertyChanged;

        string _search = "";

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ViewSelection(List<ModelView> planViews, Window owner)
        {
            Owner = owner;
            this.DataContext = this;
            PlanViews = planViews;
            InitializeComponent();
            SortedViews = PlanViews.ToList();
        }

        private void BtnClick_Apply(object sender, RoutedEventArgs e)
        {
            result = GtbWindowResult.Apply;
            Close();
        }

        private void BtnClick_SelectAll(object sender, RoutedEventArgs e)
        {
            foreach (ModelView item in SortedViews)
            {
                item.IsSelected = true;
            }
        }

        private void BtnClick_ClearAll(object sender, RoutedEventArgs e)
        {
            foreach (ModelView item in SortedViews)
            {
                item.IsSelected = false;
            }
        }

        private void TxtBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _search = TxtBoxSearch.Text;
            SortedViews = PlanViews.Where(x => x.Name.ToUpper().Contains(_search.ToUpper())).ToList();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainGrid.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(result == GtbWindowResult.Apply)
            {
                
            }
            else
            {
                foreach (ModelView mv in PlanViews)
                {
                    mv.IsSelected = false;
                }
            }
            result = GtbWindowResult.Cancel;
        }
    }
}
