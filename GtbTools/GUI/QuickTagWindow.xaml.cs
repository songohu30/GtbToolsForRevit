using Autodesk.Revit.DB;
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

namespace GUI
{
    /// <summary>
    /// Interaction logic for QuickTagWindow.xaml
    /// </summary>
    public partial class QuickTagWindow : Window
    {
        public GtbWindowResult WindowResult { get; set; }
        public ElementId WandSymbol { get; set; }
        public ElementId DeckenSymbol { get; set; }
        public ElementId BodenSymbol { get; set; }
        public bool IsOpeningModelLinked { get; set; } = false;
        public RevitLinkInstance SelectedLinkInstance { get; set; }

        List<FamilySymbol> _genericModelTags { get; set; }
        List<TagSymbol> _tagSymbols;
        List<RevitLinkInstance> _links;

        public QuickTagWindow(List<FamilySymbol> genericModelTags, List<RevitLinkInstance> links)
        {
            _genericModelTags = genericModelTags;
            _links = links;
            SetOwner();
            InitializeComponent();
            ComBoxLinked.ItemsSource = _links;
            ComBoxLinked.DisplayMemberPath = "Name";           
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if(cmBoxBoden.SelectedIndex < 0 || cmBoxDecken.SelectedIndex < 0 || cmBoxWand.SelectedIndex < 0)
            {
                Close();
                return;
            }
            WandSymbol = _tagSymbols[cmBoxWand.SelectedIndex].Id;
            DeckenSymbol = _tagSymbols[cmBoxDecken.SelectedIndex].Id;
            BodenSymbol = _tagSymbols[cmBoxBoden.SelectedIndex].Id;
            WindowResult = GtbWindowResult.Apply;
            Close();
        }

        private void SetOwner()
        {
            WindowHandleSearch windowHandleSearch = WindowHandleSearch.MainWindowHandle;
            windowHandleSearch.SetAsOwner(this);
        }

        public void SetLists()
        {
            _tagSymbols = new List<TagSymbol>();
            foreach (FamilySymbol fs in _genericModelTags)
            {
                TagSymbol tagSymbol = new TagSymbol();
                tagSymbol.Name = fs.Name;
                tagSymbol.Id = fs.Id;
                _tagSymbols.Add(tagSymbol);
            }
            cmBoxWand.ItemsSource = _tagSymbols;
            cmBoxWand.DisplayMemberPath = "Name";
            cmBoxDecken.ItemsSource = _tagSymbols;
            cmBoxDecken.DisplayMemberPath = "Name";
            cmBoxBoden.ItemsSource = _tagSymbols;
            cmBoxBoden.DisplayMemberPath = "Name";

            int wand = -1;
            int decken = -1;
            int boden = -1;
            string search1 = "Wanddurchbruch";
            string search2 = "Bodendurchbruch";
            string search3 = "Deckendurchbruch";

            for (int i = 0; i < _tagSymbols.Count; i++)
            {
                string check = _tagSymbols[i].Name;
                if (check.ToUpper() == search1.ToUpper()) wand = i;
                if (check.ToUpper() == search2.ToUpper()) boden = i;
                if (check.ToUpper() == search3.ToUpper()) decken = i;
            }
            cmBoxWand.SelectedIndex = wand;
            cmBoxBoden.SelectedIndex = boden;
            cmBoxDecken.SelectedIndex = decken;
        }
        class TagSymbol
        {
            public string Name { get; set; }
            public ElementId Id { get; set; }
        }

        private void CheckboxLinked_Checked(object sender, RoutedEventArgs e)
        {
            ComBoxLinked.IsEnabled = true;
            IsOpeningModelLinked = true;
        }

        private void CheckboxLinked_Unchecked(object sender, RoutedEventArgs e)
        {
            ComBoxLinked.IsEnabled = false;
            IsOpeningModelLinked = false;
        }

        private void ComBoxLinked_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedLinkInstance = (RevitLinkInstance)ComBoxLinked.SelectedItem;
        }
    }
}
