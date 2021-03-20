using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeFlowTool
{
    public class ManualVerticalLine
    {
		public Pipe ReferencePipe { get; set; }
		public Element TagHolder { get; set; }

		public ManualVerticalLine(Pipe pipe)
		{
			ReferencePipe = pipe;
		}

		public void GetTagHolder()
		{
			ConnectorManager cm = ReferencePipe.ConnectorManager;
			ConnectorSet cs = cm.Connectors;
			foreach (Connector c in cs)
			{
				foreach (Connector csub in c.AllRefs)
				{
					Element e = csub.Owner;
					if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
					{
						TagHolder = e;
					}
				}
			}
		}

		public void AnnotateLine(Document doc, ElementId theTagId)
		{
			if (TagHolder == null) return;
			XYZ originCoords = ((ReferencePipe.Location as LocationCurve).Curve as Line).Origin;
			Reference reference = new Reference(TagHolder);

#if DEBUG2018 || RELEASE2018
			IndependentTag newTag = IndependentTag.Create(doc, doc.ActiveView.Id, reference, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, originCoords);
			newTag.ChangeTypeId(theTagId);
#else
            IndependentTag.Create(doc, theTagId, doc.ActiveView.Id, reference, false, TagOrientation.Horizontal, originCoords);
#endif
		}
	}
}
