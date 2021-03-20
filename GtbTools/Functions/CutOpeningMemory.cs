using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ExStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Functions
{
    public class CutOpeningMemory
    {
        public XYZ PositionXYZ { get; set; }
        public bool IsRectangular { get; set; }
        public double Diameter { get; set; }
        public double Depth { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public DateTime DateTime { get; set; }
        public List<OpeningMemory> openingMemories;
        List<FamilyInstance> openings;
        public Document _doc;
        public UIDocument _uidoc;

        public CutOpeningMemory(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
        }

        public void GetAllOpenings()
        {
            openings = new List<FamilyInstance>();
            FilteredElementCollector ficol = new FilteredElementCollector(_doc);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                        .Select(x => x as FamilyInstance)
                            .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            foreach (FamilyInstance fi in genModelInstances)
            {
                Parameter gtbParameter = fi.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6"));
                if (gtbParameter != null && gtbParameter.AsString() != "GTB_Tools_location_marker") openings.Add(fi);
            }
        }

        public void SetOpeningMemories()
        {
            openingMemories = new List<OpeningMemory>();
            foreach (FamilyInstance fi in openings)
            {
                OpeningMemory openingMemory = OpeningMemory.Initialize(fi);
                openingMemories.Add(openingMemory);
            }
        }

        public void DisplayChanges()
        {
            foreach (OpeningMemory memory in openingMemories)
            {
                if (memory.IsNew)
                {
                    _uidoc.ShowElements(memory._familyInstance.Id);
                    MessageBox.Show("New opening or not saved");
                }
                if (memory.IsDimChanged)
                {
                    _uidoc.ShowElements(memory._familyInstance.Id);
                    MessageBox.Show("Dimensions changed" + Environment.NewLine + memory.OldDimensions);
                }
                if (memory.IsPosChanged)
                {
                    FamilyInstance instance = SetBall(memory.OldPosition);
                    List<ElementId> ids = new List<ElementId>();
                    ids.Add(instance.Id);
                    ids.Add(memory._familyInstance.Id);
                    _uidoc.Selection.SetElementIds(new List<ElementId>() { memory._familyInstance.Id });
                    _uidoc.ShowElements(ids);
                    MessageBox.Show("Position changed");
                    DeleteElement(instance.Id);
                }
            }
        }

        private FamilyInstance SetBall(string positionXYZ)
        {
            ElementId id = new ElementId(372571);
            FamilySymbol fs = _doc.GetElement(id) as FamilySymbol;
            

            string[] coords = positionXYZ.Split(';');
            double x = Convert.ToDouble(coords[0], System.Globalization.CultureInfo.InvariantCulture);
            double y = Convert.ToDouble(coords[1], System.Globalization.CultureInfo.InvariantCulture);
            double z = Convert.ToDouble(coords[2], System.Globalization.CultureInfo.InvariantCulture);

            XYZ xyz = new XYZ(x, y, z);

            Transaction tx = new Transaction(_doc, "insert");
            tx.Start();
            FamilyInstance instance = _doc.Create.NewFamilyInstance(xyz, fs, StructuralType.NonStructural);
            tx.Commit();
            return instance;
        }
        private void DeleteElement(ElementId id)
        {
            Transaction tx = new Transaction(_doc, "delete");
            tx.Start();
            _doc.Delete(id);
            tx.Commit();
        }

    }
}
