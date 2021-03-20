using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeFlowTool
{
    public class DefaultDirections
    {
        public FlowDirection SAN_Schmutzwasser { get; set; }
        public FlowDirection SAN_Ventilation { get; set; }
        public FlowDirection SAN_Regenwasser { get; set; }
        public FlowDirection SAN_Kaltwasser { get; set; }
        public FlowDirection SAN_Warmwasser { get; set; }
        public FlowDirection SAN_Zirkulation { get; set; }
        public FlowDirection HZG_Vorlauf { get; set; }
        public FlowDirection HZG_Rucklauf { get; set; }
        public FlowDirection KAE_Vorlauf { get; set; }
        public FlowDirection KAE_Rucklauf { get; set; }

        public DefaultDirections()
        {
            SAN_Schmutzwasser = FlowDirection.Down;
            SAN_Ventilation = FlowDirection.Up;
            SAN_Regenwasser = FlowDirection.Down;
            HZG_Vorlauf = FlowDirection.Up;
            HZG_Rucklauf = FlowDirection.Down;
            SAN_Kaltwasser = FlowDirection.Up;
            SAN_Warmwasser = FlowDirection.Up;
            SAN_Zirkulation = FlowDirection.Down;
            KAE_Vorlauf = FlowDirection.Down;
            KAE_Rucklauf = FlowDirection.Up;
        }

    }
}
