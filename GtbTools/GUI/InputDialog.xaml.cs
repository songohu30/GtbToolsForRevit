using Functions;
using OwnerSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        SanRotation _sanRotation;
        bool _focused = false;
        double _rotationAngle;
        public InputDialog(SanRotation sanRotation)
        {
            _sanRotation = sanRotation;
            SetOwner();
            InitializeComponent();
            AngleBox.TextChanged += AngleBox_TextChanged;
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private bool ValidateInput()
        {
            string convert = AngleBox.Text.Replace(",", ".");
            bool isValid = double.TryParse(convert, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _rotationAngle);
            return isValid;
        }

        private void AngleBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(!_focused)
            {
                AngleBox.Text = "";
                _focused = true;
            }
        }

        private void AngleBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if(ValidateInput())
            {
                AngleBox.Foreground = Brushes.Black;
                OkButton.IsEnabled = true;
            }
            else
            {
                AngleBox.Foreground = Brushes.Red;
                OkButton.IsEnabled = false;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _sanRotation.RotationAngle = _rotationAngle;
            Close();
        }
    }
}
