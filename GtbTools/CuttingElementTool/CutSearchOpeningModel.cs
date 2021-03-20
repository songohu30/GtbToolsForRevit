using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingElementTool
{
    public class CutSearchOpeningModel
    {
        public FamilyInstance FamilyInstance { get; set; }
        public string OpeningId { get; set; }
        public string CutElementId { get; set; }
        public Line OpCenterLine { get; set; }
        public CuttingElement CuttingElement { get; set; }
        public string PipeDiameter { get; set; }
        public string OpeningDiameter { get; set; }
        public string OpeningOffset { get; set; }
        public string PipeSlope { get; set; }

        Parameter cutOffset;
        Parameter D;
        Parameter DN;
        XYZ _centerPoint;
        XYZ _originPoint;
        XYZ _endPoint;

        public CutSearchOpeningModel(FamilyInstance familyInstance)
        {
            FamilyInstance = familyInstance;
            OpeningId = familyInstance.Id.IntegerValue.ToString();
        }

        public void SetPoints()
        {
            double depth = GetDepth();
            double halfDepth = 0.5 * GetDepth();
            XYZ ho = FamilyInstance.HandOrientation;
            LocationPoint locationPoint = FamilyInstance.Location as LocationPoint;
            _originPoint = locationPoint.Point;
            double x = _originPoint.X - ho.Y * halfDepth;
            double y = _originPoint.Y + ho.X * halfDepth;
            double z = _originPoint.Z;
            _centerPoint = new XYZ(x, y, z);
            double x2 = _originPoint.X - ho.Y * depth;
            double y2 = _originPoint.Y + ho.X * depth;
            double z2 = _originPoint.Z;
            _endPoint = new XYZ(x2, y2, z2);
        }

        public void SetCenterLine()
        {
            double depth = GetDepth();
            XYZ ho = FamilyInstance.HandOrientation;
            LocationPoint locationPoint = FamilyInstance.Location as LocationPoint;
            XYZ originPoint = locationPoint.Point;
            double x = originPoint.X - ho.Y * depth;
            double y = originPoint.Y + ho.X * depth;
            double z = originPoint.Z;
            XYZ endPoint = new XYZ(x, y, z);
            OpCenterLine = Line.CreateBound(originPoint, endPoint);
        }

        public void SetParameters()
        {
            D = FamilyInstance.LookupParameter("D");
            OpeningDiameter = (D.AsDouble() * 304.8).ToString("F1", CultureInfo.InvariantCulture);
            cutOffset = FamilyInstance.get_Parameter(new Guid("12f574e0-19fb-46bd-9b7e-0f329356db8a"));
            OpeningOffset = (cutOffset.AsDouble() * 304.8).ToString("F1", CultureInfo.InvariantCulture);
            PipeSlope = "N/A";
            CutElementId = "N/A";
            PipeDiameter = "N/A";
        }

        public bool SearchCuttingElement(List<CuttingElement> cuttingElements)
        {
            bool result = false;

            foreach (CuttingElement ce in cuttingElements)
            {
                double distance1 = ce.CenterLine.Distance(_centerPoint);
                double distance2 = ce.CenterLine.Distance(_originPoint);
                double distance3 = ce.CenterLine.Distance(_endPoint);

                if (distance1 < D.AsDouble() * 0.25)
                {
                    CuttingElement = ce;
                    result = true;
                    break;
                }
                if (distance2 < D.AsDouble() * 0.25)
                {
                    CuttingElement = ce;
                    result = true;
                    break;
                }
                if (distance3 < D.AsDouble() * 0.25)
                {
                    CuttingElement = ce;
                    result = true;
                    break;
                }
            }
            if (result == true)
            {
                PipeSlope = CuttingElement.SlopeParameter.AsValueString();
                CutElementId = CuttingElement.ElementId.IntegerValue.ToString();
            }
            return result;
        }

        private double GetDepth()
        {
            Parameter depth = FamilyInstance.get_Parameter(new Guid("17a96ef5-1311-49f2-a0d1-4fe5f3f3854b"));
            double result = depth.AsDouble();
            return result;
        }

        public bool IsDequalDN()
        {
            bool result = false;
            if (CuttingElement.Type == CuttingElementType.Pipes)
            {
                DN = CuttingElement.Element.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            }
            if(CuttingElement.Type == CuttingElementType.Ducts)
            {
                DN = CuttingElement.Element.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
            }
            if (DN == null)
            {
                PipeDiameter = "N/A";
            }
            else
            {
                PipeDiameter = (DN.AsDouble() * 304.8).ToString("F1", CultureInfo.InvariantCulture);
            }
            if (PipeDiameter == OpeningDiameter) result = true;
            return result;
        }

        public void AlignDiameters()
        {
            double newValue = DN.AsDouble();
            D.Set(newValue);
        }
        public void UpdateDiameter()
        {
            D = FamilyInstance.LookupParameter("D");
            OpeningDiameter = D.AsValueString() + " mm";
        }
    }
}
