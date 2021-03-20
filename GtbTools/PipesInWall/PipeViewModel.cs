using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipesInWall
{
    public class PipeViewModel
    {
        public string ElementId { get; set; }
        public string RefLevel { get; set; }
        public string SystemType { get; set; }
        public string Status { get; set; } = "Unknown";
        public string StatusConnector1 { get; set; } = "Unknown";
        public string StatusConnector2 { get; set; } = "Unknown";
        public string EndResult { get; set; } = "Unknown";

        public PipeModel PipeModel { get; set; } 

        public PipeViewModel(PipeModel pipeModel)
        {
            PipeModel = pipeModel;
        }

        public void SetViewModel()
        {
            ElementId = PipeModel.ElementId.IntegerValue.ToString();
            RefLevel = PipeModel.RefLevel.Name;
            SystemType = PipeModel.SystemType.AsValueString();
            if (PipeModel.PipeStatus == PipeStatus.OnePoint) Status = "OnePointIn";
            if (PipeModel.PipeStatus == PipeStatus.TwoPointsIn) Status = "TwoPointsIn";
            StatusConnector1 = Enum.GetName(typeof(ConnectorResult), PipeModel.ConnectorModel1.ConnectorResult);
            StatusConnector2 = Enum.GetName(typeof(ConnectorResult), PipeModel.ConnectorModel2.ConnectorResult);
            EndResult = Enum.GetName(typeof(EndResult), PipeModel.EndResult);
        }
    }
}
