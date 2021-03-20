using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipesInWall
{
    public class PipeModel
    {
        public ElementId ElementId { get; set; }
        public Level RefLevel { get; set; }
        public Parameter SystemType { get; set; }
        public PipeStatus PipeStatus { get; set; }
        public ConnectorModel ConnectorModel1 { get; set; }
        public ConnectorModel ConnectorModel2 { get; set; }
        public EndResult EndResult { get; set; }

        Pipe _pipe;

        public PipeModel(Pipe pipe)
        {
            _pipe = pipe;
        }

        public void SetProperties()
        {
            ElementId = _pipe.Id;
            RefLevel = _pipe.ReferenceLevel;
            SystemType = _pipe.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
        }

        public void SetConnectorModels(Wall wall)
        {
            ConnectorSet cs = _pipe.ConnectorManager.Connectors;
            List<Connector> connectors = new List<Connector>();
            foreach (Connector c in cs)
            {
                connectors.Add(c);
            }
            ConnectorModel1 = new ConnectorModel(connectors[0], wall);
            ConnectorModel1.SetConnectorModel();
            ConnectorModel2 = new ConnectorModel(connectors[1], wall);
            ConnectorModel2.SetConnectorModel();
        }

        public void SetEndResult()
        {
            //Parameter length = _pipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
            //double metricLength = UnitUtils.ConvertFromInternalUnits(length.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);

            ConnectorResult cr1 = ConnectorModel1.ConnectorResult;
            ConnectorResult cr2 = ConnectorModel2.ConnectorResult;
            EndResult = EndResult.KeineIdee;

            //only pipe fittings
            if(PipeStatus == PipeStatus.OnePoint)
            {
                if (cr1 == ConnectorResult.Rohrformteil_in && cr2 == ConnectorResult.Rohrformteil_out)
                {
                    EndResult = EndResult.Durchbruch;
                }
                if (cr1 == ConnectorResult.Rohrformteil_out && cr2 == ConnectorResult.Rohrformteil_in)
                {
                    EndResult = EndResult.Durchbruch;
                }
            }
            if (PipeStatus == PipeStatus.TwoPointsIn)
            {
                if (cr1 == ConnectorResult.Rohrformteil_in && cr2 == ConnectorResult.Rohrformteil_out)
                {
                    EndResult = EndResult.KeinDurchbruch;
                }
                if (cr1 == ConnectorResult.Rohrformteil_out && cr2 == ConnectorResult.Rohrformteil_in)
                {
                    EndResult = EndResult.KeinDurchbruch;
                }
            }
            if (cr1 == ConnectorResult.Rohrformteil_in && cr2 == ConnectorResult.Rohrformteil_in)
            {
                EndResult = EndResult.KeinDurchbruch;
            }
            if (cr1 == ConnectorResult.Rohrformteil_out && cr2 == ConnectorResult.Rohrformteil_out)
            {
                EndResult = EndResult.Durchbruch;
            }

            //PF or ME with pipe fitting
            if (cr1 == ConnectorResult.Sanitarinst_in || cr1 == ConnectorResult.Sanitarinst_out || cr1 == ConnectorResult.HlsBauteil_in || cr1 == ConnectorResult.HlsBauteil_out)
            {
                if (cr2 == ConnectorResult.Rohrformteil_in) EndResult = EndResult.Anschluss;
                if (cr2 == ConnectorResult.Rohrformteil_out) EndResult = EndResult.Durchbruch;
            }
            if (cr2 == ConnectorResult.Sanitarinst_in || cr2 == ConnectorResult.Sanitarinst_out || cr2 == ConnectorResult.HlsBauteil_in || cr2 == ConnectorResult.HlsBauteil_out)
            {
                if (cr1 == ConnectorResult.Rohrformteil_in) EndResult = EndResult.Anschluss;
                if (cr1 == ConnectorResult.Rohrformteil_out) EndResult = EndResult.Durchbruch;
            }

            //PIF with not connected
            if (cr1 == ConnectorResult.Rohrformteil_in)
            {
                if (cr2 == ConnectorResult.NotConnected_out || cr2 == ConnectorResult.NotConnected_in) EndResult = EndResult.Anschluss;
            }
            if (cr2 == ConnectorResult.Rohrformteil_in)
            {
                if (cr1 == ConnectorResult.NotConnected_out || cr1 == ConnectorResult.NotConnected_in) EndResult = EndResult.Anschluss;
            }

            if (cr1 == ConnectorResult.Rohrformteil_out)
            {
                if (cr2 == ConnectorResult.NotConnected_out || cr2 == ConnectorResult.NotConnected_in) EndResult = EndResult.Durchbruch;
            }
            if (cr2 == ConnectorResult.Rohrformteil_out)
            {
                if (cr1 == ConnectorResult.NotConnected_out || cr1 == ConnectorResult.NotConnected_in) EndResult = EndResult.Durchbruch;
            }

            //PIA with not connected
            if (cr1 == ConnectorResult.Rohrzubehor_in)
            {
                if (cr2 == ConnectorResult.NotConnected_out || cr2 == ConnectorResult.NotConnected_in) EndResult = EndResult.Anschluss;
            }
            if (cr2 == ConnectorResult.Rohrzubehor_in)
            {
                if (cr1 == ConnectorResult.NotConnected_out || cr1 == ConnectorResult.NotConnected_in) EndResult = EndResult.Anschluss;
            }

            if (cr1 == ConnectorResult.Rohrzubehor_out)
            {
                if (cr2 == ConnectorResult.NotConnected_out || cr2 == ConnectorResult.NotConnected_in) EndResult = EndResult.Durchbruch;
            }
            if (cr2 == ConnectorResult.Rohrzubehor_out)
            {
                if (cr1 == ConnectorResult.NotConnected_out || cr1 == ConnectorResult.NotConnected_in) EndResult = EndResult.Durchbruch;
            }


            //PIA with PIF
            if (cr1 == ConnectorResult.Rohrformteil_out)
            {
                if (cr2 == ConnectorResult.Rohrzubehor_out || cr2 == ConnectorResult.Rohrzubehor_in) EndResult = EndResult.Durchbruch;
            }

            if (cr1 == ConnectorResult.Rohrformteil_in)
            {
                if (cr2 == ConnectorResult.Rohrzubehor_out) EndResult = EndResult.Durchbruch;
                if (cr2 == ConnectorResult.Rohrzubehor_in) EndResult = EndResult.KeinDurchbruch;
            }
            if (cr2 == ConnectorResult.Rohrformteil_out)
            {
                if (cr1 == ConnectorResult.Rohrzubehor_out || cr1 == ConnectorResult.Rohrzubehor_in) EndResult = EndResult.Durchbruch;
            }

            if (cr2 == ConnectorResult.Rohrformteil_in)
            {
                if (cr1 == ConnectorResult.Rohrzubehor_out) EndResult = EndResult.Durchbruch;
                if (cr1 == ConnectorResult.Rohrzubehor_in) EndResult = EndResult.KeinDurchbruch;
            }

            if (cr1 == ConnectorResult.Rohrzubehor_out)
            {
                if (cr2 == ConnectorResult.Rohrformteil_out || cr2 == ConnectorResult.Rohrformteil_in) EndResult = EndResult.Durchbruch;
            }

            if (cr1 == ConnectorResult.Rohrzubehor_in)
            {
                if (cr2 == ConnectorResult.Rohrformteil_out) EndResult = EndResult.Durchbruch;
                if (cr2 == ConnectorResult.Rohrformteil_in) EndResult = EndResult.KeinDurchbruch;
            }

            if (cr2 == ConnectorResult.Rohrzubehor_out)
            {
                if (cr1 == ConnectorResult.Rohrformteil_out || cr1 == ConnectorResult.Rohrformteil_in) EndResult = EndResult.Durchbruch;
            }

            if (cr2 == ConnectorResult.Rohrzubehor_in)
            {
                if (cr1 == ConnectorResult.Rohrformteil_out) EndResult = EndResult.Durchbruch;
                if (cr1 == ConnectorResult.Rohrformteil_in) EndResult = EndResult.KeinDurchbruch;
            }
        }

        public void SetParameters()
        {
            if(EndResult == EndResult.Anschluss)
            {
                //Parameter sud = _pipe.LookupParameter("_SuD_nicht_notwendig");
                Parameter hinweise = _pipe.LookupParameter("_SUD_Hinweise_Rohre");
                if(hinweise != null) hinweise.Set("Objekt Anschluss");
            }
            if (EndResult == EndResult.KeinDurchbruch)
            {
                Parameter hinweise = _pipe.LookupParameter("_SUD_Hinweise_Rohre");
                if (hinweise != null) hinweise.Set("Kein Sud");
            }
        }
    }
}
