using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipesInWall
{
    public class ConnectorModel
    {
        public Connector Connector { get; set; }
        public bool IsConnected { get; set; }
        public bool IsInWall { get; set; }
        public FamilyInstance ConElement { get; set; }
        public ConnectorResult ConnectorResult { get; set; }

        Wall _wall;

        public ConnectorModel(Connector connector, Wall wall)
        {
            Connector = connector;
            _wall = wall;
        }

        public void SetConnectorModel()
        {
            CheckConnection();
            CheckLocation();
            if(IsConnected)
            {
                AnalyzeConnectedElement();
            }
            else
            {
                if (IsInWall)
                {
                    ConnectorResult = ConnectorResult.NotConnected_in;
                }
                else
                {
                    ConnectorResult = ConnectorResult.NotConnected_out;
                }
            }
        }

        private void CheckConnection()
        {
            IsConnected = Connector.IsConnected;
        }

        private void CheckLocation()
        {
            Element element = _wall as Element;
            BoundingBoxXYZ box = element.get_BoundingBox(null);
            XYZ min = box.Min;
            XYZ max = box.Max;
            XYZ origin = Connector.Origin;
            bool x = false;
            bool y = false;
            bool z = false;

            if (origin.X > min.X && origin.X < max.X) x = true;
            if (origin.Y > min.Y && origin.Y < max.Y) y = true;
            if (origin.Z > min.Z && origin.Z < max.Z) z = true;

            IsInWall = x && y && z;
        }

        private void AnalyzeConnectedElement()
        {
            ConnectorSet cs = Connector.AllRefs;
            ConElement = null;
            foreach (Connector c in cs)
            {
                Element e = c.Owner;
                FamilyInstance fi = e as FamilyInstance;
                if (fi != null) ConElement = fi;
            }

            if(ConElement == null)
            {
                ConnectorResult = ConnectorResult.NoElementFound;
                return;
            }

            switch(ConElement.Category.Id.IntegerValue)
            {
                case (int)BuiltInCategory.OST_PipeFitting:
                    MechanicalFitting mf1 = ConElement.MEPModel as MechanicalFitting;
                    ConnectorSet cs1 = mf1.ConnectorManager.Connectors;
                    if (IsInWall)
                    {
                        if(AreConnectorsInWall(cs1))
                        {
                            ConnectorResult = ConnectorResult.Rohrformteil_in;
                        }
                        else
                        {
                            ConnectorResult = ConnectorResult.Rohrformteil_out;
                        }
                    }
                    else
                    {
                        ConnectorResult = ConnectorResult.Rohrformteil_out;
                    }
                    break;
                case (int)BuiltInCategory.OST_PipeAccessory:
                    MEPModel mf2 = ConElement.MEPModel;
                    ConnectorSet cs2 = mf2.ConnectorManager.Connectors;
                    if (IsInWall)
                    {
                        if (AreConnectorsInWall(cs2))
                        {
                            ConnectorResult = ConnectorResult.Rohrzubehor_in;
                        }
                        else
                        {
                            ConnectorResult = ConnectorResult.Rohrzubehor_out;
                        }
                    }
                    else
                    {
                        ConnectorResult = ConnectorResult.Rohrzubehor_out;
                    }
                    break;
                case (int)BuiltInCategory.OST_PlumbingFixtures:
                    MEPModel mf3 = ConElement.MEPModel;
                    ConnectorSet cs3 = mf3.ConnectorManager.Connectors;
                    if (IsInWall)
                    {
                        if (AreConnectorsInWall(cs3))
                        {
                            ConnectorResult = ConnectorResult.Sanitarinst_in;
                        }
                        else
                        {
                            ConnectorResult = ConnectorResult.Sanitarinst_out;
                        }
                    }
                    else
                    {
                        ConnectorResult = ConnectorResult.Sanitarinst_out;
                    }
                    break;
                case (int)BuiltInCategory.OST_MechanicalEquipment:
                    MechanicalEquipment mf4 = ConElement.MEPModel as MechanicalEquipment;
                    ConnectorSet cs4 = mf4.ConnectorManager.Connectors;
                    if (IsInWall)
                    {
                        if (AreConnectorsInWall(cs4))
                        {
                            ConnectorResult = ConnectorResult.HlsBauteil_in;
                        }
                        else
                        {
                            ConnectorResult = ConnectorResult.HlsBauteil_out;
                        }
                    }
                    else
                    {
                        ConnectorResult = ConnectorResult.HlsBauteil_out;
                    }
                    break;
                default:
                    if(IsInWall)
                    {
                        ConnectorResult = ConnectorResult.Unbekannt_in;
                    }
                    else
                    {
                        ConnectorResult = ConnectorResult.Unbekannt_out;
                    }
                    break;
            }
        }

        private bool AreConnectorsInWall(ConnectorSet connectorSet)
        {
            bool result = true;
            foreach (Connector c in connectorSet)
            {
                Element element = _wall as Element;
                BoundingBoxXYZ box = element.get_BoundingBox(null);
                XYZ min = box.Min;
                XYZ max = box.Max;
                XYZ origin = c.Origin;
                bool x = false;
                bool y = false;
                bool z = false;

                if (origin.X > min.X && origin.X < max.X) x = true;
                if (origin.Y > min.Y && origin.Y < max.Y) y = true;
                if (origin.Z > min.Z && origin.Z < max.Z) z = true;
                bool logicalSum = x && y && z;
                if (!logicalSum) result = false;
            }
            return result;
        }
    }
}
