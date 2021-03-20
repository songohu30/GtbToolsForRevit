using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace Functions
{
    public class PipeBreaker
    {
        Pipe _pipe;
        XYZ _centerPoint;
        Document _document;
        List<Connector> _connectors;

        private PipeBreaker()
        {

        }

        public static PipeBreaker Initialize(ElementId pipeId, Document document)
        {
            PipeBreaker result = new PipeBreaker();
            result._document = document;
            result._pipe = document.GetElement(pipeId) as Pipe;
            if (result.PipeNull()) return null;
            result.SetCenterPoint();
            result.SetConnectors();
            return result;
        }

        public void UnionCreate(FamilySymbol familySymbol, Level referenceLevel)
        {
            Level pipeRefLevel = _pipe.ReferenceLevel;
            using (Transaction tx = new Transaction(_document, "Create Union"))
            {
                tx.Start();                
                FamilyInstance union = _document.Create.NewFamilyInstance(_centerPoint, familySymbol, referenceLevel, StructuralType.NonStructural);
                Parameter offset = union.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM);
                double offsetValue = _centerPoint.Z - referenceLevel.Elevation;
                offset.Set(offsetValue);
                RotateToVertical(union);
                SetDiameter(union);
                double newOffset = 10; //mm
                offset.Set(UnitUtils.ConvertToInternalUnits(newOffset, DisplayUnitType.DUT_MILLIMETERS));
                foreach (Connector connector in _connectors)
                {
                    DrawPipe(connector, union);
                }          
                _document.Delete(_pipe.Id);
                tx.Commit();
            }
            TaskDialog.Show("Info", "Successfully added tag holder!");
        }

        private bool PipeNull()
        {
            bool result = false;
            if (_pipe == null) result = true;
            return result;
        }
        
        private void RotateToVertical(FamilyInstance union)
        {
            Line axis = Line.CreateBound(_centerPoint, new XYZ(_centerPoint.X, _centerPoint.Y + 1, _centerPoint.Z));
            double angle = Math.PI / 180 * 90;
            ElementTransformUtils.RotateElement(_document, union.Id, axis, angle);
        }

        private void SetDiameter(FamilyInstance union)
        {
            Parameter diameter = union.LookupParameter("DN_connector1");
            diameter.Set(_pipe.Diameter);
        }

        private void SetCenterPoint()
        {
            LocationCurve lc = _pipe.Location as LocationCurve;
            Line line = lc.Curve as Line;
            IList<XYZ> points = line.Tessellate();
            XYZ point1 = points[0];
            XYZ point2 = points[1];
            _centerPoint = new XYZ((point1.X + point2.X) * 0.5, (point1.Y + point2.Y) * 0.5, (point1.Z + point2.Z) * 0.5);
        }

        private void SetConnectors()
        {
            _connectors = new List<Connector>();
            ConnectorManager cm = _pipe.ConnectorManager;
            ConnectorSet cSet = cm.Connectors;
            foreach (Connector c in cSet)
            {
                _connectors.Add(c);
            }
        }

        private void DrawPipe(Connector connector, FamilyInstance union)
        {
            ConnectorSet unionConnectors = (union.MEPModel as MechanicalFitting).ConnectorManager.Connectors;
            ElementId pipeTypeId = _pipe.GetTypeId();
            ElementId levelId = _pipe.ReferenceLevel.Id;
            ElementId systemTypeId = (_pipe.MEPSystem as PipingSystem).GetTypeId();

            if (connector.IsConnected)
            {
                Connector refElConnector = FindRefConnector(connector);
                Connector unionConnector = GetClosestConnector(connector.Origin, unionConnectors);
                Pipe pipe = Pipe.Create(_document, pipeTypeId, levelId, unionConnector, refElConnector);
                Parameter pipeDN = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                pipeDN.Set(_pipe.Diameter);
                pipe.SetSystemType(systemTypeId);
            }
            else
            {
                XYZ endPoint = connector.Origin;
                Connector unionConnector = GetClosestConnector(endPoint, unionConnectors);
                Pipe pipe = Pipe.Create(_document, pipeTypeId , levelId, unionConnector, endPoint);
                Parameter pipeDN = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                pipeDN.Set(_pipe.Diameter);
                pipe.SetSystemType(systemTypeId);
            }
        }

        private Connector GetClosestConnector(XYZ endPoint, ConnectorSet unionConectors)
        {
            Connector result = null;
            double distance = 0;
            foreach (Connector c in unionConectors)
            {
                if (result == null)
                {
                    result = c;
                    distance = c.Origin.DistanceTo(endPoint);
                }
                else
                {
                    if (c.Origin.DistanceTo(endPoint) < distance) result = c;
                }
            }
            return result;
        }

        private Connector FindRefConnector(Connector connector)
        {
            Connector result = null;
            ConnectorSet cSet = connector.AllRefs;
            foreach (Connector c in cSet)
            {
                if(c.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting 
                    || c.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory 
                        || c.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures)
                {
                    result = c;
                }
            }
            return result;
        }
    }
}
