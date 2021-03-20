using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeFlowTool
{
    public class SameCoordinatePipes
    {
        public XYZ Coordinates { get; set; }
        public List<Pipe> Pipes { get; set; }

        public SameCoordinatePipes(XYZ newCoords)
        {
            Coordinates = newCoords;
            Pipes = new List<Pipe>();
        }

        public bool IsPipeWithinTolerance(Pipe pipe)
        {
            bool result = false;
            double tolerance = pipe.Diameter * 0.5;
            LocationCurve lc = pipe.Location as LocationCurve;
            Line l = lc.Curve as Line;
            XYZ pipeOrigin = l.Origin;
            if (Math.Abs(pipeOrigin.X - Coordinates.X) < tolerance && Math.Abs(pipeOrigin.Y - Coordinates.Y) < tolerance) result = true;
            return result;
        }

        public void AddToList(Pipe pipe)
        {
            Pipes.Add(pipe);
        }
    }
}
