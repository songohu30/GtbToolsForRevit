using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class RevitOpenedViews
    {
        public List<OpenedView> OpenedViews { get; set; }
        public bool SavePathExists { get; set; }
        public bool IsSaving { get; set; }
        public bool IsLoading { get; set; }

        public ExternalEvent OneEvent { get; set; }

        UIDocument _uidoc;
        UIApplication _uiapp;
        string _initialDirectory;

        public RevitOpenedViews()
        {

        }

        public void SetEvent(ExternalEvent externalEvent)
        {
            OneEvent = externalEvent;
        }

        public void LoadContext(UIApplication uiapp)
        {
            _uidoc = uiapp.ActiveUIDocument;
            _uiapp = uiapp;
            SetInitialDirectory();
        }

        private void SetInitialDirectory()
        {
            _initialDirectory = Path.Combine(@"H:\Revit\Makros\Gemeinsam genutzte Dateien", Environment.UserName);
            SavePathExists = Directory.Exists(_initialDirectory);
        }

        public void SaveOpenedViews()
        {
            if (!SavePathExists) return;
            OpenedViews = new List<OpenedView>();
            IList<UIView> uiviews = _uidoc.GetOpenUIViews();
            foreach (UIView uiview in uiviews)
            {
                View view = _uidoc.Document.GetElement(uiview.ViewId) as View;
                OpenedView openedView = new OpenedView();
                openedView.Name = view.Name;
                openedView.ID = view.Id.IntegerValue;
                OpenedViews.Add(openedView);
            }
            string output = JsonConvert.SerializeObject(OpenedViews);
            string documentName = _uidoc.Document.Title;
            string filename = Path.Combine(_initialDirectory, documentName + ".json");
            File.WriteAllText(filename, output);
            TaskDialog.Show("Info", "Geöffnete Ansichten wurden gespeichert." + Environment.NewLine + "Schönen Feierabend!");
        }

        public void LoadSavedViews()
        {
            if (!SavePathExists) return;
            string documentName = _uidoc.Document.Title;
            string filename = Path.Combine(_initialDirectory, documentName + ".json");
            if (!File.Exists(filename))
            {
                TaskDialog.Show("Info", "Gespeicherte Einstellungen können nicht gefunden werden!");
                return;
            }
            string content = File.ReadAllText(filename);
            OpenedViews = new List<OpenedView>();
            OpenedViews = JsonConvert.DeserializeObject<List<OpenedView>>(content);
            string notFound = "";
            int count = 0;
            foreach (OpenedView item in OpenedViews)
            {
                ElementId id = new ElementId(item.ID);
                View view = _uidoc.Document.GetElement(id) as View;
                if(view == null)
                {
                    notFound += String.Format("Ansicht mit ID Nummer {0} existiert nicht", id.IntegerValue) + Environment.NewLine;
                    count++;
                    continue;
                }
                _uidoc.ActiveView = view;                
            }
            if(OpenedViews.Count == count)
            {
                TaskDialog.Show("Info", notFound);
                return;
            }
#if DEBUG2018 || RELEASE2018
            RevitCommandId commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.TileWindows);

#else
            RevitCommandId commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.TileViews);
#endif
            _uiapp.PostCommand(commandId);
            foreach (UIView item in _uidoc.GetOpenUIViews())
            {
                View v = _uidoc.Document.GetElement(item.ViewId) as View;
                if (v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.CeilingPlan || v.ViewType == ViewType.EngineeringPlan || v.ViewType == ViewType.ThreeD) item.ZoomToFit();
                if (OpenedViews.Where(x => x.ID == v.Id.IntegerValue).ToList().Count == 0) item.Close();
            }
            if (notFound != "") TaskDialog.Show("Info", notFound);
        }
    }
}
