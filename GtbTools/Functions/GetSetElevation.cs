using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace Functions
{
    public class GetSetElevation
    {
        Document doc;
        List<FamilyInstance> _wallOpenings;

        public GetSetElevation(Document document)
        {
            doc = document;
        }

        public void GetOpenings()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(doc);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                                    .Select(x => x as FamilyInstance)
                                        .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            List<FamilyInstance> allOpenings = new List<FamilyInstance>();
            foreach (var item in genModelInstances)
            {
                Parameter p = item.get_Parameter(new Guid("6674e38a-1c26-498a-bcb0-89856c998d0b"));
                if (p != null) allOpenings.Add(item);
            }
            _wallOpenings = allOpenings.Where(e => e.GetTransform().BasisZ.Z == 0).ToList();
        }

        public void SetElevations(string position, DurchbruchShape shape)
        {
            if(position == "center" && shape == DurchbruchShape.Rectangular)
            {
                SetCenterToRectangular();
            }
            if (position == "center" && shape == DurchbruchShape.Round)
            {
                SetCenterToRectangular();
            }
        }

        public void SetCenterToRound()
        {
            List<FamilyInstance> roundOpenings = _wallOpenings.Where(e => e.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a")) != null).ToList();
            using (Transaction tx = new Transaction(doc, "Copying Elevation to OpElevation"))
            {
                tx.Start();
                foreach (var item in roundOpenings)
                {
                    Parameter defaultElevation = item.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                    string defaultValue = defaultElevation.AsValueString();
                    Parameter openingElevation = item.get_Parameter(new Guid("6674e38a-1c26-498a-bcb0-89856c998d0b"));
                    string openingEl = openingElevation.AsValueString();
                    double value = defaultElevation.AsDouble();
                    if (defaultValue != openingEl)
                    {
                        openingElevation.Set(value);
                    }
                }
                tx.Commit();
            }
        }

        public void SetCenterToRectangular()
        {
            List<FamilyInstance> rectangularOpenings = _wallOpenings.Where(e => e.get_Parameter(new Guid("8eb274b3-fc0c-43e0-a46b-236bf59f292d")) != null).ToList();
            using (Transaction tx = new Transaction(doc, "Copying Elevation to OpElevation"))
            {
                tx.Start();
                foreach (var item in rectangularOpenings)
                {
                    Parameter defaultElevation = item.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                    string defaultValue = defaultElevation.AsValueString();
                    Parameter openingElevation = item.get_Parameter(new Guid("6674e38a-1c26-498a-bcb0-89856c998d0b"));
                    string openingEl = openingElevation.AsValueString();
                    double value = defaultElevation.AsDouble();
                    if (defaultValue != openingEl)
                    {
                        openingElevation.Set(value);
                    }
                }
                tx.Commit();
            }
        }

        public void SetBottomToRound()
        {
            List<FamilyInstance> roundOpenings = _wallOpenings.Where(e => e.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a")) != null).ToList();
            using (Transaction tx = new Transaction(doc, "Copying Elevation to OpElevation"))
            {
                tx.Start();
                foreach (var item in roundOpenings)
                {
                    Parameter defaultElevation = item.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                    double defaultValue = defaultElevation.AsDouble();
                    Parameter openingElevation = item.get_Parameter(new Guid("6674e38a-1c26-498a-bcb0-89856c998d0b"));
                    double openingEl = openingElevation.AsDouble();
                    Parameter diameter = item.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a"));
                    double halfDiameter = diameter.AsDouble() * 0.5;
                    double value = defaultValue - halfDiameter;
                    if (defaultValue != (openingEl + halfDiameter))
                    {
                        openingElevation.Set(value);
                    }
                }
                tx.Commit();
            }
        }

        public void SetTopToRound()
        {
            List<FamilyInstance> roundOpenings = _wallOpenings.Where(e => e.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a")) != null).ToList();
            using (Transaction tx = new Transaction(doc, "Copying Elevation to OpElevation"))
            {
                tx.Start();
                foreach (var item in roundOpenings)
                {
                    Parameter defaultElevation = item.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                    double defaultValue = defaultElevation.AsDouble();
                    Parameter openingElevation = item.get_Parameter(new Guid("6674e38a-1c26-498a-bcb0-89856c998d0b"));
                    double openingEl = openingElevation.AsDouble();
                    Parameter diameter = item.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a"));
                    double halfDiameter = diameter.AsDouble() * 0.5;
                    double value = defaultValue + halfDiameter;
                    if (defaultValue != (openingEl - halfDiameter))
                    {
                        openingElevation.Set(value);
                    }
                }
                tx.Commit();
            }
        }

        public void SetBottomToRectangular()
        {
            List<FamilyInstance> rectangularOpenings = _wallOpenings.Where(e => e.get_Parameter(new Guid("8eb274b3-fc0c-43e0-a46b-236bf59f292d")) != null).ToList();
            using (Transaction tx = new Transaction(doc, "Copying Elevation to OpElevation"))
            {
                tx.Start();
                foreach (var item in rectangularOpenings)
                {
                    Parameter defaultElevation = item.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                    double defaultValue = defaultElevation.AsDouble();
                    Parameter openingElevation = item.get_Parameter(new Guid("6674e38a-1c26-498a-bcb0-89856c998d0b"));
                    double openingEl = openingElevation.AsDouble();
                    Parameter height = item.get_Parameter(new Guid("8eb274b3-fc0c-43e0-a46b-236bf59f292d"));
                    double halfHeight = height.AsDouble() * 0.5;
                    double value = defaultValue - halfHeight;
                    if (defaultValue != (openingEl + halfHeight))
                    {
                        openingElevation.Set(value);
                    }
                }
                tx.Commit();
            }
        }

        public void SetTopToRectangular()
        {
            List<FamilyInstance> rectangularOpenings = _wallOpenings.Where(e => e.get_Parameter(new Guid("8eb274b3-fc0c-43e0-a46b-236bf59f292d")) != null).ToList();
            using (Transaction tx = new Transaction(doc, "Copying Elevation to OpElevation"))
            {
                tx.Start();
                foreach (var item in rectangularOpenings)
                {
                    Parameter defaultElevation = item.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                    double defaultValue = defaultElevation.AsDouble();
                    Parameter openingElevation = item.get_Parameter(new Guid("6674e38a-1c26-498a-bcb0-89856c998d0b"));
                    double openingEl = openingElevation.AsDouble();
                    Parameter height = item.get_Parameter(new Guid("8eb274b3-fc0c-43e0-a46b-236bf59f292d"));
                    double halfHeight = height.AsDouble() * 0.5;
                    double value = defaultValue + halfHeight;
                    if (defaultValue != (openingEl - halfHeight))
                    {
                        openingElevation.Set(value);
                    }
                }
                tx.Commit();
            }
        }
    }
}
