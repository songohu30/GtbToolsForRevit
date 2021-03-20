using Autodesk.Revit.DB;
using GtbTools;
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
using ViewModels;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MemoryFunctionOptimisation.xaml
    /// </summary>
    public partial class MemoryFunctionOptimisation : Window, INotifyPropertyChanged
    {
        public OptimisationChoice OptimisationChoice { get; set; }
        public bool _selectEnabled;
        public bool SelectEnabled
        {
            get => _selectEnabled;
            set
            {
                if (_selectEnabled != value)
                {
                    _selectEnabled = value;
                    OnPropertyChanged(nameof(SelectEnabled));
                }
            }
        }
        public bool _applyEnabled;
        public bool ApplyEnabled
        {
            get => _applyEnabled;
            set
            {
                if (_applyEnabled != value)
                {
                    _applyEnabled = value;
                    OnPropertyChanged(nameof(ApplyEnabled));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        bool _isButtonClosed = false;
        public List<ModelView> PlanViews { get; set; }
        public List<ModelView> SelectedViews { get; set; }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MemoryFunctionOptimisation(string label1,string label2, List<ModelView> planViews)
        {
            SetOwner();
            DataContext = this;
            SelectEnabled = false;
            ApplyEnabled = false;
            InitializeComponent();
            LblHeader1.Content = label1;
            LblHeader2.Content = label2;
            PlanViews = planViews;
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void BtnClick_SelectViews(object sender, RoutedEventArgs e)
        {
            ViewSelection window = new ViewSelection(PlanViews, this);
            window.ShowDialog();
            SelectedViews = window.PlanViews.Where(x => x.IsSelected == true).ToList();
            if (SelectedViews.Count > 0)
            {
                ApplyEnabled = true;
            }
            else
            {
                ApplyEnabled = false;
            }
        }

        private void BtnClick_Apply(object sender, RoutedEventArgs e)
        {
            _isButtonClosed = true;
            Close();
        }

        private void RadBtnMedium_Checked(object sender, RoutedEventArgs e)
        {
            ApplyEnabled = true;
            OptimisationChoice = OptimisationChoice.Medium;
            //MessageBox.Show("Medium");
        }

        private void RadBtnSlow_Checked(object sender, RoutedEventArgs e)
        {
            ApplyEnabled = false;
            SelectEnabled = true;
            OptimisationChoice = OptimisationChoice.Slow;
            //MessageBox.Show("Slow");
        }

        private void RadBtnFast_Checked(object sender, RoutedEventArgs e)
        {
            ApplyEnabled = true;
            OptimisationChoice = OptimisationChoice.Fast;
            //MessageBox.Show("Fast");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(!_isButtonClosed)
            {
                OptimisationChoice = OptimisationChoice.None;
            }
        }
    }
}
