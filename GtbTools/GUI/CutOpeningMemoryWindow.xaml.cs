using Autodesk.Revit.DB;
using ExStorage;
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
using ViewModels;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CutOpeningMemory.xaml
    /// </summary>
    public partial class CutOpeningMemoryWindow : Window
    {
        public DurchbruchMemoryViewModel DurchbruchMemoryViewModel { get; set; }
        public CutOpeningMemory CutOpeningMemory { get; set; }
        public GtbWindowResult GtbWindowResult { get; set; }
        public CutOpeningMemoryWindow(CutOpeningMemory cutOpeningMemory)
        {
            DataContext = this;
            CutOpeningMemory = cutOpeningMemory;
            InitializeComponent();
        }

        public CutOpeningMemoryWindow(DurchbruchMemoryViewModel durchbruchMemoryViewModel)
        {
            DurchbruchMemoryViewModel = durchbruchMemoryViewModel;
            InitializeComponent();
        }

        public void UpdateDurchbruchViewModel()
        {
            DurchbruchMemoryViewModel.LoadContextEvent.Raise();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GtbWindowResult = GtbWindowResult.Apply;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GtbSchema gtbSchema = new GtbSchema();
            gtbSchema.SetGtbSchema();
            Transaction tx = new Transaction(CutOpeningMemory._doc, "Saving openings config");
            tx.Start();
            foreach (OpeningMemory mem in CutOpeningMemory.openingMemories)
            {
                gtbSchema.SetEntityField(mem._familyInstance, "positionXYZ", mem.NewPosition);
                gtbSchema.SetEntityField(mem._familyInstance, "dimensions", mem.NewDimensions);
                gtbSchema.SetEntityField(mem._familyInstance, "dateSaved", mem.NewDateSaved);
            }
            tx.Commit();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int elid = int.Parse(TxtBoxId.Text);
            ElementId elementId = new ElementId(elid);
            DurchbruchMemoryViewModel.CurrentSelection = elementId;
            DurchbruchMemoryViewModel.LoadContextEvent.Raise();
            //DurchbruchMemoryViewModel.ShowElementEvent.Raise();
        }
    }
}
