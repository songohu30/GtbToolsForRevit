using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Office.Interop.Excel;

namespace RaumBuch
{
    public class ExcelOperations
    {
        public ExcelOperations()
        {

        }

		public static Dictionary<string, List<int>> GetExcelDataModel(string path, out Dictionary<string, string> dictConstants)
		{
			dictConstants = new Dictionary<string, string>();
			Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
			Application excelApp = new Application();
			Workbook excelBook = excelApp.Workbooks.Open(path);
			Worksheet worksheet = (Worksheet)excelBook.ActiveSheet;
			if((string)worksheet.Cells[1, 1].Text == "GTB_EXP_01")
            {
				int endIndex = 417;
				for (int i = 4; i < endIndex; i++)
				{
					string value5 = (string)worksheet.Cells[i, 5].Text; //cell address
					string value6 = (string)worksheet.Cells[i, 6].Text; //symbolId
					if (value5 == null || value5 == "0" || value5 == "x" || value5 == "") continue;
					if (value6 == null || value6 == "0" || value6 == "x" || value6 == "") continue;

					int integer = 0;
					if (Int32.TryParse(value6, out integer))
					{
						if (result.ContainsKey(value5))
						{
							result[value5].Add(integer);
						}
						else
						{

							List<int> list = new List<int>() { integer };
							result.Add(value5, list);
						}
					}
				}
				Worksheet constants = excelBook.Sheets["Parameters&Constants"];
                for (int i = 3; i < 101; i++)
                {
					string cell = (string)constants.Cells[i, 2].Text;
					string value = (string)constants.Cells[i, 3].Text;
					if (cell == null || cell == "") continue;
					if (value == null) value = "";
					dictConstants.Add(cell, value);
                }

				Marshal.ReleaseComObject(constants);
				Marshal.ReleaseComObject(worksheet);
				excelBook.Close(false);
				excelApp.Quit();
				return result;
			}
			else
            {
				Marshal.ReleaseComObject(worksheet);
				excelBook.Close(false);
				excelApp.Quit();
				TaskDialog.Show("Info", "Die ausgewählte Datei ist falsch!");
				return null;
            }

		}

