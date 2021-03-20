using Autodesk.Revit.UI;
using DurchbruchRotationFix;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for DurchbruchRotationFixViewModel.xaml
    /// </summary>
    public partial class DurchbruchRotationFixWindow: Window
    {
        public RotationFixViewModel RotationFixViewModel { get; set; }
        public WindowDecision WindowDecision { get; set; }
        public Functions.DurchbruchRotationFix DurchbruchRotationFix { get; set; }

        public DurchbruchRotationFixWindow(RotationFixViewModel rotationFixViewModel)
        {
            RotationFixViewModel = rotationFixViewModel;
            DataContext = RotationFixViewModel;
            InitializeComponent();
        }

        private void BtnNoSymbol_Click(object sender, RoutedEventArgs e)
        {
            string info = "";
            foreach (var item in RotationFixViewModel.NotVisibleEckigList)
            {
                info += item.FamilyInstance.Id.IntegerValue.ToString() + Environment.NewLine;
            }
            foreach (var item in RotationFixViewModel.NotVisibleRoundList)
            {
                info += item.FamilyInstance.Id.IntegerValue.ToString() + Environment.NewLine;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
            string name = "FamilyIds_" + date;

            dlg.FileName = name; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                File.WriteAllText(filename, info);
            }
        }

        private void BtnRot90_Click(object sender, RoutedEventArgs e)
        {
            string info = "";
            foreach (var item in RotationFixViewModel.Rotated90EckigList)
            {
                info += item.FamilyInstance.Id.IntegerValue.ToString() + Environment.NewLine;
            }
            foreach (var item in RotationFixViewModel.Rotated90RoundList)
            {
                info += item.FamilyInstance.Id.IntegerValue.ToString() + Environment.NewLine;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
            string name = "FamilyIds_" + date;

            dlg.FileName = name; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                File.WriteAllText(filename, info);
            }
        }

        private void BtnRot180_Click(object sender, RoutedEventArgs e)
        {
            string info = "";
            foreach (var item in RotationFixViewModel.Rotated180EckigList)
            {
                info += item.FamilyInstance.Id.IntegerValue.ToString() + Environment.NewLine;
            }
            foreach (var item in RotationFixViewModel.Rotated180RoundList)
            {
                info += item.FamilyInstance.Id.IntegerValue.ToString() + Environment.NewLine;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
            string name = "FamilyIds_" + date;

            dlg.FileName = name; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                File.WriteAllText(filename, info);
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            RotationFixViewModel.RotatedElementsToFix = new List<DurchbruchRotatedModel>();
            if(ChBoxNoSymbol.IsChecked == true)
            {
                foreach (var item in RotationFixViewModel.NotVisibleEckigList)
                {
                    RotationFixViewModel.RotatedElementsToFix.Add(item);
                }
                foreach (var item in RotationFixViewModel.NotVisibleRoundList)
                {
                    RotationFixViewModel.RotatedElementsToFix.Add(item);

                }
            }
            if (ChBoxRot90.IsChecked == true)
            {
                foreach (var item in RotationFixViewModel.Rotated90EckigList)
                {
                    RotationFixViewModel.RotatedElementsToFix.Add(item);
                }
                foreach (var item in RotationFixViewModel.Rotated90RoundList)
                {
                    RotationFixViewModel.RotatedElementsToFix.Add(item);

                }
            }
            if (ChBoxRot180.IsChecked == true)
            {
                foreach (var item in RotationFixViewModel.Rotated180EckigList)
                {
                    RotationFixViewModel.RotatedElementsToFix.Add(item);
                }
                foreach (var item in RotationFixViewModel.Rotated180RoundList)
                {
                    RotationFixViewModel.RotatedElementsToFix.Add(item);

                }
            }
            WindowDecision = WindowDecision.Apply;
            Close();
        }
    }
}
