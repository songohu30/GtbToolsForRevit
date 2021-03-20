using Functions;
using OpeningSymbol;
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
    /// Interaction logic for OpeningsMainWindow.xaml
    /// </summary>
    public partial class OpeningsMainWindow : Window, INotifyPropertyChanged
    {
        public OpeningWindowMainViewModel OpeningWindowMainViewModel { get; set; }
        public OpeningSymbolTool OpeningSymbolTool { get; set; }

        private bool _processApproved;
        public bool ProcessApproved
        {
            get => _processApproved;
            set
            {
                if (_processApproved != value)
                {
                    _processApproved = value;
                    OnPropertyChanged(nameof(ProcessApproved));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public OpeningsMainWindow(OpeningWindowMainViewModel openingWindowMainViewModel)
        {
            OpeningWindowMainViewModel = openingWindowMainViewModel;
            ProcessApproved = false;
            InitializeComponent();
            DataContext = this;
            Topmost = true;
        }

        private void BtnSelectPlans_Click(object sender, RoutedEventArgs e)
        {
            OpeningWindowMainViewModel.SelectAllPlanViews();
        }

        private void BtnClearPlans_Click(object sender, RoutedEventArgs e)
        {
            OpeningWindowMainViewModel.ClearAllPlanViews();
        }

        private void BtnSelectSections_Click(object sender, RoutedEventArgs e)
        {
            OpeningWindowMainViewModel.SelectAllSectionViews();
        }

        private void BtnClearSections_Click(object sender, RoutedEventArgs e)
        {
            OpeningWindowMainViewModel.ClearAllSectionViews();
        }

        private void RadBtnArc_Checked(object sender, RoutedEventArgs e)
        {
            OpeningWindowMainViewModel.ViewDiscipline = ViewDiscipline.TWP;
            ProcessApproved = true;
        }

        private void RadBtnTga_Checked(object sender, RoutedEventArgs e)
        {
            OpeningWindowMainViewModel.ViewDiscipline = ViewDiscipline.TGA;
            ProcessApproved = true;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            OpeningSymbolTool = OpeningSymbolTool.Initialize(OpeningWindowMainViewModel);
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            OpeningWindowMainViewModel = null;
            this.Close();
        }
    }
}
