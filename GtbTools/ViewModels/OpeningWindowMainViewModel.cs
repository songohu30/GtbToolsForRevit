using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GtbTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class OpeningWindowMainViewModel
    {
        public UIDocument UIDocument { get; set; }
        public Document Document { get; set; }
        public List<ModelView> SectionViews { get; set; }
        public List<ModelView> PlanViews { get; set; }
        public OpeningSymbol.ViewDiscipline ViewDiscipline { get; set; }

        List<View> _views;

        private OpeningWindowMainViewModel()
        {

        }

        public static OpeningWindowMainViewModel Initialize(UIDocument uidoc)
        {
            OpeningWindowMainViewModel result = new OpeningWindowMainViewModel();
            result.UIDocument = uidoc;
            result.Document = uidoc.Document;
            result.GetAllViews();
            result.CreatePlanViewList();
            result.CreateSectionViewList();
            return result;
        }

        private void GetAllViews()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            _views = ficol.OfClass(typeof(View)).Select(x => x as View).ToList();
        }

        private void CreateSectionViewList()
        {
            SectionViews = new List<ModelView>();
            foreach (View v in _views)
            {
                if(v.ViewType == ViewType.Section)
                {
                    ModelView modelView = new ModelView();
                    modelView.IsSelected = false;
                    modelView.IsSectionView = true;
                    modelView.Name = v.Name;
                    modelView.View = v;
                    SectionViews.Add(modelView);
                }               
            }
        }

        private void CreatePlanViewList()
        {
            PlanViews = new List<ModelView>();
            foreach (View v in _views)
            {
                if (v.IsTemplate) continue;
                if (v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.CeilingPlan || v.ViewType == ViewType.EngineeringPlan || v.ViewType == ViewType.AreaPlan)
                {
                    ModelView modelView = new ModelView();
                    modelView.IsSelected = false;
                    modelView.IsSectionView = false;
                    modelView.Name = v.Name;
                    modelView.View = v;
                    PlanViews.Add(modelView);
                }
            }
        }

        public void SelectAllPlanViews()
        {
            foreach (ModelView mv in PlanViews)
            {
                mv.IsSelected = true;
            }
        }

        public void ClearAllPlanViews()
        {
            foreach (ModelView mv in PlanViews)
            {
                mv.IsSelected = false;
            }
        }

        public void SelectAllSectionViews()
        {
            foreach (ModelView mv in SectionViews)
            {
                mv.IsSelected = true;
            }
        }

        public void ClearAllSectionViews()
        {
            foreach (ModelView mv in SectionViews)
            {
                mv.IsSelected = false;
            }
        }
    }
}
