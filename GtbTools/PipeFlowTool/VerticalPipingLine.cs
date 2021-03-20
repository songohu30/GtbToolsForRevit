using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PipeFlowTool
{
    public class VerticalPipingLine
    {
        public string SystemTypeName { get; set; }
        public Pipe ReferencePipe { get; set; } //each vertical pipe on the plan that resides on different xyz coordinates
        public List<Element> AboveLineElements { get; set; } //elements located above reference pipe with connectors going up (tee, reducer, socket)
        public List<Element> BelowLineElements { get; set; } //elements located below reference pipe with connectors going down (tee, reducer, socket)
        public List<Element> ReducingElements { get; set; } //reducing element in the vertical line if any
        public List<Element> ViaElements { get; set; } //elements with vertical flow
        public List<Element> EndElements { get; set; } //elements without vertical flow, mostly elbows, plugs
        public bool GoesAbove { get; set; } = false; //piping line goes above view range top level
        public bool GoesBelow { get; set; } = false; // piping line goes below view range bottom level
        public List<Element> LineElements { get; set; } //elements that were added to the line
        public Element TagHolder { get; set; }
        public FlowToolFlag FlowToolFlag { get; set; }
        public TagOption TagOption { get; set; }
        public bool IsTagged { get; set; } = false;

        double _topLevel;
        double _bottomLevel;
        XYZ _originCoords;
        Autodesk.Revit.DB.View _activeView;

        private VerticalPipingLine()
        {

        }

        public static VerticalPipingLine Initialize(Pipe verticalPipe, string systemTypeName, double topLevel, double bottomLevel, Autodesk.Revit.DB.View activeView)
        {
            VerticalPipingLine result = new VerticalPipingLine();
            result._topLevel = topLevel;
            result._bottomLevel = bottomLevel;
            result.ReferencePipe = verticalPipe;
            result.SystemTypeName = systemTypeName;
            result._activeView = activeView;
            result.SetOriginCoords();
            result.AboveLineElements = new List<Element>();
            result.BelowLineElements = new List<Element>();
            result.LineElements = new List<Element>();
            result.LoopAboveElements();
            result.LoopBelowElements();
            result.CategorizeElements();
            result.FindTagHolder();
            return result;
        }

        public bool TagTheLine(List<FamilySymbol> selectedTags, Document doc, DefaultDirections defaultDirections)
        {
            SetTagOption(defaultDirections);
            if (IsTagged || TagOption == TagOption.None || TagHolder == null) return false;
            FamilySymbol theTag = selectedTags[(int)TagOption];
            ElementId theTagId = theTag.Id;
            Reference reference = new Reference(TagHolder);
#if DEBUG2018 || RELEASE2018
            IndependentTag newTag = IndependentTag.Create(doc, doc.ActiveView.Id, reference, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, _originCoords);
            newTag.ChangeTypeId(theTagId);
#else
            IndependentTag.Create(doc, theTagId, doc.ActiveView.Id, reference, false, TagOrientation.Horizontal, _originCoords);
#endif
            return true;
        }

        private void SetTagOption(DefaultDirections defaultDirections)
        {
            TagOption = TagOption.None;
            if(SystemTypeName.Contains("SAN_Schmutzwasser"))
            {
                if(GoesAbove && GoesBelow)
                {
                    if(defaultDirections.SAN_Schmutzwasser == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.SAN_Schmutzwasser == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.SAN_Schmutzwasser == FlowDirection.Down)  TagOption = TagOption.VonOben;
                        if (defaultDirections.SAN_Schmutzwasser == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.SAN_Schmutzwasser == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.SAN_Schmutzwasser == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("SAN_Ventilation"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.SAN_Ventilation == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.SAN_Ventilation == FlowDirection.Up) TagOption = TagOption.VentViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.SAN_Ventilation == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.SAN_Ventilation == FlowDirection.Up) TagOption = TagOption.VentNachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.SAN_Ventilation == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.SAN_Ventilation == FlowDirection.Up) TagOption = TagOption.VentVonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("SAN_Regenwasser"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.SAN_Regenwasser == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.SAN_Regenwasser == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.SAN_Regenwasser == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.SAN_Regenwasser == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.SAN_Regenwasser == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.SAN_Regenwasser == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("SAN_Kaltwasser"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.SAN_Kaltwasser == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.SAN_Kaltwasser == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.SAN_Kaltwasser == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.SAN_Kaltwasser == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.SAN_Kaltwasser == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.SAN_Kaltwasser == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("SAN_Warmwasser"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.SAN_Warmwasser == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.SAN_Warmwasser == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.SAN_Warmwasser == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.SAN_Warmwasser == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.SAN_Warmwasser == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.SAN_Warmwasser == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("SAN_Zirkulation"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.SAN_Zirkulation == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.SAN_Zirkulation == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.SAN_Zirkulation == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.SAN_Zirkulation == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.SAN_Zirkulation == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.SAN_Zirkulation == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("HZG_Vorlauf"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.HZG_Vorlauf == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.HZG_Vorlauf == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.HZG_Vorlauf == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.HZG_Vorlauf == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.HZG_Vorlauf == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.HZG_Vorlauf == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("HZG_Rücklauf"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.HZG_Rucklauf == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.HZG_Rucklauf == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.HZG_Rucklauf == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.HZG_Rucklauf == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.HZG_Rucklauf == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.HZG_Rucklauf == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("KAE_Vorlauf"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.KAE_Vorlauf == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.KAE_Vorlauf == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.KAE_Vorlauf == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.KAE_Vorlauf == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.KAE_Vorlauf == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.KAE_Vorlauf == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
            if (SystemTypeName.Contains("KAE_Rücklauf"))
            {
                if (GoesAbove && GoesBelow)
                {
                    if (defaultDirections.KAE_Rucklauf == FlowDirection.Down) TagOption = TagOption.ViaDown;
                    if (defaultDirections.KAE_Rucklauf == FlowDirection.Up) TagOption = TagOption.ViaUp;
                }
                else
                {
                    if (GoesAbove)
                    {
                        if (defaultDirections.KAE_Rucklauf == FlowDirection.Down) TagOption = TagOption.VonOben;
                        if (defaultDirections.KAE_Rucklauf == FlowDirection.Up) TagOption = TagOption.NachOben;
                    }
                    if (GoesBelow)
                    {
                        if (defaultDirections.KAE_Rucklauf == FlowDirection.Down) TagOption = TagOption.NachUnten;
                        if (defaultDirections.KAE_Rucklauf == FlowDirection.Up) TagOption = TagOption.VonUnten;
                    }
                }
            }
        }

        private void FindTagHolder()
        {
            if (ReducingElements.Count == 1)
            {
                TagHolder = ReducingElements.First();
                return;
            }
            if(ReducingElements.Count > 1)
            {
                FlowToolFlag = FlowToolFlag.MultiRedElemente;
                return;
            }
            if(ViaElements.Count > 0)
            {
                TagHolder = ViaElements.FirstOrDefault();
                return;
            }
            if (EndElements.Count > 0)
            {
                TagHolder = EndElements.FirstOrDefault();
                return;
            }
            FlowToolFlag = FlowToolFlag.KeineElemente;
        }

        private void CategorizeElements()
        {
            ViaElements = new List<Element>();
            ReducingElements = new List<Element>();
            EndElements = new List<Element>();
            foreach (Element e in LineElements)
            {
                if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves) continue;
                if (e.IsHidden(_activeView)) continue;
                if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                {
                    FamilyInstance fi = e as FamilyInstance;
                    MechanicalFitting mf = fi.MEPModel as MechanicalFitting;
                    ConnectorManager cm = mf.ConnectorManager;
                    ConnectorSet cs = cm.Connectors;
                    List<Connector> inLineConnectors = new List<Connector>();
                    foreach (Connector c in cs)
                    {
                        if (IsConnectorVertical(c) && IsInLine(c)) inLineConnectors.Add(c);
                    }
                    if (inLineConnectors.Count == 1) EndElements.Add(e);
                    if (inLineConnectors.Count == 2)
                    {
                        if(IsViaReduced(inLineConnectors))
                        {
                            ReducingElements.Add(e);
                        }
                        else
                        {
                            ViaElements.Add(e);
                        }
                    }
                }
                if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
                {
                    FamilyInstance fi = e as FamilyInstance;
                    MEPModel mepModel = fi.MEPModel;
                    ConnectorManager cm = mepModel.ConnectorManager;
                    ConnectorSet cs = cm.Connectors;
                }

            }
        }

        private void LoopAboveElements()
        {
            Element element = ReferencePipe as Element;
            Parameter systemTypeParameter = ReferencePipe.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            string systemTypeName = systemTypeParameter.AsValueString();
            Connector lastConnector = null;
            do
            {
                Connector connector = FindAboveConnector(element, lastConnector);
                element = FindConnectorOwner(connector, element, systemTypeName);
                lastConnector = connector;
            }
            while (element != null);
        }

        private void LoopBelowElements()
        {
            Element element = ReferencePipe as Element;
            Parameter systemTypeParameter = ReferencePipe.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            string systemTypeName = systemTypeParameter.AsValueString();
            Connector lastConnector = null;
            do
            {
                Connector connector = FindBelowConnector(element, lastConnector);
                element = FindConnectorOwner(connector, element, systemTypeName);
                lastConnector = connector;
            }
            while (element != null);
        }

        private bool CheckAboveNextElement(Connector aboveConnector)
        {
            bool result = false; //reached top lvl
            if (_topLevel - aboveConnector.Origin.Z < 0) result = true;
            return result;
        }

        private bool CheckBelowNextElement(Connector belowConnector)
        {
            bool result = false; //reached below lvl
            if (_bottomLevel - belowConnector.Origin.Z > 0) result = true;
            return result;
        }

        private Connector FindAboveConnector(Element element, Connector lastConnector)
        {
            Connector result = null;
            LineElements.Add(element);
            //is pipe
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
            {
                Pipe p = element as Pipe;
                ConnectorManager cm = p.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                //loop through "inline" connectors to find the highest
                foreach (Connector c in cs)
                {
                    if (!IsInLine(c)) continue;
                    if (result == null)
                    {
                        result = c;
                    }
                    else
                    {
                        if (c.Origin.Z > result.Origin.Z) result = c;
                    }
                }
            }
            //is pipe fitting
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                FamilyInstance fi = element as FamilyInstance;
                MechanicalFitting mf = fi.MEPModel as MechanicalFitting;
                ConnectorManager cm = mf.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                //loop through "inline" connectors to find the highest
                foreach (Connector c in cs)
                {
                    if (!IsInLine(c)) continue;
                    if (result == null)
                    {
                        result = c;
                    }
                    else
                    {
                        if (c.Origin.Z > result.Origin.Z) result = c;
                    }
                }
            }
            //is pipe accessory
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
            {
                FamilyInstance fi = element as FamilyInstance;
                MEPModel mepModel = fi.MEPModel;
                ConnectorManager cm = mepModel.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                //loop through "inline" connectors to find the highest
                foreach (Connector c in cs)
                {
                    if (!IsInLine(c)) continue;
                    if (result == null)
                    {
                        result = c;
                    }
                    else
                    {
                        if (c.Origin.Z > result.Origin.Z) result = c;
                    }
                }
            }
            //summary
            if (result != null)
            {
                if (!IsConnectorVertical(result)) return null;
                AboveLineElements.Add(element);
                GoesAbove = CheckAboveNextElement(result);
                if (GoesAbove) return null;
            }
            else
            {
                return null;
            }
            if (lastConnector == null || result.Origin.Z > lastConnector.Origin.Z)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private Connector FindBelowConnector(Element element, Connector lastConnector)
        {
            Connector result = null;
            LineElements.Add(element);
            //is pipe
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
            {
                Pipe p = element as Pipe;
                ConnectorManager cm = p.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                //loop through "inline" connectors to find the lowest
                foreach (Connector c in cs)
                {
                    if (!IsInLine(c)) continue;
                    if (result == null)
                    {
                        result = c;
                    }
                    else
                    {
                        if (c.Origin.Z < result.Origin.Z) result = c;
                    }
                }
            }
            //is pipe fitting
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                FamilyInstance fi = element as FamilyInstance;
                MechanicalFitting mf = fi.MEPModel as MechanicalFitting;
                ConnectorManager cm = mf.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                //loop through "inline" connectors to find the lowest
                foreach (Connector c in cs)
                {
                    if (!IsInLine(c)) continue;
                    if (result == null)
                    {
                        result = c;
                    }
                    else
                    {
                        if (c.Origin.Z < result.Origin.Z) result = c;
                    }
                }
            }
            //is pipe accessory
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
            {
                FamilyInstance fi = element as FamilyInstance;
                MEPModel mepModel = fi.MEPModel;
                ConnectorManager cm = mepModel.ConnectorManager;
                ConnectorSet cs = cm.Connectors;
                //loop through "inline" connectors to find the lowest
                foreach (Connector c in cs)
                {
                    if (!IsInLine(c)) continue;
                    if (result == null)
                    {
                        result = c;
                    }
                    else
                    {
                        if (c.Origin.Z < result.Origin.Z) result = c;
                    }
                }
            }
            //summary
            if (result != null)
            {
                if (!IsConnectorVertical(result)) return null;

                BelowLineElements.Add(element);
                GoesBelow = CheckBelowNextElement(result);
                if (GoesBelow) return null;
            }
            else
            {
                return null;
            }
            if (lastConnector == null || result.Origin.Z < lastConnector.Origin.Z)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private void SetOriginCoords()
        {
            LocationCurve lc = ReferencePipe.Location as LocationCurve;
            Line l = lc.Curve as Line;
            _originCoords = l.Origin;
        }

        private bool IsInLine(Connector connector)
        {
            //!!! is in line and goes up or down!!! currently the loop is indefinite

            bool result = false;
            XYZ conOrigin = connector.Origin;
            double tolerance = connector.Radius;
            if(Math.Abs(conOrigin.X - _originCoords.X) < tolerance && Math.Abs(conOrigin.Y - _originCoords.Y) < tolerance)
            {
                result = true;
            }
            return result;
        }

        private Element FindConnectorOwner(Connector connector, Element lastElement, string systemTypeName)
        {
            Element result = null;
            if (connector == null) return null;
            if (!connector.IsConnected) return null;
            ConnectorSet cs = connector.AllRefs;
            foreach (Connector c in cs)
            {
                Element e = c.Owner;
                if(e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                {
                    Pipe pipe = e as Pipe;
                    Parameter parameter = pipe.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                    string name = parameter.AsValueString();
                    if (name != systemTypeName) continue;

                }
                ElementId id = e.Id;
                if (id == ElementId.InvalidElementId || id.IntegerValue == lastElement.Id.IntegerValue 
                    || e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeInsulations
                        || e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipingSystem) continue;
                result = e;
            }
            return result;
        }

        private bool IsConnectorVertical(Connector connector)
        {
            bool result = false;
            if (Math.Abs(connector.CoordinateSystem.BasisZ.Z) > Math.Sin(Math.PI / 180 * 87)) result = true;
            return result;
        }

        private bool IsViaReduced(List<Connector> twoConnectors)
        {
            bool result = false;
            Connector c1 = twoConnectors[0];
            Connector c2 = twoConnectors[1];
            if(Math.Abs(c1.Radius - c2.Radius) > 0.001)
            {
                result = true;
            }
            return result;
        }
    }
}
