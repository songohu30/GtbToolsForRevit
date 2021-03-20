using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace GtbTools.Excel
{
    class ExcelWrapper
    {
        public ExcelWrapper()
        {

        }
        public static IDictionary<string, object[,]> ReadAllSheetsAsStringArrays(string excelFilePath)
        {
            IDictionary<string, object[,]> result = new Dictionary<string, object[,]>();
            Application excelApp = new Application();
            Workbook excelBook = excelApp.Workbooks.Open(excelFilePath);

            foreach (Worksheet sheet in excelBook.Worksheets)
            {
                object[,] ar = sheet.UsedRange.Value;
                result.Add(sheet.Name, ar);
                Marshal.ReleaseComObject(sheet);
            }
            excelBook.Close();
            excelApp.Quit();
            return result;
        }
        public static void WriteValuesToCells(List<ExcelCellValue> values, string excelFilePath, string sheetName)
        {
            Application excelApp = new Application();
            Workbook excelBook = excelApp.Workbooks.Open(excelFilePath);
            Worksheet worksheet = excelBook.ActiveSheet;
            


            foreach (ExcelCellValue value in values)
            {
                worksheet.Cells[value.Row, value.Column] = value.Value;
            }
            Marshal.ReleaseComObject(worksheet);
            excelBook.Close();
            excelApp.Quit();
        }

        public static void WriteValuesToCells(ExcelCellValue value, string excelFilePath, string sheetName)
        {
            Application excelApp = new Application();
            Workbook excelBook = excelApp.Workbooks.Open(excelFilePath);
            Worksheet worksheet = excelBook.ActiveSheet;

            if (worksheet.Name != sheetName)
            {
                System.Windows.MessageBox.Show("Active sheet name is not: " + sheetName + System.Environment.NewLine + excelFilePath);
                Marshal.ReleaseComObject(worksheet);
                excelBook.Close();
                excelApp.Quit();
                return;
            }

            worksheet.Cells[value.Row, value.Column] = value.Value;
            Marshal.ReleaseComObject(worksheet);
            excelBook.Close(true);
            excelApp.Quit();
        }
        
        public static int ConvertCellAddress(string cellColumnLetter)
        {
            int column = 0;

            if(cellColumnLetter.Length > 1)
            {
                System.Windows.MessageBox.Show("Only single letters allowed");
                return 0;
            }

            char letter = System.Convert.ToChar(cellColumnLetter);
            column = char.ToUpper(letter) - 64;

            return column;
        }
    }
}
