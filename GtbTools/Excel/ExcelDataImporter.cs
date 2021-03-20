using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace GtbTools.Excel
{
    public class ExcelDataImporter : INotifyPropertyChanged
    {   
        public List<ExcelFileData> _fileList;
        public List<ExcelFileData> FileList
        {
            get => _fileList;
            set
            {
                if (_fileList != value)
                {
                    _fileList = value;
                    OnPropertyChanged(nameof(FileList));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        string _searchDirectory;
        string _filter;
        string[] _excelPathList;

        public ExcelDataImporter(string searchDirectory, string filter)
        {
            _searchDirectory = searchDirectory;
            _filter = filter;
            //CreateExcelPathList();
            //CreateFileList();
        }

        public void CreateExcelPathList()
        {
            _excelPathList = Directory.EnumerateFiles(_searchDirectory, "*.xlsx", SearchOption.AllDirectories).Take(20).ToArray();
            if (_excelPathList.Length == 20) MessageBox.Show("Fur die Sicherheit die Anzahl der Dateien wurde auf 20 begrentz.");
            //_excelPathList = Directory.GetFiles(_searchDirectory, "*.xlsx", SearchOption.AllDirectories);
            //MessageBox.Show(_excelPathList.Length.ToString());
        }

        public void CreateFileList()
        {
            FileList = new List<ExcelFileData>();
            foreach (string item in _excelPathList)
            {
                if(Path.GetFileName(item).ToUpper().Contains(_filter.ToUpper()))
                {
                    ExcelFileData excelFileData = new ExcelFileData();
                    excelFileData.DirectoryPath = Path.GetDirectoryName(item);
                    excelFileData.FileName = Path.GetFileName(item);
                    excelFileData.IsSelected = false;
                    FileList.Add(excelFileData);
                }
            }
            //MessageBox.Show(FileList.Count.ToString());
        }

        public void WriteValueToFiles(int row, int column, string value, string filePath, string sheetName)
        {
            ExcelCellValue excelCellValue = new ExcelCellValue();
            excelCellValue.Row = row;
            excelCellValue.Column = column;
            excelCellValue.Value = value;



            ExcelWrapper.WriteValuesToCells(excelCellValue, filePath, sheetName);
        }
    }
}
