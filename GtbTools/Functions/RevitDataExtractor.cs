using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class RevitDataExtractor
    {
        Document _doc;

        public RevitDataExtractor(Document doc)
        {
            _doc = doc;
        }

        public void ExtractMepSystems()
        {
			FilteredElementCollector ficol = new FilteredElementCollector(_doc);
			ficol.OfClass(typeof(MEPSystemType));

			Dictionary<string, List<MEPSystemType>> mepSystems = new Dictionary<string, List<MEPSystemType>>();

			foreach (MEPSystemType mepSystem in ficol)
			{
				string categoryName = mepSystem.Category.Name;
				if (mepSystems.ContainsKey(categoryName))
				{
					mepSystems[categoryName].Add(mepSystem);
				}
				else
				{

					List<MEPSystemType> systemList = new List<MEPSystemType>();
					systemList.Add(mepSystem);
					mepSystems.Add(categoryName, systemList);
				}
			}
			string text = "";

			foreach (var pair in mepSystems)
			{
				text += "System Category:;" + pair.Key + Environment.NewLine;
				string header = "System type name;System classification;RGB";
				text += header + Environment.NewLine;
				foreach (MEPSystemType system in pair.Value)
				{
					string line = "";
					line += system.Name + ";";
					Parameter sysClass = system.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);


					line += sysClass.AsString() + ";";
					if (system.LineColor.IsValid)
					{
						line += system.LineColor.Red.ToString() + "-" + system.LineColor.Green.ToString() + "-" + system.LineColor.Blue.ToString();
					}
					else
					{
						line += "N/A";
					}
					text += line + Environment.NewLine;
				}
				text += Environment.NewLine;
			}

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
			string name = "Extract_" + date;

			dlg.FileName = name; // Default file name
			dlg.DefaultExt = ".csv"; // Default file extension
			dlg.Filter = "Text documents (.csv)|*.csv"; // Filter files by extension
			Nullable<bool> result = dlg.ShowDialog();
			System.Text.Encoding encoding = System.Text.Encoding.UTF8;

			if (result == true)
			{
				string filename = dlg.FileName;
				File.WriteAllText(filename, text, encoding);
			}
		}
    }
}
