using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalLinkControl
{
    public class RevitLinkViewModel
    {
        public List<RevitViewModel> RevitViewModels { get; set; }
        public RevitLinkType RevitLinkType { get; set; }

        List<View> _views;
        List<View> _viewTemplates;
        
        public RevitLinkViewModel(RevitLinkType revitLinkType, List<View> views)
        {
            RevitLinkType = revitLinkType;
            _views = views;
        }

        public void CreateViewModels()
        {
            RevitViewModels = new List<RevitViewModel>();
            _viewTemplates = _views.Where(e => e.IsTemplate).OrderBy(e => e.Name).ToList();
            foreach (View v in _views)
            {
                RevitViewModel model = RevitViewModel.Initialize(v, RevitLinkType, _viewTemplates);
                RevitViewModels.Add(model);
            }
        }

        public void UpdateModel()
        {
            foreach (RevitViewModel model in RevitViewModels)
            {
                model.UpdateVisibility();
                model.UpdatePreselectedTemplate();
                model.UpdateControlledBy();
            }
        }
    }
}
