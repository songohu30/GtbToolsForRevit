using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeFlowTool
{
    public class LineViewModel
    {
        public ElementId ReferencePipeId { get; set; }
        public string ReferencePipe { get; set; }
        public List<string> AboveElements { get; set; }
        public List<string> BelowElements { get; set; }
        public List<string> ReducingElements { get; set; }
        public List<string> ViaElements { get; set; }
        public List<string> EndElements { get; set; }
        public string GoesAbove { get; set; }
        public string GoesBelow { get; set; }
        public string TagHolder { get; set; }
        public string IsTagged { get; set; }

        VerticalPipingLine _verticalPipingLine;

        private LineViewModel()
        {

        }

        public static LineViewModel Initialize(VerticalPipingLine verticalPipingLine)
        {
            LineViewModel result = new LineViewModel();
            result._verticalPipingLine = verticalPipingLine;
            result.SetViewModel();
            return result;
        }

        private void SetViewModel()
        {
            ReferencePipeId = _verticalPipingLine.ReferencePipe.Id;
            ReferencePipe = _verticalPipingLine.ReferencePipe.Id.IntegerValue.ToString();
            GoesAbove = _verticalPipingLine.GoesAbove.ToString();
            GoesBelow = _verticalPipingLine.GoesBelow.ToString();
            IsTagged = _verticalPipingLine.IsTagged.ToString();
            if (_verticalPipingLine.TagHolder != null)
            {
                TagHolder = _verticalPipingLine.TagHolder.Id.IntegerValue.ToString();
            }
            else
            {
                TagHolder = _verticalPipingLine.FlowToolFlag.ToString();
            }
            AboveElements = _verticalPipingLine.AboveLineElements.Select(e => e.Id.IntegerValue.ToString()).ToList();
            BelowElements = _verticalPipingLine.BelowLineElements.Select(e => e.Id.IntegerValue.ToString()).ToList();
            ReducingElements = _verticalPipingLine.ReducingElements.Select(e => e.Id.IntegerValue.ToString()).ToList();
            ViaElements = _verticalPipingLine.ViaElements.Select(e => e.Id.IntegerValue.ToString()).ToList();
            EndElements = _verticalPipingLine.EndElements.Select(e => e.Id.IntegerValue.ToString()).ToList();
        }

    }
}
