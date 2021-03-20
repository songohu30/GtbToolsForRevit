using GtbTools;
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
    /// Interaction logic for DurchbruchViews.xaml
    /// </summary>
    public partial class DurchbruchViews : Window
    {

        public List<ModelView> Views { get; set; }
        public DurchbruchMemoryViewModel DurchbruchMemoryViewModel { get; set; }
        public DurchbruchViews(List<ModelView> views, Window owner)
        {
            Owner = owner;
            Views = views;
            InitializeComponent();
            MyComboBox.DataContext = this;
        }

        private void Btn_Click_ShowView(object sender, RoutedEventArgs e)
        {
            if (MyComboBox.SelectedIndex < 0) return;
            ModelView mv = Views[MyComboBox.SelectedIndex];
            DurchbruchMemoryViewModel.DesiredView = mv.View;
            DurchbruchMemoryViewModel.OpenViewEvent.Raise();
        }
    }
}
