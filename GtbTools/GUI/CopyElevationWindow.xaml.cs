using Functions;
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
    /// Interaction logic for CopyElevationWindow.xaml
    /// </summary>
    public partial class CopyElevationWindow : Window
    {
        GetSetElevation _getSetElevation;
        public CopyElevationWindow(GetSetElevation getSetElevation)
        {
            _getSetElevation = getSetElevation;
            SetOwner();
            InitializeComponent();
            SetLists();
        }

        private void SetLists()
        {
            List<string> list = new List<string>() { "Center", "Top", "Bottom" };
            ComboBoxRectangular.ItemsSource = list;
            ComboBoxRectangular.SelectedIndex = 0;
            ComboBoxRound.ItemsSource = list;
            ComboBoxRound.SelectedIndex = 0;
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxRound.SelectedIndex == 0)
            {
                _getSetElevation.SetCenterToRound();
            }
            if (ComboBoxRound.SelectedIndex == 1)
            {
                _getSetElevation.SetTopToRound();
            }
            if (ComboBoxRound.SelectedIndex == 2)
            {
                _getSetElevation.SetBottomToRound();
            }

            if(ComboBoxRectangular.SelectedIndex == 0)
            {
                _getSetElevation.SetCenterToRectangular();
            }
            if (ComboBoxRectangular.SelectedIndex == 1)
            {
                _getSetElevation.SetTopToRectangular();
            }
            if (ComboBoxRectangular.SelectedIndex == 2)
            {
                _getSetElevation.SetBottomToRectangular();
            }
            Close();
        }
    }
}