		public static string WriteToSheets(List<ExportedRoom> exportedRooms, string templatePath, Dictionary<string, string> constants)
        {
			string result = templatePath + Environment.NewLine;
			Application excelApp = new Application();
			Workbook excelBook = excelApp.Workbooks.Open(templatePath);
			foreach (Worksheet worksheet in excelBook.Sheets)
            {
				int rowCorrection = 0;
				if (!TestTheSheet(worksheet, out rowCorrection))
				{
					result += "Tabelle: " + worksheet.Name + ", hat ein anderes Format. Berechnete Zeilenkorrektur ist: " + rowCorrection.ToString() + Environment.NewLine;
					if (rowCorrection == -1) continue;
				}
				ExportedRoom exportedRoom = exportedRooms.Where(e => e.MepRoomNumber == worksheet.Name).FirstOrDefault();
				if(exportedRoom == null) exportedRoom = exportedRooms.Where(e => e.MepSpaceNumber == worksheet.Name).FirstOrDefault();
				if (exportedRoom == null)
                {
					result += "Tabelle: " + worksheet.Name + ", kann keinen entsprechenden MepRaum im Projekt finden." + Environment.NewLine;
					continue;
				}
				// if goes through tests then fill up the sheet
				int kaltwasser = 0;
				int warmwasser = 0;
				string kaltWasFlow = "";
				string warmWasFlow = "";
				foreach (KeyValuePair<string, List<FamilyInstance>> pair in exportedRoom.ExportItems)
                {
					List<int> rowcol = ReadCellAddress(pair.Key);
					int row = rowcol[0] + rowCorrection;
					int column = rowcol[1];
					if(pair.Value.Count == 0)
                    {
						worksheet.Cells[row, column].HorizontalAlignment = XlHAlign.xlHAlignCenter;
						continue;
					}
					string fillText = pair.Value.Count.ToString();
					worksheet.Cells[row, column] = fillText;
					worksheet.Cells[row, column].HorizontalAlignment = XlHAlign.xlHAlignCenter;
					if (pair.Key == "H69" || pair.Key == "H82" || pair.Key == "H84")
					{
						kaltwasser += pair.Value.Count;
						if(pair.Key == "H69")
                        {
                            foreach (FamilyInstance fi in pair.Value)
                            {
								if(fi.Id.IntegerValue == 2823259 || fi.Id.IntegerValue == 13112762)
								{
									kaltWasFlow += "+0.07";
								}
								else
                                {
									kaltWasFlow += "+0.07";
									warmWasFlow += "+0.07";
								}

							}
						}
						if (pair.Key == "H82")
						{
							kaltWasFlow += "+" + pair.Value.Count.ToString() + "*0.15";
							warmWasFlow += "+" + pair.Value.Count.ToString() + "*0.15";
						}
						if (pair.Key == "H84")
						{
							kaltWasFlow += "+" + pair.Value.Count.ToString() + "*0.1";
						}
					}
					if (pair.Key == "H69" || pair.Key == "H82")
					{
						warmwasser += pair.Value.Count;
					}
				}
				if(kaltwasser > 0)
                {
					List<int> rowcol = ReadCellAddress("H87");
					int row = rowcol[0] + rowCorrection;
					int column = rowcol[1];
					string fillText = kaltwasser.ToString();
					worksheet.Cells[row, column] = fillText;
					worksheet.Cells[row, column].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    //add formula to calculate kaltwasser flow
                    string flow = "=" + kaltWasFlow.Substring(1);
                    worksheet.Cells[row, column + 1].Formula = flow;
                    worksheet.Cells[row, column + 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    //check that abwasser is in the room
                    worksheet.Cells[row + 9, column] = "x";
                    worksheet.Cells[row + 9, column].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                }
				if (warmwasser > 0)
				{
					List<int> rowcol = ReadCellAddress("H88");
					int row = rowcol[0] + rowCorrection;
					int column = rowcol[1];
					string fillText = warmwasser.ToString();
					worksheet.Cells[row, column] = fillText;
					worksheet.Cells[row, column].HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    //add formula to calculate kaltwasser flow
                    string flow = "=" + warmWasFlow.Substring(1);
                    worksheet.Cells[row, column + 1].Formula = flow;
                    worksheet.Cells[row, column + 1].HorizontalAlignment = XlHAlign.xlHAlignCenter;
                }
				WriteConstants(worksheet, constants, rowCorrection);
				WriteParameters(worksheet, exportedRoom, rowCorrection);
				Marshal.ReleaseComObject(worksheet);
			}
			string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
			string directory = Path.GetDirectoryName(templatePath);
			DirectoryInfo dirInfo = Directory.CreateDirectory(Path.Combine(directory, "GTB_ExportedTemplates"));
			string fileName = Path.GetFileNameWithoutExtension(templatePath) + "_exported_" + date + ".xlsx";
			string fullExportPath = Path.Combine(dirInfo.FullName, fileName);
			excelBook.SaveAs(fullExportPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, 
								Type.Missing, Type.Missing, false, false, XlSaveAsAccessMode.xlNoChange,
									Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
			excelApp.Quit();
			return result;
		}

		private static bool TestTheSheet(Worksheet worksheet, out int correction)
        {
			correction = -1;
			bool result = false;
			string value1 = (string)worksheet.Cells[174, 3].Text;

			if (value1 != null)
			{
				if (value1.Contains("RJ45"))
				{
					result = true;
					correction = 0;
				}
				else
                {
					result = false;
					object[,] ar = worksheet.UsedRange.Value;
					int rows = worksheet.UsedRange.Rows.Count;
					for (int i = 1; i < rows + 1; i++)
					{
						string check = (string)worksheet.Cells[i, 3].Text;
						if (check == null) continue;
						if (check == "2-fach Datendose RJ45 KAT 6")
						{
							correction = i - 174;
						}
					}
				}
			}
			else
            {
				result = false;
				object[,] ar = worksheet.UsedRange.Value;
				int rows = worksheet.UsedRange.Rows.Count;
                for (int i = 1; i < rows + 1; i++)
                {
					string check = (string)worksheet.Cells[i, 3].Text;
					if (check == null) continue;
					if(check == "2-fach Datendose RJ45 KAT 6")
                    {
						correction = i - 174;
                    }
				}
			}
			return result;
				
					
		}

		private static List<int> ReadCellAddress(string address)
        {
			List<int> result = new List<int>();
			char columnLetter = address[0];
			int columnNo = char.ToUpper(columnLetter) - 64;
			string rowString = address.Substring(1);
			int rowNo = 0;
			Int32.TryParse(rowString, out rowNo);
			result.Add(rowNo);
			result.Add(columnNo);
			return result;
        }

		private static void WriteConstants(Worksheet worksheet, Dictionary<string, string> constants, int rowCorrection)
        {
            foreach (KeyValuePair<string, string> pair in constants)
            {
				List<int> rowcol = ReadCellAddress(pair.Key);
				int row = rowcol[0] + rowCorrection;
				int column = rowcol[1];
				worksheet.Cells[row, column] = pair.Value;
				worksheet.Cells[row, column].HorizontalAlignment = XlHAlign.xlHAlignCenter;
			}
        }

		private static void WriteParameters(Worksheet worksheet, ExportedRoom space, int rowCorrection)
        {
			//abluft H53
			Autodesk.Revit.DB.Parameter abluft = space.MepRoom.get_Parameter(BuiltInParameter.ROOM_ACTUAL_EXHAUST_AIRFLOW_PARAM);
			if(abluft != null)
            {
				double abluftValue = abluft.AsDouble();
				double abluftValueMetric = UnitUtils.ConvertFromInternalUnits(abluftValue, DisplayUnitType.DUT_CUBIC_METERS_PER_HOUR);
				List<int> abRowCol = ReadCellAddress("H53");
				int abrow = abRowCol[0] + rowCorrection;
				int abcolumn = abRowCol[1];
				worksheet.Cells[abrow, abcolumn] = abluftValueMetric.ToString();
				worksheet.Cells[abrow, abcolumn].HorizontalAlignment = XlHAlign.xlHAlignCenter;
			}

			//zuluft H54
			Autodesk.Revit.DB.Parameter zuluft = space.MepRoom.get_Parameter(BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM);
			if(zuluft != null)
            {
				double zuluftValue = zuluft.AsDouble();
				double zuluftValueMetric = UnitUtils.ConvertFromInternalUnits(zuluftValue, DisplayUnitType.DUT_CUBIC_METERS_PER_HOUR);
				List<int> zuRowCol = ReadCellAddress("H54");
				int zurow = zuRowCol[0] + rowCorrection;
				int zucolumn = zuRowCol[1];
				worksheet.Cells[zurow, zucolumn] = zuluftValueMetric.ToString();
				worksheet.Cells[zurow, zucolumn].HorizontalAlignment = XlHAlign.xlHAlignCenter;
			}

			//heizung temp O45
			Autodesk.Revit.DB.Parameter raumTemparaturHeizung = space.MepRoom.get_Parameter(new Guid("11f8c67d-73c6-4e07-bb62-b941038dd3fa"));
			if(raumTemparaturHeizung != null)
            {
				double raumTemperatur = raumTemparaturHeizung.AsDouble();
				double raumTemperaturMetric = UnitUtils.ConvertFromInternalUnits(raumTemperatur, DisplayUnitType.DUT_CELSIUS);
				List<int> htRowCol = ReadCellAddress("O45");
				int htrow = htRowCol[0] + rowCorrection;
				int htcolumn = htRowCol[1];
				worksheet.Cells[htrow, htcolumn] = raumTemperaturMetric.ToString();
				worksheet.Cells[htrow, htcolumn].HorizontalAlignment = XlHAlign.xlHAlignCenter;
			}
		}
	}
}
