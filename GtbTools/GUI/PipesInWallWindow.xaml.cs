using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Functions;
using OwnerSearch;
using PipesInWall;
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
    /// Interaction logic for PipesInWallWindow.xaml
    /// </summary>
    public partial class PipesInWallWindow : Window
    {
        PipesInWallSearch PipesInWallSearch { get; set; }

        public PipesInWallWindow(PipesInWallSearch pipesInWallSearch)
        {
            PipesInWallSearch = pipesInWallSearch;
            this.DataContext = pipesInWallSearch;
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void Btn_Click_ArcAnalyze(object sender, RoutedEventArgs e)
        {
            //logic
            PipesInWallSearch.PipesInWallViewModel.GetSelectedWallTypes();
            PipesInWallSearch.PipesInWallViewModel.GetAllWallInstances();
            BtnTgaAnalyze.IsEnabled = true;
            MessageBox.Show("Architecture model has been analyzed!");
        }

        private void ComboBoxLinks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PipesInWallSearch.PipesInWallViewModel.SelectedLink = ComboBoxLinks.SelectedItem as RevitLinkInstance;
            PipesInWallSearch.PipesInWallViewModel.SetWallFamilies();
            WallFamiliesBox.ItemsSource = PipesInWallSearch.PipesInWallViewModel.WallFamilies;
            BtnArcAnalyze.IsEnabled = true;
            BtnTgaAnalyze.IsEnabled = false;
            BtnApply.IsEnabled = false;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            //logic
            LblProcessing.Visibility = System.Windows.Visibility.Visible;
            PipesInWallSearch.Action = PipesInWallAction.Apply;
            PipesInWallSearch.TheEvent.Raise();
            PipesInWallSearch.SignalEvent.WaitOne();
            PipesInWallSearch.SignalEvent.Reset();
            LblProcessing.Visibility = System.Windows.Visibility.Hidden;
            MessageBox.Show("Pipes have been updated!");
        }

        private void BtnTgaAnalyze_Click(object sender, RoutedEventArgs e)
        {
            //logic
            LblProcessing.Visibility = System.Windows.Visibility.Visible;
            PipesInWallSearch.PipesInWallViewModel.AnalyzePipes();
            DataGridControlList.ItemsSource = PipesInWallSearch.PipesInWallViewModel.PipeViewModels;
            BtnApply.IsEnabled = true;
            LblProcessing.Visibility = System.Windows.Visibility.Hidden;
        }

        private void DataGridControlList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PipeViewModel item = DataGridControlList.SelectedItem as PipeViewModel;
            if (item == null) return;
            PipesInWallSearch.PipesInWallViewModel.SelectedElement = item.PipeModel.ElementId;
            PipesInWallSearch.Action = PipesInWallAction.Show;
            PipesInWallSearch.TheEvent.Raise();
        }
    }
}
