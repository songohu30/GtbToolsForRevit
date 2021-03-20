using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class SelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem == null) return false;
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves) return true;
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting) return true;
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory) return true;
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
