using Autodesk.Revit.DB;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingElementTool
{
    public class CuttingElement
    {
        //cut openings cutting elements: ducts, pipes, cable trays, conduits
        //yet pipes only
        public Element Element { get; set; }
        public ElementId ElementId { get; set; }
        public CuttingElementType Type { get; set; }
        public Curve CenterCurve { get; set; }
        public Line CenterLine { get; set; }
        public double Slope { get; set; }
        public Parameter SlopeParameter { get; set; }
        
        private CuttingElement()
        {

        }

        public static CuttingElement Initialize(Element element, CuttingElementType type)
        {
            CuttingElement result = new CuttingElement();
            result.Element = element;
            result.ElementId = element.Id;
            result.Type = type;
            result.SetLine();
            result.SetSlope();
            return result;
        }

        private void SetLine()
        {
            LocationCurve locationCurve = Element.Location as LocationCurve;
            CenterLine = locationCurve.Curve as Line;
        }

        private void SetSlope()
        {
            if(Type == CuttingElementType.Pipes)
            {
                SlopeParameter = Element.get_Parameter(BuiltInParameter.RBS_PIPE_SLOPE);
                Slope = SlopeParameter.AsDouble();
            }
            if (Type == CuttingElementType.Ducts)
            {
                SlopeParameter = Element.get_Parameter(BuiltInParameter.RBS_DUCT_SLOPE);
                Slope = SlopeParameter.AsDouble();
            }
        }
    }
}
