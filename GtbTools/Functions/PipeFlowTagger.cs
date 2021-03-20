using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using GUI;
using PipeFlowTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Functions
{
    public class PipeFlowTagger : INotifyPropertyChanged
    {
        #region Properties and fields
        public UIDocument UIDocument { get; set; }
        public Document Document { get; set; }
        public List<Pipe> ReferencePipes { get; set; }
        public List<VerticalPipingLine> PipingLines { get; set; }
        public ElementId SelectedItem { get; set; }
        public Level GeneratingLevel { get; set; }
        public PipeFlowToolAction Action { get; set; }
        public ExternalEvent StartEvent { get; set; }
        public List<XYZ> UsedCoordinates { get; set; }
        public FamilySymbol ManualTag { get; set; }

        private List<LineViewModel> _lineViewModels;
        public List<LineViewModel> LineViewModels
        {
            get => _lineViewModels;
            set
            {
                if (_lineViewModels != value)
                {
                    _lineViewModels = value;
                    OnPropertyChanged(nameof(LineViewModels));
                }
            }
        }

        public List<FamilySymbol> PipeFittingTags { get; set; }
        public List<FamilySymbol> SelectedTags { get; set; }
        public DefaultDirections DefaultDirections { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Dictionary<string, List<XYZ>> _systemUsedCoordinates;
        List<SameCoordinatePipes> _sameCoordinatePipesList;
        List<IndependentTag> _allViewTags;
        List<IndependentTag> _pipeFlowTags;
        List<ElementId> _taggedElementIds;
        double _topLevel;
        double _bottomLevel;
        View _activeView;
        ViewType _viewType;
        bool _isTopUndefined = false;
        bool _isBottomUndefined = false;


        public ManualResetEvent SignalEvent = new ManualResetEvent(false);
        #endregion

        public PipeFlowTagger()
        {
            DefaultDirections = new DefaultDirections();
        }

        public void Initialize(UIDocument uIDocument)
        {
            UIDocument = uIDocument;
            Document = uIDocument.Document;
            SetActiveView();
            SetViewLevels();
            SetAnnotationSymbols();
            SignalEvent.Set();
        }

        public void AnnotateSelection(List<ElementId> selectedIds)
        {
            List<Pipe> pipes = new List<Pipe>();
            foreach (ElementId id in selectedIds)
            {
                Element e = Document.GetElement(id);
                if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves) pipes.Add(e as Pipe);
            }

            List<ManualVerticalLine> lines = new List<ManualVerticalLine>();
            foreach (Pipe p in pipes)
            {
                Line l = (p.Location as LocationCurve).Curve as Line;
                if (Math.Abs(l.Direction.Z) > 0.999)
                {
                    ManualVerticalLine vl = new ManualVerticalLine(p);
                    vl.GetTagHolder();
                    lines.Add(vl);
                }
            }
            using(Transaction tx = new Transaction(Document, "Manual beschriftung"))
            {
                tx.Start();
                foreach (ManualVerticalLine line in lines)
                {
                    line.AnnotateLine(Document, ManualTag.Id);
                }
                tx.Commit();
            }
        }

        public void ExchangeTags(List<ElementId> selectedIds)
        {
            if(selectedIds == null || selectedIds.Count == 0)
            {
                TaskDialog.Show("Info", "Bitte eine Tags im Projekt wählen!");
                return;
            }
            List<FamilySymbol> symbolsMitDN = PipeFittingTags.Where(e => e.Family.Name == "XXX Strang mit DN").ToList();
            List<FamilySymbol> symbolsOhneDN = PipeFittingTags.Where(e => e.Family.Name == "XXX Strang ohne DN").ToList();

            using(Transaction tx = new Transaction(Document, "Exchange tag types"))
            {
                tx.Start();
                foreach (ElementId id in selectedIds)
                {
                    Element el = Document.GetElement(id);
                    if (el == null || el.Category == null) continue;
                    if (el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFittingTags)
                    {
                        IndependentTag tag = el as IndependentTag;
                        ElementId symbolId = tag.GetTypeId();
                        FamilySymbol tagSymbol = Document.GetElement(symbolId) as FamilySymbol;
                        string familyName = tagSymbol.Family.Name;
                        string symbolName = tagSymbol.Name;
                        if (familyName == "XXX Strang mit DN")
                        {
                            FamilySymbol newSymbol = symbolsOhneDN.Where(e => e.Name == symbolName).FirstOrDefault();
                            if (newSymbol != null)
                            {
                                tag.ChangeTypeId(newSymbol.Id);
                            }
                        }
                        if (familyName == "XXX Strang ohne DN")
                        {
                            FamilySymbol newSymbol = symbolsMitDN.Where(e => e.Name == symbolName).FirstOrDefault();
                            if (newSymbol != null)
                            {
                                tag.ChangeTypeId(newSymbol.Id);
                            }
                        }
                    }
                }
                tx.Commit();
            }
        }

        public void SetEvent(ExternalEvent startEvent)
        {
            StartEvent = startEvent;
        }

        public void AnalyzeView()
        {
            UsedCoordinates = new List<XYZ>();
            _systemUsedCoordinates = new Dictionary<string, List<XYZ>>();
            _sameCoordinatePipesList = new List<SameCoordinatePipes>();
            SetActiveView(); //added from initialize
            SetViewLevels(); //added from initialize
            SetAnnotationSymbols(); //added from initialize
            if(CheckFlags()) return;
            FindReferencePipes();
            CreatePipingLines();
            SetAllViewTags();
        }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                SignalEvent.WaitOne();
                SignalEvent.Reset();
                PipeFlowTagWindow window = new PipeFlowTagWindow(this);
                window.Show();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void TagAllLines()
        {
            int count = 0;
            using (Transaction tx = new Transaction(Document, "Adding PIF Tags"))
            {
                tx.Start();
                foreach (VerticalPipingLine line in PipingLines)
                {
                    if (line.TagTheLine(SelectedTags, Document, DefaultDirections)) count++;
                }
                tx.Commit();
            }
            TaskDialog.Show("Info", String.Format("Added {0} new tags!", count));
        }

        public void SelectElement()
        {
            UIDocument.Selection.SetElementIds(new List<ElementId>() { SelectedItem });
        }

        public void SetTaggedElementIds()
        {
            SetPipeFlowTags();
            _taggedElementIds = new List<ElementId>();
            foreach (IndependentTag item in _pipeFlowTags)
            {
                ElementId id = item.TaggedLocalElementId;
                if(id.IntegerValue > 0) _taggedElementIds.Add(id);
            }
        }

        private void SetAnnotationSymbols()
        {
            PipeFittingTags = new List<FamilySymbol>();
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            PipeFittingTags = ficol.OfClass(typeof(FamilySymbol)).Select(x => x as FamilySymbol)
                                     .Where(y => y.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFittingTags).ToList();
        }

        public void SetViewModelList()
        {
            LineViewModels = new List<LineViewModel>();
            foreach (VerticalPipingLine line in PipingLines)
            {
                LineViewModel model = LineViewModel.Initialize(line);
                LineViewModels.Add(model);
            }
        }

        private void CreatePipingLines()
        {
            PipingLines = new List<VerticalPipingLine>();
            foreach (Pipe pipe in ReferencePipes)
            {
                string systemTypeName = SetSystemTypeName(pipe);
                VerticalPipingLine line = VerticalPipingLine.Initialize(pipe, systemTypeName, _topLevel, _bottomLevel, _activeView);
                if (line.GoesAbove || line.GoesBelow) PipingLines.Add(line);
            }
        }

        private bool CheckFlags()
        {
            bool result = false;
            if (_viewType != ViewType.FloorPlan && _viewType != ViewType.CeilingPlan)
            {
                TaskDialog.Show("Warning", "View must be of type: Grundriss oder Decken Plan");
                result = true;
            }
            if (_isTopUndefined) TaskDialog.Show("Warning", "Top level is undefined");
            if (_isBottomUndefined) TaskDialog.Show("Warning", "Bottom level is undefined");
            return result;
        }

        private  void FindReferencePipes()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document, _activeView.Id);
            ReferencePipes = ficol.OfClass(typeof(Pipe)).Select(x => x as Pipe).Where(p => IsPipeVertical(p) && !IsLocationUsedBySystem(p) && !ConnectorsOutOfRange(p)).ToList();
        }

        private bool ConnectorsOutOfRange(Pipe pipe)
        {
            bool result = false;
            ConnectorManager cm = pipe.ConnectorManager;
            ConnectorSet cs = cm.Connectors;
            List<Connector> cList = new List<Connector>();
            double zConnector1;
            double zConnector2;

            foreach (Connector c in cs)
            {
                cList.Add(c);
            }
            zConnector1 = cList[0].Origin.Z;
            zConnector2 = cList[1].Origin.Z;

            if (zConnector1 > _topLevel && zConnector2 > _topLevel) result = true;
            if (zConnector1 < _bottomLevel && zConnector2 < _bottomLevel) result = true;
            return result;
        }

        private void SetActiveView()
        {
            _activeView = Document.ActiveView;
            _viewType = Document.ActiveView.ViewType;
        }

        private void SetViewLevels()
        {
            GeneratingLevel = Document.ActiveView.GenLevel;
            if (_viewType == ViewType.FloorPlan || _viewType == ViewType.CeilingPlan)
            {
                ViewPlan viewPlan = _activeView as ViewPlan;
                PlanViewRange range = viewPlan.GetViewRange();
                ElementId bottomLvlId = range.GetLevelId(PlanViewPlane.BottomClipPlane);
                ElementId topLvlId = range.GetLevelId(PlanViewPlane.TopClipPlane);

                if (bottomLvlId.IntegerValue > 0)
                {
                    //double offset = range.GetOffset(PlanViewPlane.BottomClipPlane);
                    Element bottomLevelAsElement = Document.GetElement(bottomLvlId);
                    Level bottomLevel = bottomLevelAsElement as Level;
                    _bottomLevel = bottomLevel.Elevation;
                }
                else
                {
                    _isBottomUndefined = true;
                }


                if (topLvlId.IntegerValue > 0)
                {
                    //double offset = range.GetOffset(PlanViewPlane.TopClipPlane);
                    Element topLevelAsElement = Document.GetElement(topLvlId);
                    Level topLevel = topLevelAsElement as Level;
                    _topLevel = topLevel.Elevation;
                }
                else
                {
                    _isTopUndefined = true;
                }
            }
        }

        private bool IsPipeVertical(Pipe pipe)
        {
            bool result = false;
            LocationCurve lc = pipe.Location as LocationCurve;
            Line l = lc.Curve as Line;
            XYZ direction = l.Direction;
            double test = Math.Sin(Math.PI / 180 * 89);
            if (Math.Abs(direction.Z) >= test) result = true;
            return result;
        }

        /// <summary>
        /// Checks if the coordinates are used by pipe of the same systemtype name
        /// </summary>
        private bool IsLocationUsedBySystem(Pipe pipe)
        {
            bool result = false;
            LocationCurve lc = pipe.Location as LocationCurve;
            Line l = lc.Curve as Line;
            XYZ origin = l.Origin;
            double tolerance = 0.5 * pipe.Diameter;
            string systemTypeName = SetSystemTypeName(pipe);
            var testList = _systemUsedCoordinates.Where(e => e.Key == systemTypeName).Select(e => e.Value).ToList();
            if(testList.Count == 1)
            {
                List<XYZ> systemCoordinates = _systemUsedCoordinates[systemTypeName];
                List<XYZ> test = systemCoordinates.Where(e => Math.Abs(e.X - origin.X) < tolerance && Math.Abs(e.Y - origin.Y) < tolerance).ToList();
                if (test.Count > 0)
                {
                    if (HasDirectLineConnection(pipe)) result = true;
                }
                else
                {
                    systemCoordinates.Add(origin);
                }
            }
            else
            {
                List<XYZ> newList = new List<XYZ>() { origin };
                _systemUsedCoordinates.Add(systemTypeName, newList);
            }
            SameCoordinatePipes sameCoordinatePipes = _sameCoordinatePipesList.Where(e => e.IsPipeWithinTolerance(pipe)).FirstOrDefault();
            if (sameCoordinatePipes == null)
            {
                SameCoordinatePipes newInstance = new SameCoordinatePipes(origin);
                newInstance.AddToList(pipe);
                _sameCoordinatePipesList.Add(newInstance);
            }
            else
            {
                sameCoordinatePipes.AddToList(pipe);
            }
            return result;
        }

        /// <summary>
        /// Checks if the pipe has direct connection with exisitng pipes in the same coordinates
        /// </summary>
        private bool HasDirectLineConnection(Pipe pipe)
        {
            bool result = false;

            SameCoordinatePipes sameCoordinatePipes = _sameCoordinatePipesList.Where(e => e.IsPipeWithinTolerance(pipe)).FirstOrDefault();
            if (sameCoordinatePipes != null)
            {
                LocationCurve lc = pipe.Location as LocationCurve;
                Line l = lc.Curve as Line;
                //XYZ origin = l.Origin;
                double testedPipeZ = GetLineMiddleZ(l);
                foreach (Pipe listedPipe in sameCoordinatePipes.Pipes)
                {
                    LocationCurve lcListPipe = listedPipe.Location as LocationCurve;
                    Line lListedPipe = lcListPipe.Curve as Line;
                    //XYZ originListedPIpe = lListedPipe.Origin;
                    double listedPipeZ = GetLineMiddleZ(lListedPipe);
                    Element element = pipe as Element;
                    Pipe searchedPipe = listedPipe;
                    //tested is above listed
                    if(testedPipeZ > listedPipeZ)
                    {
                        do
                        {
                            Element returnedElement = LoopDown(element, searchedPipe, out result);
                            element = returnedElement;
                        }
                        while (element != null);
                    }

                    //tested is below listed
                    if(testedPipeZ < listedPipeZ)
                    {
                        do
                        {
                            Element returnedElement = LoopUp(element, searchedPipe, out result);
                            element = returnedElement;
                        }
                        while (element != null);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if searched pipe lies in direct line that goes up through pipe and fittings connectors
        /// </summary>
        private Element LoopUp(Element element, Pipe searchedPipe, out bool searchResult)
        {
            Element result = null;
            searchResult = false;

            //if is pipe
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
            {
                Pipe pipe = element as Pipe;
                ConnectorManager cm = pipe.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                Connector topConnector = null;
                foreach (Connector c in cs)
                {
                    if (topConnector == null) topConnector = c;
                    if (topConnector.Origin.Z < c.Origin.Z) topConnector = c;
                }
                ConnectorSet connectorLinks = topConnector.AllRefs;
                foreach (Connector link in connectorLinks)
                {
                    Element linkedElement = link.Owner;
                    if (linkedElement.Id.IntegerValue == element.Id.IntegerValue) continue;
                    if(linkedElement.Id.IntegerValue == searchedPipe.Id.IntegerValue)
                    {
                        searchResult = true;
                        return null;
                    }
                    if(linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        result = linkedElement;
                    }
                }
            }
            //if is pipe fitting
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                MechanicalFitting pipeFitting = familyInstance.MEPModel as MechanicalFitting;
                ConnectorManager cm = pipeFitting.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                Connector topConnector = null;
                int count = 0;
                foreach (Connector c in cs)
                {
                    if (!IsConnectorVertical(c)) continue;
                    if (topConnector == null) topConnector = c;
                    if (topConnector.Origin.Z < c.Origin.Z) topConnector = c;
                    count++;
                }
                if (topConnector == null) return null;
                if (count == 1) return null;
                ConnectorSet connectorLinks = topConnector.AllRefs;
                foreach (Connector link in connectorLinks)
                {
                    Element linkedElement = link.Owner;
                    if (linkedElement.Id.IntegerValue == element.Id.IntegerValue) continue;
                    if (linkedElement.Id.IntegerValue == searchedPipe.Id.IntegerValue)
                    {
                        searchResult = true;
                        return null;
                    }
                    if (linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        result = linkedElement;
                    }
                }
            }
            //if is pipe accessorry
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                MEPModel pipeAccessory = familyInstance.MEPModel;
                ConnectorManager cm = pipeAccessory.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                Connector topConnector = null;
                int count = 0;
                foreach (Connector c in cs)
                {
                    if (!IsConnectorVertical(c)) continue;
                    if (topConnector == null) topConnector = c;
                    if (topConnector.Origin.Z < c.Origin.Z) topConnector = c;
                    count++;
                }
                if (topConnector == null) return null;
                if (count == 1) return null;
                ConnectorSet connectorLinks = topConnector.AllRefs;
                foreach (Connector link in connectorLinks)
                {
                    Element linkedElement = link.Owner;
                    if (linkedElement.Id.IntegerValue == element.Id.IntegerValue) continue;
                    if (linkedElement.Id.IntegerValue == searchedPipe.Id.IntegerValue)
                    {
                        searchResult = true;
                        return null;
                    }
                    if (linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        result = linkedElement;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if searched pipe lies in direct line that goes down through pipe and fittings connectors
        /// </summary>
        private Element LoopDown(Element element, Pipe searchedPipe, out bool searchResult)
        {
            Element result = null;
            searchResult = false;
            //if is pipe
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
            {
                Pipe pipe = element as Pipe;
                ConnectorManager cm = pipe.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                Connector bottomConnector = null;
                foreach (Connector c in cs)
                {
                    if (bottomConnector == null) bottomConnector = c;
                    if (bottomConnector.Origin.Z > c.Origin.Z) bottomConnector = c;
                }
                ConnectorSet connectorLinks = bottomConnector.AllRefs;
                foreach (Connector link in connectorLinks)
                {
                    Element linkedElement = link.Owner;
                    if (linkedElement.Id.IntegerValue == element.Id.IntegerValue) continue;
                    if (linkedElement.Id.IntegerValue == searchedPipe.Id.IntegerValue)
                    {
                        searchResult = true;
                        return null;
                    }
                    if (linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        result = linkedElement;
                    }
                }
            }
            //if is pipe fitting
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                MechanicalFitting pipeFitting = familyInstance.MEPModel as MechanicalFitting;
                ConnectorManager cm = pipeFitting.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                Connector bottomConnector = null;
                int count = 0;
                foreach (Connector c in cs)
                {
                    if (!IsConnectorVertical(c)) continue;
                    if (bottomConnector == null) bottomConnector = c;
                    if (bottomConnector.Origin.Z > c.Origin.Z) bottomConnector = c;
                    count++;
                }
                if (bottomConnector == null) return null;
                if (count == 1) return null;
                ConnectorSet connectorLinks = bottomConnector.AllRefs;
                foreach (Connector link in connectorLinks)
                {
                    Element linkedElement = link.Owner;
                    if (linkedElement.Id.IntegerValue == element.Id.IntegerValue) continue;
                    if (linkedElement.Id.IntegerValue == searchedPipe.Id.IntegerValue)
                    {
                        searchResult = true;
                        return null;
                    }
                    if (linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        result = linkedElement;
                    }
                }
            }
            //if is pipe accessorry
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
            {
                FamilyInstance familyInstance = element as FamilyInstance;
                MEPModel pipeAccessory = familyInstance.MEPModel;
                ConnectorManager cm = pipeAccessory.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                Connector topConnector = null;
                int count = 0;
                foreach (Connector c in cs)
                {
                    if (!IsConnectorVertical(c)) continue;
                    if (topConnector == null) topConnector = c;
                    if (topConnector.Origin.Z > c.Origin.Z) topConnector = c;
                    count++;
                }
                if (topConnector == null) return null;
                if (count == 1) return null;
                ConnectorSet connectorLinks = topConnector.AllRefs;
                foreach (Connector link in connectorLinks)
                {
                    Element linkedElement = link.Owner;
                    if (linkedElement.Id.IntegerValue == element.Id.IntegerValue) continue;
                    if (linkedElement.Id.IntegerValue == searchedPipe.Id.IntegerValue)
                    {
                        searchResult = true;
                        return null;
                    }
                    if (linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory || linkedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        result = linkedElement;
                    }
                }
            }
            return result;
        }

        private double GetLineMiddleZ(Line line)
        {
            IList<XYZ> ends = line.Tessellate();
            XYZ l1 = ends[0];
            XYZ l2 = ends[1];
            double middleZ = (l1.Z + l2.Z) * 0.5;
            return middleZ;
        }

        private bool IsConnectorVertical(Connector connector)
        {
            bool result = false;
            if (Math.Abs(connector.CoordinateSystem.BasisZ.Z) > Math.Sin(Math.PI / 180 * 87)) result = true;
            return result;
        }

        private string SetSystemTypeName(Pipe pipe)
        {
            ElementId pipingSystemId = pipe.MEPSystem.GetTypeId();
            if(pipingSystemId.IntegerValue > 0)
            {
                PipingSystemType pipingSystemType = Document.GetElement(pipingSystemId) as PipingSystemType;
                return pipingSystemType.Name;
            }
            else
            {
                return "UnknownSystem";
            }
        }

        private void SetAllViewTags()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document, _activeView.Id);
            _allViewTags = ficol.OfClass(typeof(IndependentTag)).Select(x => x as IndependentTag).ToList();
        }

        private void SetPipeFlowTags()
        {
            _pipeFlowTags = _allViewTags.Where(e => IsOfSelectedFamilySymbol(e)).ToList();
        }

        private bool IsOfSelectedFamilySymbol(IndependentTag tag)
        {
            bool result = false;

            List<FamilySymbol> included = PipeFittingTags.Where(e => e.Family.Name == "XXX Strang mit DN" || e.Family.Name == "XXX Strang ohne DN").ToList();
            

            foreach (FamilySymbol fs in included)
            {
                ElementId fsId = fs.Id;
                if(tag.GetTypeId().IntegerValue == fsId.IntegerValue)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void CheckExistingTags()
        {
            foreach (VerticalPipingLine vpl in PipingLines)
            {
                List<Element> elements = vpl.LineElements.Where(e => _taggedElementIds.Contains(e.Id)).ToList();
                if (elements.Count > 0) vpl.IsTagged = true;
            }
        }
    }
}
