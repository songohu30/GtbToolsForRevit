using Autodesk.Revit.DB;
using Functions;
using OwnerSearch;
using PipeFlowTool;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    /// <summary>
    /// Interaction logic for PipeFlowTagWindow.xaml
    /// </summary>
    public partial class PipeFlowTagWindow : Window
    {
        public PipeFlowTagger PipeFlowTagger { get; set; }

        public PipeFlowTagWindow(PipeFlowTagger pipeFlowTagger)
        {
            SetOwner();
            PipeFlowTagger = pipeFlowTagger;
            InitializeComponent();
            SetComBoxes();
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void SetComBoxes()
        {
            ComBoxViaUp.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxViaUp.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxViaUp, "durchgehend", "auf");

            ComBoxViaDown.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxViaDown.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxViaDown, "durchgehend", "ab");

            ComBoxNachOben.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxNachOben.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxNachOben, "nach", "oben");

            ComBoxNachUnten.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxNachUnten.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxNachUnten, "nach", "unten");

            ComBoxVonOben.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxVonOben.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxVonOben, "von", "oben");

            ComBoxVonUnten.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxVonUnten.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxVonUnten, "von", "unten");

            ComBoxDurchVent.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxDurchVent.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxDurchVent, "ventilation", "durchgehend");

            ComBoxNachObenVent.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxNachObenVent.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxNachObenVent, "ventilation", "oben");

            ComBoxVonUntenVent.ItemsSource = PipeFlowTagger.PipeFittingTags;
            ComBoxVonUntenVent.DisplayMemberPath = "Name";
            SetSelectedValueMitDN(ComBoxVonUntenVent, "ventilation", "unten");
        }

        private void SetSelectedValueMitDN(ComboBox cbox, string filter1, string filter2)
        {
            FamilySymbol familySymbol = PipeFlowTagger.PipeFittingTags.Where(e => e.Family.Name.ToUpper().Contains("MIT") && e.Name.ToUpper().Contains(filter1.ToUpper()) && e.Name.ToUpper().Contains(filter2.ToUpper())).FirstOrDefault();
            cbox.SelectedItem = familySymbol;
        }

        private void Btn_Click_Analyze(object sender, RoutedEventArgs e)
        {
            PipeFlowTagger.Action = PipeFlowToolAction.Analyze;
            PipeFlowTagger.StartEvent.Raise();
            PipeFlowTagger.SignalEvent.WaitOne();
            PipeFlowTagger.SignalEvent.Reset();
            PipeFlowTagger.SelectedTags = new List<FamilySymbol>();
            try
            {
                PipeFlowTagger.SelectedTags.Add(ComBoxViaUp.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxViaDown.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxNachOben.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxNachUnten.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxVonOben.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxVonUnten.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxDurchVent.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxNachObenVent.SelectedItem as FamilySymbol);
                PipeFlowTagger.SelectedTags.Add(ComBoxVonUntenVent.SelectedItem as FamilySymbol);

                Btn_TagThem.IsEnabled = true;
                //sort the list of tags
                PipeFlowTagger.SetTaggedElementIds();
                PipeFlowTagger.CheckExistingTags();
                PipeFlowTagger.SetViewModelList();
                MyDataGrid.DataContext = PipeFlowTagger;
                MyDataGrid.Items.Refresh();
            }
            catch
            {
                MessageBox.Show("Select all types!");
            }
        }

        private void Btn_Click_TagThemAll(object sender, RoutedEventArgs e)
        {
            PipeFlowTagger.Action = PipeFlowToolAction.Tag;
            PipeFlowTagger.StartEvent.Raise();
            PipeFlowTagger.SignalEvent.WaitOne();
            PipeFlowTagger.SignalEvent.Reset();
            Btn_TagThem.IsEnabled = false; ;
        }

        private void Btn_Click_DefaultDirections(object sender, RoutedEventArgs e)
        {
            CustomDirectionsWindow window = new CustomDirectionsWindow(PipeFlowTagger.DefaultDirections, this);
            window.ShowDialog();
        }

        private void MyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LineViewModel item = MyDataGrid.SelectedItem as LineViewModel;
            if (item == null) return;
            PipeFlowTagger.SelectedItem = item.ReferencePipeId;
            PipeFlowTagger.Action = PipeFlowToolAction.Show;
            PipeFlowTagger.StartEvent.Raise();
        }

        private void Btn_Click_SelectedWithoutDN(object sender, RoutedEventArgs e)
        {
            PipeFlowTagger.Action = PipeFlowToolAction.ChangeToNonDN;
            PipeFlowTagger.StartEvent.Raise();
        }

        private void Btn_Click_AddManualSymbols(object sender, RoutedEventArgs e)
        {
            PipeFlowTagger.ManualTag = ComBoxManualTags.SelectedItem as FamilySymbol;
            PipeFlowTagger.Action = PipeFlowToolAction.AddManualSymbols;
            PipeFlowTagger.StartEvent.Raise();
        }

        private void ComBoxManualTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnManualTags.IsEnabled = true;
        }

        private void RadBtnMit_Checked(object sender, RoutedEventArgs e)
        {
            ComBoxManualTags.IsEnabled = true;
            ComBoxManualTags.ItemsSource = PipeFlowTagger.PipeFittingTags.Where(x => x.Family.Name == "XXX Strang mit DN").ToList();
            ComBoxManualTags.DisplayMemberPath = "Name";
        }

        private void RadBtnOhne_Checked(object sender, RoutedEventArgs e)
        {
            ComBoxManualTags.IsEnabled = true;
            ComBoxManualTags.ItemsSource = PipeFlowTagger.PipeFittingTags.Where(x => x.Family.Name == "XXX Strang ohne DN").ToList();
            ComBoxManualTags.DisplayMemberPath = "Name";
        }
    }
}
