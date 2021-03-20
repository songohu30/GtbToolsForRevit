using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalLinkControl
{
    public class ExternalLinkToolViewModel
    {
        public List<RevitLinkViewModel> RevitLinkViewModels { get; set; }
        public List<CadLinkViewModel> CadLinkViewModels { get; set; }
        public RevitLinkViewModel EditedRvtLinkViewModel { get; set; }
        public CadLinkViewModel EditedCadLinkViewModel { get; set; }

        Document _document;
        
        public ExternalLinkToolViewModel(Document document)
        {
            _document = document;
        }

        public void CreateViewModels()
        {
            RevitLinkViewModels = new List<RevitLinkViewModel>();
            CadLinkViewModels = new List<CadLinkViewModel>();
            List<View> views = GetViews();
            //TemplateControlsTheView(views);
            
            foreach (RevitLinkType rvtLinkType in GetRevitLinkTypes())
            {
                ExternalFileReference fileReference = rvtLinkType.GetExternalFileReference();
                if (fileReference.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
                {
                    RevitLinkViewModel model = new RevitLinkViewModel(rvtLinkType, views);
                    model.CreateViewModels();
                    RevitLinkViewModels.Add(model);
                }
            }

            foreach (CADLinkType cadLinkType in GetCadLinkTypes())
            {
                
                ExternalFileReference fileReference = cadLinkType.GetExternalFileReference();
                if(fileReference.GetLinkedFileStatus() == LinkedFileStatus.Loaded)
                {
                    ElementId ownerViewId = null;
                    if (IsOneViewOnly(cadLinkType, out ownerViewId))
                    {
                        View oneView = _document.GetElement(ownerViewId) as View;
                        List<View> templatesPlusOne = views.Where(e => e.IsTemplate).ToList();
                        templatesPlusOne.Add(oneView);
                        CadLinkViewModel model = new CadLinkViewModel(cadLinkType, templatesPlusOne);
                        model.CreateViewModels();
                        CadLinkViewModels.Add(model);
                    }
                    else
                    {
                        CadLinkViewModel model = new CadLinkViewModel(cadLinkType, views);
                        model.CreateViewModels();
                        CadLinkViewModels.Add(model);
                    }
                }
            }
        }

        public void ApplyChangesInRvtLinks()
        {
            foreach (RevitViewModel model in EditedRvtLinkViewModel.RevitViewModels)
            {
                if (model.IsVisible) model.TurnVisibilityOn(_document);
                if (!model.IsVisible) model.TurnVisibilityOff(_document);
                if(!model.IsTemplate)
                {
                    model.ChangeViewTemplate(_document);
                }
            }
        }

        public void ApplyChangesInCadLinks()
        {
            foreach (CadViewModel model in EditedCadLinkViewModel.CadViewModels)
            {
                if (model.IsVisible) model.TurnVisibilityOn(_document);
                if (!model.IsVisible) model.TurnVisibilityOff(_document);
                if (!model.IsTemplate)
                {
                    model.ChangeViewTemplate(_document);
                }
            }
        }

        private bool IsOneViewOnly(CADLinkType cadLinkType, out ElementId ownerViewId)
        {
            ownerViewId = null;
            bool result = false;
            FilteredElementCollector ficol = new FilteredElementCollector(_document);
            ImportInstance instance = ficol.OfClass(typeof(ImportInstance)).Select(e => e as ImportInstance).Where(e => e.GetTypeId().IntegerValue == cadLinkType.Id.IntegerValue).FirstOrDefault();
            if(instance != null)
            {
                if(instance.OwnerViewId != null && instance.OwnerViewId != ElementId.InvalidElementId)
                {
                    result = true;
                    ownerViewId = instance.OwnerViewId;
                }
            }
            return result;
        }

        private List<RevitLinkType> GetRevitLinkTypes()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_document);
            return ficol.OfClass(typeof(RevitLinkType)).Select(e => e as RevitLinkType).ToList();
        }

        private List<CADLinkType> GetCadLinkTypes()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_document);
            return ficol.OfClass(typeof(CADLinkType)).Select(e => e as CADLinkType).ToList();
        }

        private List<View> GetViews()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_document);
            return ficol.OfClass(typeof(View)).Select(e => e as View).Where(e => IsPrimaryView(e) && e.ViewType == ViewType.FloorPlan || e.ViewType == ViewType.CeilingPlan).ToList();
        }

        private bool IsPrimaryView(View view)
        {
            bool result = false;
            ElementId test = view.GetPrimaryViewId();
            if (test == null || test == ElementId.InvalidElementId) result = true;
            return result;
        }

        private void TemplateControlsTheView(List<View> views)
        {
            string warning = "";
            foreach (View v in views)
            {
                if(v.IsTemplate)
                {
                    List<int> nonControlledParameterIds = v.GetNonControlledTemplateParameterIds().Select(e => e.IntegerValue).ToList();
                    int cad = (int)BuiltInParameter.VIS_GRAPHICS_IMPORT;
                    int rvt = (int)BuiltInParameter.VIS_GRAPHICS_RVT_LINKS;
                    if(!nonControlledParameterIds.Contains(cad))
                    {
                        warning += "Cad imports are controlled by view template: " + v.Name + Environment.NewLine; 
                    }
                    if (!nonControlledParameterIds.Contains(rvt))
                    {
                        warning += "Revit links are controlled by view template: " + v.Name + Environment.NewLine;
                    }
                }
            }
            if (warning != "") TaskDialog.Show("Warning!!!", warning);
        }
    }
}
