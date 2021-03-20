using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GUI;
using OpeningSymbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Functions
{
    public class OpeningTagger
    {
        public Document Document { get; set; }
        public Document LinkedDocument { get; set; }

        public List<FamilySymbol> GenericModelTags { get; set; }
        public bool IsOpeningModelLinked { get; set; } = false;
        public List<RevitLinkInstance> Links { get; set; }
        public List<XYZ> TagHeadPositions { get; set; }

        double _absoluteCutPlane;
        double _absoluteBotomPlane;
        double _absoluteUpperPlane;
        List<FamilyInstance> _linkedOpenings;

        List<FamilyInstance> wallInstances;
        List<FamilyInstance> bodenInstances;
        List<FamilyInstance> deckenInstances;

        ElementId wallTagId;
        ElementId floorTagId;
        ElementId ceilingTagId;

        List<ElementId> taggedWallIds;
        List<ElementId> taggedFloorIds;
        List<ElementId> taggedCeilingIds;

        int newTagsCount = 0;
        RevitLinkInstance _selectedLinkInstance;

        private OpeningTagger()
        {

        }

        public static OpeningTagger Initialize(Document document)
        {
            OpeningTagger result = new OpeningTagger();
            result.Document = document;
            result.SetRevitLinks();
            result.SetGenericModelTags();
            result.SetOpenings();

            return result;
        }

        public void TagThemAll()
        {
            GetAllTaggedIds();
            if (wallTagId == null || floorTagId == null || ceilingTagId == null)
            {
                TaskDialog.Show("Info", "Bitte wählen Sie alle Typen aus");
                return;
            }

            using (Transaction tx = new Transaction(Document, "Multi tagging"))
            {
                tx.Start();
                foreach (FamilyInstance fi in wallInstances)
                {
                    ElementId openingId = fi.Id;
                    if (taggedWallIds.Contains(openingId)) continue;
                    Element element = fi as Element;
                    LocationPoint lp = fi.Location as LocationPoint;
                    XYZ xyz = lp.Point;
                    Reference reference = new Reference(element);
#if DEBUG2018 || RELEASE2018
                    IndependentTag newTag = IndependentTag.Create(Document, Document.ActiveView.Id, reference, true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, xyz);
                    newTag.ChangeTypeId(wallTagId);
#else
                    IndependentTag newTag = IndependentTag.Create(Document, wallTagId, Document.ActiveView.Id, reference, true, TagOrientation.Horizontal, xyz);
#endif
                    newTagsCount++;
                }
                foreach (FamilyInstance fi in deckenInstances)
                {
                    ElementId openingId = fi.Id;
                    if (taggedCeilingIds.Contains(openingId)) continue;
                    Element element = fi as Element;
                    LocationPoint lp = fi.Location as LocationPoint;
                    XYZ xyz = lp.Point;
                    Reference reference = new Reference(element);
#if DEBUG2018 || RELEASE2018
                    IndependentTag newTag = IndependentTag.Create(Document, Document.ActiveView.Id, reference, true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, xyz);
                    newTag.ChangeTypeId(ceilingTagId);
#else
                    IndependentTag newTag = IndependentTag.Create(Document, ceilingTagId, Document.ActiveView.Id, reference, true, TagOrientation.Horizontal, xyz);
#endif
                    XYZ thPos = newTag.TagHeadPosition;
                    XYZ newPosition = new XYZ(thPos.X, thPos.Y + 0.1, thPos.Z);
                    newTag.TagHeadPosition = newPosition;
                    newTagsCount++;
                }
                foreach (FamilyInstance fi in bodenInstances)
                {
                    ElementId openingId = fi.Id;
                    if (taggedFloorIds.Contains(openingId)) continue;
                    Element element = fi as Element;
                    LocationPoint lp = fi.Location as LocationPoint;
                    XYZ xyz = lp.Point;
                    Reference reference = new Reference(element);
#if DEBUG2018 || RELEASE2018
                    IndependentTag newTag = IndependentTag.Create(Document, Document.ActiveView.Id, reference, true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, xyz);
                    newTag.ChangeTypeId(floorTagId);
#else
                    IndependentTag newTag = IndependentTag.Create(Document, floorTagId, Document.ActiveView.Id, reference, true, TagOrientation.Horizontal, xyz);
#endif
                    newTagsCount++;
                }
                tx.Commit();
            }
        }

        public GtbWindowResult DisplayWindow()
        {
            QuickTagWindow quickTagWindow = new QuickTagWindow(GenericModelTags, Links);
            quickTagWindow.SetLists();
            quickTagWindow.ShowDialog();
            wallTagId = quickTagWindow.WandSymbol;
            floorTagId = quickTagWindow.BodenSymbol;
            ceilingTagId = quickTagWindow.DeckenSymbol;
            IsOpeningModelLinked = quickTagWindow.IsOpeningModelLinked;
            if(IsOpeningModelLinked && quickTagWindow.SelectedLinkInstance != null)
            {
                LinkedDocument = quickTagWindow.SelectedLinkInstance.GetLinkDocument();
                _selectedLinkInstance = quickTagWindow.SelectedLinkInstance;
            }
            return quickTagWindow.WindowResult;
        }

        public void AnnotateLinkedOpenings()
        {
            SetViewPlanesValues();
            GetTaggedPositions();
            GetLinkedOpenings();

            using (Transaction tx = new Transaction(Document, "Multi tagging"))
            {
                tx.Start();
                foreach (FamilyInstance fi in _linkedOpenings)
                {
                    ElementId categoryId = fi.Host.Category.Id;
                    if (categoryId.IntegerValue == (int)BuiltInCategory.OST_Walls)
                    {

                        Reference reference = new Reference(fi).CreateLinkReference(_selectedLinkInstance);                      
                        XYZ origin = (fi.Location as LocationPoint).Point;
                        if (IsOpeningOnView(fi))
                        {
#if DEBUG2018 || RELEASE2018
                            IndependentTag newTag = IndependentTag.Create(Document, Document.ActiveView.Id, reference, true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, origin);
                            newTag.ChangeTypeId(wallTagId);
#else
                            IndependentTag newTag = IndependentTag.Create(Document, wallTagId, Document.ActiveView.Id, reference, true, TagOrientation.Horizontal, origin);
#endif
                            if(IsPositionTaken(newTag))
                            {
                                Document.Delete(newTag.Id);
                            }
                            else
                            {
                                newTagsCount++;
                            }
                        }
                    }

                    if (categoryId.IntegerValue == (int)BuiltInCategory.OST_Floors || categoryId.IntegerValue == (int)BuiltInCategory.OST_Ceilings)
                    {

                        Reference reference = new Reference(fi).CreateLinkReference(_selectedLinkInstance);
                        XYZ origin = (fi.Location as LocationPoint).Point;
                        ElementId tagId;
                        if (origin.Z > _absoluteCutPlane)
                        {
                            tagId = ceilingTagId;
                        }
                        else
                        {
                            tagId = floorTagId;
                        }
                        
                        if (IsOpeningOnView(fi))
                        {
#if DEBUG2018 || RELEASE2018
                            IndependentTag newTag = IndependentTag.Create(Document, Document.ActiveView.Id, reference, true, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, origin);
                            newTag.ChangeTypeId(tagId);
#else
                            IndependentTag newTag = IndependentTag.Create(Document, tagId, Document.ActiveView.Id, reference, true, TagOrientation.Horizontal, origin);
#endif
                            if (IsPositionTaken(newTag))
                            {
                                Document.Delete(newTag.Id);
                            }
                            else
                            {
                                newTagsCount++;
                            }
                        }
                    }
                }
                tx.Commit();
            }
        }

        private void GetLinkedOpenings()
        {
            _linkedOpenings = new List<FamilyInstance>();
            FilteredElementCollector ficol = new FilteredElementCollector(LinkedDocument);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            foreach (FamilyInstance fi in genModelInstances)
            {
                Parameter gtbParameter = fi.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6"));
                if (gtbParameter != null && gtbParameter.AsString() != "GTB_Tools_location_marker")
                {
                    _linkedOpenings.Add(fi);
                }
            }
        }

        private bool IsOpeningOnView(FamilyInstance familyInstance)
        {
            BoundingBoxXYZ viewBox = Document.ActiveView.get_BoundingBox(null);
            XYZ origin = (familyInstance.Location as LocationPoint).Point;
            bool x = true;
            bool y = true;
            bool z = true;
            if (origin.X < viewBox.Min.X || origin.X > viewBox.Max.X) x = false;
            if (origin.Y < viewBox.Min.Y || origin.Y > viewBox.Max.Y) y = false;
            if (origin.Z < _absoluteBotomPlane - 0.001 || origin.Z > _absoluteUpperPlane + 0.001) z = false;
            return x & y & z;
        }

        private bool IsPositionTaken(IndependentTag newTag)
        {
            bool result = false;
            XYZ position = newTag.TagHeadPosition;
            foreach (XYZ tagHead in TagHeadPositions)
            {
                if (position.DistanceTo(tagHead) < 0.0001) result = true;
            }
            return result;
        }

        private void GetTaggedPositions()
        {
            List<IndependentTag> genericModelTags = new FilteredElementCollector(Document, Document.ActiveView.Id).OfClass(typeof(IndependentTag))
                                                        .Select(e => e as IndependentTag).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModelTags).ToList();
            TagHeadPositions = new List<XYZ>();
            foreach (IndependentTag tag in genericModelTags)
            {
                TagHeadPositions.Add(tag.TagHeadPosition);
            }
        }

        private void SetViewPlanesValues()
        {
            View activeView = Document.ActiveView;
            ViewPlan viewPlan = activeView as ViewPlan;
            PlanViewRange planViewRange = viewPlan.GetViewRange();
            ElementId cutPlaneId = planViewRange.GetLevelId(PlanViewPlane.CutPlane);
            double cutPlaneOffset = planViewRange.GetOffset(PlanViewPlane.CutPlane);
            ElementId topPlaneId = planViewRange.GetLevelId(PlanViewPlane.TopClipPlane);
            double topPlaneOffset = planViewRange.GetOffset(PlanViewPlane.TopClipPlane);
            ElementId bottomPlaneId = planViewRange.GetLevelId(PlanViewPlane.BottomClipPlane);
            double bottomPlaneOffset = planViewRange.GetOffset(PlanViewPlane.BottomClipPlane);
            if (cutPlaneId.IntegerValue > 0)
            {
                Level level = Document.GetElement(cutPlaneId) as Level;
                double elevation = level.Elevation;
                _absoluteCutPlane = elevation + cutPlaneOffset;
            }
            if (topPlaneId.IntegerValue > 0)
            {
                Level level = Document.GetElement(topPlaneId) as Level;
                double elevation = level.Elevation;
                _absoluteUpperPlane = elevation + topPlaneOffset;
            }
            if (bottomPlaneId.IntegerValue > 0)
            {
                Level level = Document.GetElement(bottomPlaneId) as Level;
                double elevation = level.Elevation;
                _absoluteBotomPlane = elevation + bottomPlaneOffset;
            }
        }

        private void SetRevitLinks()
        {
            Links = new FilteredElementCollector(Document).OfClass(typeof(RevitLinkInstance)).Select(e => e as RevitLinkInstance).ToList();
        }

        private void SetGenericModelTags()
        {
            GenericModelTags = new List<FamilySymbol>();
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            GenericModelTags = ficol.OfClass(typeof(FamilySymbol))
                                    .Select(x => x as FamilySymbol)
                                        .Where(y => y.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModelTags).ToList();
        }

        private void SetOpenings()
        {
            PlanView planView = new PlanView(Document, Document.ActiveView, OpeningSymbol.ViewDiscipline.TGA);
            planView.CreateOpeningList();
            List<RoundOpening> roundOpenings = planView.RoundOpenings;
            List<RectangularOpening> rectangularOpenings = planView.RectangularOpenings;
            wallInstances = new List<FamilyInstance>();
            bodenInstances = new List<FamilyInstance>();
            deckenInstances = new List<FamilyInstance>();
            foreach (RoundOpening ro in roundOpenings)
            {
                if (ro.OpeningHost == OpeningHost.Wall) wallInstances.Add(ro.FamilyInstance);
                if (ro.OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    if(ro.PlanViewLocation == PlanViewLocation.AboveCutPlane)
                    {
                        deckenInstances.Add(ro.FamilyInstance);
                    }
                    else
                    {
                        bodenInstances.Add(ro.FamilyInstance);
                    }
                }
            }
            foreach (RectangularOpening ro in rectangularOpenings)
            {
                if (ro.OpeningHost == OpeningHost.Wall) wallInstances.Add(ro.FamilyInstance);
                if (ro.OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    if (ro.PlanViewLocation == PlanViewLocation.AboveCutPlane)
                    {
                        deckenInstances.Add(ro.FamilyInstance);
                    }
                    else
                    {
                        bodenInstances.Add(ro.FamilyInstance);
                    }

                }
            }
        }

        private void GetAllTaggedIds()
        {
            List<IndependentTag> wallTags = new List<IndependentTag>();
            List<IndependentTag> floorTags = new List<IndependentTag>();
            List<IndependentTag> ceilingTags = new List<IndependentTag>();
            FilteredElementCollector ficol1 = new FilteredElementCollector(Document, Document.ActiveView.Id);
            FilteredElementCollector ficol2 = new FilteredElementCollector(Document, Document.ActiveView.Id);
            FilteredElementCollector ficol3 = new FilteredElementCollector(Document, Document.ActiveView.Id);
            wallTags = ficol1.OfClass(typeof(IndependentTag)).Select(x => x as IndependentTag).ToList();
            floorTags = ficol2.OfClass(typeof(IndependentTag)).Select(x => x as IndependentTag).ToList();
            ceilingTags = ficol3.OfClass(typeof(IndependentTag)).Select(x => x as IndependentTag).ToList();
            taggedWallIds = wallTags.Select(x => x.TaggedLocalElementId).ToList();
            taggedFloorIds = floorTags.Select(x => x.TaggedLocalElementId).ToList();
            taggedCeilingIds = ceilingTags.Select(x => x.TaggedLocalElementId).ToList();
        }

        public static bool IsValidViewType(Document doc)
        {
            bool result = false;
            View v = doc.ActiveView;
            if (v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.CeilingPlan || v.ViewType == ViewType.EngineeringPlan)
            {
                result = true;
            }
            return result;
        }
        
        public void ShowNewTagsInfo()
        {
            string info1 = String.Format("Es wurde {0} neues Beschriftung hinzugefügt.", newTagsCount);
            string info2 = String.Format("Es wurden {0} neue Beschriftungen hinzugefügt.", newTagsCount);
            if(newTagsCount == 1)
            {
                TaskDialog.Show("Info", info1);
            }
            else
            {
                TaskDialog.Show("Info", info2);
            }
        }
    }
}
