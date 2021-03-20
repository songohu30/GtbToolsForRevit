using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ExStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace Model
{
    public class DurchbruchModel
    {
        public ElementId ElementId { get; set; }
        public DurchbruchShape Shape { get; set; }
        public Parameter Diameter { get; set; }
        public Parameter PipeDiameter { get; set; }
        public Parameter Width { get; set; }
        public Parameter Height { get; set; }
        public Parameter Depth { get; set; }
        public Parameter CutOffset { get; set; }
        public List<View> Views { get; set; }
        public Parameter OpeningMark { get; set; }
        public Parameter SystemType { get; set; }
        public Parameter FireRating { get; set; }
        public DurchbruchStatus DurchbruchStatus { get; set; }
        public FamilyInstance FamilyInstance { get; set; }
        public OpeningMemory OpeningMemory { get; set; }
        public double NewDiameter { get; set; }
        public double NewOffset { get; set; }

        UIDocument _uiDoc;

        private DurchbruchModel()
        {

        }

        public static DurchbruchModel Initialize(FamilyInstance familyInstance, UIDocument uIDocument)
        {
            DurchbruchModel result = new DurchbruchModel();
            result.FamilyInstance = familyInstance;
            result._uiDoc = uIDocument;
            result.SetId();
            result.SetDimensions();
            result.SetShape();
            result.SetOpeningMark();
            result.SetSystemTypeAndFireRating();
            result.Views = new List<View>();
            result.SetMemory();
            result.SetStatus();
            return result;
        }

        public void SetNewDiameter(Document doc)
        {
            using (Transaction tx = new Transaction(doc, "Setting new diameter"))
            {
                tx.Start();
#if DEBUG2021 || RELEASE2021
                double newValue = UnitUtils.ConvertToInternalUnits(NewDiameter, UnitTypeId.Millimeters);
#else
                double newValue = UnitUtils.ConvertToInternalUnits(NewDiameter, DisplayUnitType.DUT_MILLIMETERS);
#endif
                PipeDiameter.Set(newValue);
                tx.Commit();
            }
        }

        public void SetNewOffset(Document doc)
        {
            using(Transaction tx =  new Transaction(doc, "Setting new offset"))
            {
                tx.Start();
#if DEBUG2021 || RELEASE2021
                double newValue = UnitUtils.ConvertToInternalUnits(NewOffset, UnitTypeId.Millimeters);
#else
                double newValue = UnitUtils.ConvertToInternalUnits(NewOffset, DisplayUnitType.DUT_MILLIMETERS);
#endif
                CutOffset.Set(newValue);
                tx.Commit();
            }
        }

        private void SetSystemTypeAndFireRating()
        {
            SystemType = FamilyInstance.get_Parameter(new Guid("b0a9a9db-7bf3-44e7-a50f-7c8dacbf6214"));
            FireRating = FamilyInstance.get_Parameter(new Guid("731e30b3-63a2-4a2c-bb0c-c98c0f4eb46b"));
        }

        private void SetId()
        {
            ElementId = FamilyInstance.Id;
        }

        private void SetDimensions()
        {
            Depth = FamilyInstance.get_Parameter(new Guid("17a96ef5-1311-49f2-a0d1-4fe5f3f3854b"));
            Width = FamilyInstance.get_Parameter(new Guid("46982e85-76c3-43fb-828f-ddf7a643566f"));
            Height = FamilyInstance.get_Parameter(new Guid("8eb274b3-fc0c-43e0-a46b-236bf59f292d"));
            Diameter = FamilyInstance.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a"));
            CutOffset = FamilyInstance.get_Parameter(new Guid("12f574e0-19fb-46bd-9b7e-0f329356db8a"));
            PipeDiameter = FamilyInstance.LookupParameter("D");
        }

        private void SetShape()
        {
            if(Width != null && Height != null)
            {
                Shape = DurchbruchShape.Rectangular;
            }
            if(Diameter != null)
            {
                Shape = DurchbruchShape.Round;
            }
        }

        private void SetOpeningMark()
        {
            OpeningMark = FamilyInstance.get_Parameter(new Guid("ed25fc8e-129f-4a2b-8d69-4de0c6615ec5"));
        }

        //optimisation: postponed this method
        private void SetViews()
        {
            Views = new List<View>();
            List<View> allViews = new List<View>();
            FilteredElementCollector ficol = new FilteredElementCollector(_uiDoc.Document);
            ficol.OfClass(typeof(View));
            foreach (View view in ficol)
            {
                if (view.IsTemplate) continue;
                if (view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan || view.ViewType == ViewType.EngineeringPlan || view.ViewType == ViewType.Section)
                {
                    allViews.Add(view);
                }
            }
            foreach (View view in allViews)
            {
                FilteredElementCollector ficol2 = new FilteredElementCollector(_uiDoc.Document, view.Id);
                List<FamilyInstance> instances = ficol2.OfClass(typeof(FamilyInstance)).Select(x => x as FamilyInstance).Where(y => y.Id.IntegerValue == FamilyInstance.Id.IntegerValue).ToList();
                if (instances.Count > 0) Views.Add(view);
            }
        }

        private void SetMemory()
        {
            OpeningMemory = OpeningMemory.Initialize(FamilyInstance);
        }

        public void UpdateMemory()
        {
            SetMemory();
        }

        private void SetStatus()
        {
            if (OpeningMemory.IsNew)
            {
                DurchbruchStatus = DurchbruchStatus.New;
                return;
            }
            if (OpeningMemory.IsDimChanged && OpeningMemory.IsPosChanged)
            {
                DurchbruchStatus = DurchbruchStatus.MovedAndResized;
                return;
            }
            if (OpeningMemory.IsPosChanged)
            {
                DurchbruchStatus = DurchbruchStatus.Moved;
                return;
            }
            if (OpeningMemory.IsDimChanged)
            {
                DurchbruchStatus = DurchbruchStatus.Resized;
                return;
            }

            DurchbruchStatus = DurchbruchStatus.Unchanged;
        }

        public void UpdateStatus()
        {
            SetStatus();
        }
    }
}
