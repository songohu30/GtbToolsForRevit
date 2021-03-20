using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeFlowTool
{
    public class ConnectorModel
    {
        public Connector Connector { get; set; }
        public XYZ Coords { get; set; }
        public Element ConnectedElement { get; set; }
        public bool IsPotentialTagHolder { get; set; }

        bool _isConnected;
        bool _isConnectedElementPipe;
        bool _isConnectedPipeVertical;
        PipeDirection _direction;

        private ConnectorModel()
        {

        }

        public static ConnectorModel Initialize(Connector connector)
        {
            ConnectorModel result = new ConnectorModel();
            result.Connector = connector;
            result.SetCoords();
            if(!result.CheckConnection()) return result;
            result.FindConnectedElement();
            if(!result.IsConnectedElementPipe()) return result;
            if (!result.IsConnectedPipeVertical()) return result;
            result.SetPotentiality();
            result.SetDirection();
            return result;
        }

        private void SetDirection()
        {
            Pipe pipeInstance = ConnectedElement as Pipe;
            ConnectorManager conMgr = pipeInstance.ConnectorManager;
            ConnectorSet conSet = conMgr.Connectors;

            //setting connector which is located above or below
            Connector otherConnector = null;
            foreach (Connector c in conSet)
            {
                if (otherConnector == null) otherConnector = c;
                if (Math.Abs(otherConnector.Origin.Z - c.Origin.Z) > 0.001) otherConnector = c;
            }
            XYZ otherConnectorXYZ = otherConnector.Origin;
            if(otherConnectorXYZ.Z - Coords.Z > 0)
            {
                _direction = PipeDirection.Up;
            }
            else
            {
                _direction = PipeDirection.Down;
            }
        }

        private void SetCoords()
        {
            Coords = Connector.Origin;
        }

        private void SetPotentiality()
        {
            if (_isConnectedPipeVertical == true)
            {
                IsPotentialTagHolder = true;
            }
            else
            {
                IsPotentialTagHolder = false;
            }
        }

        private bool CheckConnection()
        {
            _isConnected = Connector.IsConnected;
            return _isConnected;
        }

        private void FindConnectedElement()
        {
            //set should be one element
            ConnectorSet set = Connector.AllRefs;
            Connector connected = null;
            int count = 0;
            foreach (Connector c in set)
            {
                count++;
                connected = c;
            }
            if (count == 0) TaskDialog.Show("info", "0 connected elements");
            if (count > 1) TaskDialog.Show("info", ">1 connected elements");
            ConnectedElement = connected.Owner;
        }

        private bool IsConnectedElementPipe()
        {
            //FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel
            if (ConnectedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
            {
                _isConnectedElementPipe = true;
            }
            else
            {
                _isConnectedElementPipe = false;
            }
            return _isConnectedElementPipe;
        }

        //+/- 3 degrees Z direction < sin(87)
        private bool IsConnectedPipeVertical()
        {
            Pipe pipe = ConnectedElement as Pipe;
            LocationCurve lc = pipe.Location as LocationCurve;
            Curve c = lc.Curve;
            Line l = c as Line;
            XYZ lineDirection = l.Direction;
            double limit = Math.Sin(1.518436449235);
            double test = Math.Abs(lineDirection.Z);
            if(limit < test)
            {
                _isConnectedPipeVertical = true;
            }
            else
            {
                _isConnectedPipeVertical = false;
            }
            return _isConnectedPipeVertical;
        }
    }
}
