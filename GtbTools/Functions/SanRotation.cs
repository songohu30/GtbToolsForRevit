using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using GtbTools;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class SanRotation
    {
        public double RotationAngle { get; set; }
        UIDocument _uIDocument;

        public SanRotation(UIDocument uIDocument)
        {
            _uIDocument = uIDocument;
        }

        public void RotateElements(SelectionFilter selectionFilter, ErrorLog errorLog)
        {
            List<ElementId> ids = new List<ElementId>();
            IList<Reference> references = null;
            try
            {
                references = _uIDocument.Selection.PickObjects(ObjectType.Element, selectionFilter, "Choose elements!");
            }
            catch
            {
                errorLog.WriteToLog("User cancelled selection.");
            }
            if (references == null) return;
            foreach (Reference rf in references)
            {
                ElementId id = _uIDocument.Document.GetElement(rf).Id;
                ids.Add(id);
            }
            InputDialog inputDialog = new InputDialog(this);
            inputDialog.ShowDialog();
            if(RotationAngle != 0)
            {
                FamilyInstance fi = AnalyzeSelection(ids);
                if(fi != null)
                {
                    XYZ origin = (fi.Location as LocationPoint).Point;
                    XYZ ho = fi.HandOrientation;
                    Line axis = Line.CreateBound(origin, new XYZ(origin.X + ho.X, origin.Y + ho.Y, origin.Z + ho.Z));
                    double angle = UnitUtils.ConvertToInternalUnits(RotationAngle, DisplayUnitType.DUT_DECIMAL_DEGREES);
                    using(Transaction tx = new Transaction(_uIDocument.Document, "RotateElements"))
                    {
                        tx.Start();
                        ElementTransformUtils.RotateElements(_uIDocument.Document, ids, axis, angle);
                        tx.Commit();
                    }
                }
            }
        }

        private FamilyInstance AnalyzeSelection(List<ElementId> elements)
        {
            FamilyInstance result = null;
            int check = 0;
            foreach (ElementId id in elements)
            {
                Element el = _uIDocument.Document.GetElement(id);
                if(el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                {
                    result = el as FamilyInstance;
                    check++;
                }
            }
            if(check == 1)
            {
                return result;
            }
            else
            {
                TaskDialog.Show("Info", "Auswahl muss eine Rohrformteil enthalten!");
                return null;
            }
        }
    }
}
