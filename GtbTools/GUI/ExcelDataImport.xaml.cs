using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GtbTools.Excel;
using System.ComponentModel;

namespace GtbTools.GUI
{
    /// <summary>
    /// Interaction logic for ExcelDataImport.xaml
    /// </summary>
    public partial class ExcelDataImport : Window, INotifyPropertyChanged
    {
        public ExcelDataImporter _excelDataImporter;
        public ExcelDataImporter ExcelDataImporter
        {
            get => _excelDataImporter;
            set
            {
                if (_excelDataImporter != value)
                {
                    _excelDataImporter = value;
                    OnPropertyChanged(nameof(ExcelDataImporter));
                }
            }
        }
        public bool ClearSelectAllEnabled { get; set; } = false;
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

        bool _isValidSheetName = false;
        bool _isInteger = false;
        bool _isLetter = false;

        private bool _userAllowed;
        private bool _processApproved;

        public ExcelDataImport()
        {
            DataContext = this;
            Topmost = true;
            InitializeComponent();
            ProcessApproved = false;           
            _userAllowed = false;
            if (Environment.UserName == "m.trawczynski" || Environment.UserName == "f.schoesser" || Environment.UserName == "m.petersmann")
            {
                lblFunctionInactive.Visibility = Visibility.Hidden;
                _userAllowed = true;
            }
        }

        private bool SearchDirectories(string filter)
        {
            bool result = false;
            string searchDirectory = "";
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult dialogResult = dialog.ShowDialog();
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    searchDirectory = dialog.SelectedPath;
                    result = true;
                }
            }
            if (string.IsNullOrEmpty(searchDirectory)) return result;
            ExcelDataImporter = new ExcelDataImporter(searchDirectory, filter);
            return result;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string filter = TxtBoxSearch.Text;
            if(!SearchDirectories(filter)) return;
            ExcelDataImporter.CreateExcelPathList();
            ExcelDataImporter.CreateFileList();
            //MessageBox.Show(ExcelDataImporter.FileList.Count.ToString());
            if (ExcelDataImporter.FileList == null || ExcelDataImporter.FileList.Count == 0)
            {
                ClearSelectAllEnabled = false;
            }
            else
            {
                ClearSelectAllEnabled = true;
            }
            BtnClearAll.IsEnabled = ClearSelectAllEnabled;
            BtnSelectAll.IsEnabled = ClearSelectAllEnabled;
            //CheckBoxList.ItemsSource = ExcelDataImporter.FileList;
        }

        private void BtnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ExcelDataImporter.FileList == null || ExcelDataImporter.FileList.Count == 0)
            {
                MessageBox.Show("Nothing to process!");
                return;
            }

            int row = Convert.ToInt16(TxtBoxCellRow.Text); //TxtBoxCellRow.Text



            int column = ExcelWrapper.ConvertCellAddress(TxtBoxCellColumn.Text);


            string value = "";
            if (RadBtnFixedValue.IsChecked == true)
            {
                value = txtBoxFixedValue.Text;
            }

            foreach (ExcelFileData excelFileData in ExcelDataImporter.FileList)
            {
                if (excelFileData.IsSelected != true) continue;
                string filePath = System.IO.Path.Combine(excelFileData.DirectoryPath, excelFileData.FileName);
                if (RadBtnCombinedValue.IsChecked == true)
                {
                    string dirPath = "";
                    if (ChckBoxFolderLink.IsChecked == true) dirPath = excelFileData.DirectoryPath;
                    string fileName = "";
                    if (ChckBoxFileName.IsChecked == true) fileName = excelFileData.FileName;
                    string plusValue = TxtBoxFixedValuePlus.Text;
                    value = dirPath + fileName + plusValue;
                }
                if (value == "") MessageBox.Show("Value is empty");

                //MessageBox.Show(row.ToString());
                //MessageBox.Show(column.ToString());
                //MessageBox.Show(value);
                //MessageBox.Show(filePath);
                //MessageBox.Show(TxtBoxSheetName.Text);

                ExcelCellValue excelCellValue = new ExcelCellValue();
                excelCellValue.Row = row;
                excelCellValue.Column = column;
                excelCellValue.Value = value;

                ExcelWrapper.WriteValuesToCells(excelCellValue, filePath, TxtBoxSheetName.Text);
                //ExcelWrapper.ReadAllSheetsAsStringArrays(filePath);
                
            }
            MessageBox.Show("Mission complete");
            //ExcelDataImporter.WriteValueToFiles()
        }

        private bool IsLetter(string letter)
        {
            bool result = false;
            int errorCounter = Regex.Matches(letter, @"[a-zA-Z]").Count;
            if (errorCounter == 1 && letter.Length == 1)
            {
                result = true;
                _isLetter = true;
            }
            else
            {
                _isLetter = false;
            }
            return result;
        }

        private bool IsInteger(string number)
        {
            bool result = false;
            Match match = Regex.Match(number, @"^[1-9][0-9]?$|^100$");
            if (match.Success)
            {
                result = true;
                _isInteger = true;
            }
            else
            {
                _isInteger = false;

            }
            return result;
        }

        private bool IsValidSheetName(string sheetName)
        {
            bool result = false;
            bool match = sheetName.Length > 0 && sheetName.Length < 32 && !sheetName.Contains(':') && !sheetName.Contains('/')
                            && !sheetName.Contains(@"\") && !sheetName.Contains('?') && !sheetName.Contains('*') && !sheetName.Contains('[') && !sheetName.Contains(']');
            if (match)
            {
                result = true;
                _isValidSheetName = true;
            }
            else
            {
                _isValidSheetName = false;
            }
            return result;
        }

        private void TxtBoxCellColumn_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsLetter(TxtBoxCellColumn.Text))
            {
                TxtBoxCellColumn.Foreground = Brushes.Black;
                TxtBoxCellColumn.BorderBrush = Brushes.Black;
            }
            else
            {
                TxtBoxCellColumn.Foreground = Brushes.Red;
                TxtBoxCellColumn.BorderBrush = Brushes.Red;
            }
        }

        private void TxtBoxCellRow_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsInteger(TxtBoxCellRow.Text))
            {
                TxtBoxCellRow.Foreground = Brushes.Black;
                TxtBoxCellRow.BorderBrush = Brushes.Black;
            }
            else
            {
                TxtBoxCellRow.Foreground = Brushes.Red;
                TxtBoxCellRow.BorderBrush = Brushes.Red;
            }
        }

        private void TxtBoxSheetName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsValidSheetName(TxtBoxSheetName.Text))
            {
                TxtBoxSheetName.Foreground = Brushes.Black;
                TxtBoxSheetName.BorderBrush = Brushes.Black;
            }
            else
            {
                TxtBoxSheetName.Foreground = Brushes.Red;
                TxtBoxSheetName.BorderBrush = Brushes.Red;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            myGrid.Focus();
            bool radioChecked = RadBtnFixedValue.IsChecked == true || RadBtnCombinedValue.IsChecked == true;
            ProcessApproved = _isInteger && _isLetter && _isValidSheetName && radioChecked && _userAllowed;
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (ExcelDataImporter.FileList == null || ExcelDataImporter.FileList.Count == 0) return;
            foreach (ExcelFileData item in ExcelDataImporter.FileList)
            {
                item.IsSelected = false;
            }
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (ExcelDataImporter.FileList == null || ExcelDataImporter.FileList.Count == 0) return;
            foreach (ExcelFileData item in ExcelDataImporter.FileList)
            {
                item.IsSelected = true;
            }
        }
    }
}
