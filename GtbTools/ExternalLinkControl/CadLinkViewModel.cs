using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalLinkControl
{
    public class CadLinkViewModel
    {
        public List<CadViewModel> CadViewModels { get; set; }
        public CADLinkType CadLinkType { get; set; }

        List<View> _views;
        List<View> _viewTemplates;

        public CadLinkViewModel(CADLinkType cadLinkType, List<View> views)
        {
            CadLinkType = cadLinkType;
            _views = views;
        }

        public void CreateViewModels()
        {
            CadViewModels = new List<CadViewModel>();
            _viewTemplates = _views.Where(e => e.IsTemplate).ToList();
            foreach (View v in _views)
            {
                CadViewModel model = CadViewModel.Initialize(v, CadLinkType, _viewTemplates);
                if (model.CategoryExistsInView)
                {
                    CadViewModels.Add(model);
                }
            }
        }

        public void UpdateModel()
        {
            foreach (CadViewModel model in CadViewModels)
            {
                model.UpdateVisibility();
                model.UpdatePreselectedTemplate();
                model.UpdateControlledBy();
            }
        }
    }
}
