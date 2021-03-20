using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class ForceConnection
    {
        Document _document;
        public ForceConnection(Document document)
        {
            _document = document;
        }

        public void ConnectElements(IList<Reference> references)
        {
            FamilyInstance fi1 = null;
            FamilyInstance fi2 = null;
            Connector c1 = null;
            Connector c2 = null;

            if(references.Count == 2)
            {
                fi1 = _document.GetElement(references[0]) as FamilyInstance;
                fi2 = _document.GetElement(references[1]) as FamilyInstance;
                if (fi1 == null || fi2 == null) return;
            }
            else
            {
                return;
            }

            if(fi1.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                MechanicalFitting mf = fi1.MEPModel as MechanicalFitting;
                ConnectorSet cs = mf.ConnectorManager.UnusedConnectors;
                int count = cs.Size;
                if(count == 1)
                {

                    foreach (Connector c in cs)
                    {
                        c1 = c;
                    }
                }
            }

            if (fi2.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                MechanicalFitting mf = fi2.MEPModel as MechanicalFitting;
                ConnectorSet cs = mf.ConnectorManager.UnusedConnectors;
                int count = cs.Size;
                if (count == 1)
                {

                    foreach (Connector c in cs)
                    {
                        c2 = c;
                    }
                }
            }

            if(c1 != null && c2 != null)
            {
                ElementId id = _document.GetDefaultElementTypeId(ElementTypeGroup.PipeType);
                ElementId levelId = GetLevel(fi1);
                
                using(Transaction tx = new Transaction(_document, "ForceConnection"))
                {
                    tx.Start();
                    Pipe.Create(_document, id, levelId, c1, c2);
                    tx.Commit();
                }
            }
        }

        private ElementId GetLevel(FamilyInstance familyInstance)
        {
            if(_document.ActiveView.ViewType == ViewType.FloorPlan)
            {
                return _document.ActiveView.GenLevel.Id;
            }
            else
            {
                return familyInstance.LevelId;
            }
        }
    }
}
