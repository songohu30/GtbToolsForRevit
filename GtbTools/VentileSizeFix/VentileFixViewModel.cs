using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentileSizeFix
{
    public class VentileFixViewModel
    {
        public ChangedValve ChangedValve { get; set; }
        public string ElementId { get; set; } = "Nicht gefunden";
        public string ReferenceLevel { get; set; } = "Nicht gefunden";
        public string SystemType { get; set; } = "Nicht gefunden";
        public string CurrentType { get; set;} = "Nicht gefunden";
        public string NewType { get; set; } = "Nicht gefunden";

        public VentileFixViewModel(ChangedValve changedValve)
        {
            ChangedValve = changedValve;
        }

        public void SetProperties()
        {
            ElementId = ChangedValve.ElementId.IntegerValue.ToString();
            CurrentType = ChangedValve.CurrentSymbol.Name;

            if(ChangedValve.FamilyInstance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM) != null)
            {
                ReferenceLevel = ChangedValve.FamilyInstance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).AsValueString();
            }

            if (ChangedValve.FamilyInstance.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM) != null)
            {
                SystemType = ChangedValve.FamilyInstance.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM).AsValueString();
            }

            if (ChangedValve.NewSymbol != null)
            {
                NewType = ChangedValve.NewSymbol.Name;
            }
        }
    }
}
