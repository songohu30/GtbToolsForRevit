using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using CuttingElementTool;
using GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Functions
{
    public class CuttingElementSearch : INotifyPropertyChanged
    {
        public List<CuttingElement> CuttingElements { get; set; }
        public CutElementToolAction ToolAction { get; set; }

        private List<CutSearchOpeningModel> _openingModels;
        public List<CutSearchOpeningModel> OpeningModels
        {
            get { return _openingModels; }
            set
            {
                if (_openingModels != value)
                {
                    _openingModels = value;
                    OnPropertyChanged(nameof(OpeningModels));
                }
            }
        }

        private List<CutSearchOpeningModel> _notFoundModels;
        public List<CutSearchOpeningModel> NotFoundModels
        {
            get => _notFoundModels;
            set
            {
                if (_notFoundModels != value)
                {
                    _notFoundModels = value;
                    OnPropertyChanged(nameof(NotFoundModels));
                }
            }
        }

        private List<CutSearchOpeningModel> _otherModels;
        public List<CutSearchOpeningModel> OtherModels
        {
            get => _otherModels;
            set
            {
                if (_otherModels != value)
                {
                    _otherModels = value;
                    OnPropertyChanged(nameof(OtherModels));
                }
            }
        }

        public Document Document { get; set; }
        public List<RevitLinkInstance> Links { get; set; }
        public Document LinkedDoc;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExternalEvent TheEvent { get; set; }
        public ManualResetEvent SignalEvent = new ManualResetEvent(false);
        public List<CutSearchOpeningModel> Selection { get; set; }
        public RevitLinkInstance SelectedLink { get; set; }

        public CuttingElementSearch()
        {

        }

        public void InitializeDocument(Document doc)
        {
            Document = doc;
        }

        public void SetEvent(ExternalEvent externalEvent)
        {
            TheEvent = externalEvent;
        }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                DiameterFixWindow window = new DiameterFixWindow(this);
                window.Show();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void FixSelection()
        {
            using (Transaction tx = new Transaction(Document, "Setting diameters"))
            {
                tx.Start();
                foreach (CutSearchOpeningModel item in Selection)
                {
                    item.AlignDiameters();
                }
                tx.Commit();
            }
        }

        public void SetLinks()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document).OfClass(typeof(RevitLinkInstance)).WhereElementIsNotElementType();
            Links = ficol.Select(e => e as RevitLinkInstance).ToList();
        }

        public void SetLinkedDoc()
        {
            LinkedDoc = SelectedLink.GetLinkDocument();
        }

        public void SetCuttingElements()
        {
            CuttingElements = new List<CuttingElement>();
            FilteredElementCollector ficol1 = new FilteredElementCollector(Document);
            FilteredElementCollector ficol2 = new FilteredElementCollector(Document);

            List<Element> pipes = ficol1.OfClass(typeof(Pipe)).ToList();
            List<Element> ducts = ficol2.OfClass(typeof(Duct)).ToList();
            foreach (Element el in pipes)
            {
                CuttingElement ce = CuttingElement.Initialize(el, CuttingElementType.Pipes);
                CuttingElements.Add(ce);
            }
            foreach (Element el in ducts)
            {
                CuttingElement ce = CuttingElement.Initialize(el, CuttingElementType.Ducts);
                CuttingElements.Add(ce);
            }
        }
        public void SetLinkedCuttingElements()
        {
            CuttingElements = new List<CuttingElement>();
            FilteredElementCollector ficol1 = new FilteredElementCollector(LinkedDoc);
            FilteredElementCollector ficol2 = new FilteredElementCollector(LinkedDoc);

            List<Element> pipes = ficol1.OfClass(typeof(Pipe)).ToList();
            List<Element> ducts = ficol2.OfClass(typeof(Duct)).ToList();
            foreach (Element el in pipes)
            {
                CuttingElement ce = CuttingElement.Initialize(el, CuttingElementType.Pipes);
                CuttingElements.Add(ce);
            }
            foreach (Element el in ducts)
            {
                CuttingElement ce = CuttingElement.Initialize(el, CuttingElementType.Ducts);
                CuttingElements.Add(ce);
            }
        }

        public void SetOpeningModels()
        {
            OpeningModels = new List<CutSearchOpeningModel>();
            NotFoundModels = new List<CutSearchOpeningModel>();
            OtherModels = new List<CutSearchOpeningModel>();
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                                    .Select(x => x as FamilyInstance)
                                        .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            List<FamilyInstance> openings = genModelInstances.Where(e => e.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6")) != null).ToList();
            List<FamilyInstance> wallOpenings = openings.Where(e => e.GetTransform().BasisZ.Z == 0).ToList();

            int notFound = 0;

            foreach (FamilyInstance fi in wallOpenings)
            {
                Parameter gtbParameter = fi.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6"));
                if (gtbParameter.AsString() != "GTB_Tools_location_marker")
                {
                    CutSearchOpeningModel model = new CutSearchOpeningModel(fi);
                    Parameter diameter = fi.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a"));
                    if (diameter != null)
                    {
                        model.SetPoints();
                        //model.SetCenterLine();
                        model.SetParameters();
                        bool result = model.SearchCuttingElement(CuttingElements);
                        if (result)
                        {
                            if (model.IsDequalDN())
                            {
                                OtherModels.Add(model);
                            }
                            else
                            {
                                OpeningModels.Add(model);
                            }
                        }
                        else
                        {
                            NotFoundModels.Add(model);
                            notFound++;
                        }
                    }
                }
            }
            if (notFound > 0) MessageBox.Show(String.Format("Can't find cutting element for {0} openings", notFound));
        }
    }
}
