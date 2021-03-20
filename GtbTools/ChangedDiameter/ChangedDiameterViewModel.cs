using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangedDiameter
{
    public class ChangedDiameterViewModel 
    {
        public List<PipeViewModel> PipeModels { get; set; }

        List<Pipe> _pipes;

        public ChangedDiameterViewModel(List<Pipe> pipes)
        {

        }

        public void CreatePipeModels()
        {
            PipeModels = new List<PipeViewModel>();
            foreach (Pipe pipe in _pipes)
            {
                double diameter = UnitUtils.ConvertFromInternalUnits(pipe.Diameter, DisplayUnitType.DUT_MILLIMETERS);
                PipeViewModel model = new PipeViewModel();
                model.CurrentDiameter = diameter;
                model.ElementId = pipe.Id;
            }
        }

        public void WriteToFile()
        {
            string jsonString = JsonConvert.SerializeObject(PipeModels);
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
            string name = "RohrDimensionExtract" + date;
            dlg.FileName = name; // Default file name
            dlg.DefaultExt = ".json"; // Default file extension
            string initialDirectory = Path.Combine(@"H:\Revit\Makros\Gemeinsam genutzte Dateien", Environment.UserName);
            if (Directory.Exists(initialDirectory)) dlg.InitialDirectory = initialDirectory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                File.WriteAllText(filename, jsonString);
            }
        }

        public void ReadFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            string initialDirectory = Path.Combine(@"H:\Revit\Makros\Gemeinsam genutzte Dateien", Environment.UserName);
            if (Directory.Exists(initialDirectory)) dlg.InitialDirectory = initialDirectory;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string savePath = dlg.FileName;
                string jsonString = File.ReadAllText(savePath);
                List<PipeViewModel> oldModels = JsonConvert.DeserializeObject<List<PipeViewModel>>(jsonString);
                foreach (PipeViewModel oldModel in oldModels)
                {
                    PipeViewModel currentModel = PipeModels.Where(e => e.ElementId == oldModel.ElementId).FirstOrDefault();
                    if (currentModel != null) currentModel.OldDiameter = oldModel.CurrentDiameter;
                }
            }
        }
    }
}
