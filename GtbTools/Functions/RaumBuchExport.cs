using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using GUI;
using RaumBuch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class RaumBuchExport
    {
        public List<ExportedRoom> ExportedRooms { get; set; }
        public Dictionary<string, string> Constants { get; set; }
        public string ExportExcelTemplatePath { get; set; }
        public string ExportDataModelPath { get; set; }

        Document _doc;
        List<Space> _mepSpaces;
        List<FamilyInstance> _allInstances;
        List<FamilyInstance> _relevantInstances;
        Dictionary<string, List<int>> _excelDataModel;

        private RaumBuchExport()
        {

        }

        public static RaumBuchExport Initialize(Document doc)
        {
            RaumBuchExport result = new RaumBuchExport();
            result._doc = doc;
            result.GetAllFamilyInstances();
            result.SetMepSpaces();
            return result;
        }

        public void DisplayWindow()
        {
            AokRaumbuchExport window = new AokRaumbuchExport(this);
            window.ShowDialog();
        }

        public void SaveToExcel()
        {
            string content = ExcelOperations.WriteToSheets(ExportedRooms, ExportExcelTemplatePath, Constants);
            string directory = Path.GetDirectoryName(ExportExcelTemplatePath);
            string logName = Path.GetFileNameWithoutExtension(ExportExcelTemplatePath) + ".log";
            string saveLogPath = Path.Combine(directory, logName);
            File.WriteAllText(saveLogPath, content);
        }

        private void GetAllFamilyInstances()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_doc);
            _allInstances = ficol.OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).ToList();
            _relevantInstances = _allInstances.Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures ||
                                                                e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FireAlarmDevices ||
                                                                    e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_NurseCallDevices ||
                                                                        e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_SecurityDevices ||
                                                                            e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DataDevices ||
                                                                                e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalFixtures ||
                                                                                    e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures ||
                                                                                        e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment ||
                                                                                            e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CommunicationDevices ||
                                                                                                e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment).ToList();                                                                                                   
        }

        private void SetMepSpaces()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_doc);
            _mepSpaces = ficol.OfCategoryId(new ElementId((int)BuiltInCategory.OST_MEPSpaces)).Select(e => e as Space).ToList();
        }

        public bool SetExcelDataModel()
        {
            Dictionary<string, string> constants = new Dictionary<string, string>();
            _excelDataModel = ExcelOperations.GetExcelDataModel(ExportDataModelPath, out constants);
            Constants = constants;
            if(_excelDataModel == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetExportedRooms()
        {            
            ExportedRooms = new List<ExportedRoom>();
            foreach (Space mepSpace in _mepSpaces)
            {
                ExportedRoom exportedRoom = new ExportedRoom(mepSpace, _relevantInstances);
                exportedRoom.SortContainingElements(_excelDataModel);
                ExportedRooms.Add(exportedRoom);
            }
        }

        public void SaveExportedRoomsAs()
        {
            string content = "Detailed extract:" + Environment.NewLine;
            foreach (ExportedRoom er in ExportedRooms)
            {
                Dictionary<string, List<FamilyInstance>> dict = er.SortedItems;
                content += er.MepRoomNumber + ":" + Environment.NewLine;
                foreach (KeyValuePair<string, List<FamilyInstance>> pair in dict)
                {
                    content += pair.Key + Environment.NewLine;
                    foreach (FamilyInstance fi in pair.Value)
                    {
                        content += fi.Symbol.Family.Name + ", " + fi.Symbol.Name + ", " + fi.Id.IntegerValue.ToString() + Environment.NewLine;
                    }
                }
                content += Environment.NewLine;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
            string name = "Raumbuch_Extract_" + date;

            dlg.FileName = name; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                File.WriteAllText(filename, content);
            }
        }
    }
}
