using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ExternalLinkControl
{
    public class RevitViewModel : INotifyPropertyChanged
    {
        public ElementId ViewId { get; set; }
        public View View { get; set; }
        public RevitLinkType RevitLinkType { get; set; }
        public List<View> ViewTemplates { get; set; }
        public View PreSelectedTemplate { get; set; }

        private string _controlledBy;
        public string ControlledBy
        {
            get { return _controlledBy; }
            set
            {
                if (_controlledBy != value)
                {
                    _controlledBy = value;
                    OnPropertyChanged(nameof(ControlledBy));
                }
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }
        public string ViewType { get; set; }
        public bool IsTemplate { get; set; }
        public System.Windows.Visibility ComboBoxVisibility { get; set; } = System.Windows.Visibility.Visible;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private RevitViewModel()
        {

        }

        public static RevitViewModel Initialize(View view, RevitLinkType revitLinkType, List<View> viewTemplates)
        {
            RevitViewModel result = new RevitViewModel();
            result.ViewTemplates = viewTemplates;
            result.View = view;
            result.ViewId = view.Id;
            result.RevitLinkType = revitLinkType;
            result.CheckVisibility();
            result.SetViewType();
            result.SetPreSelectedTemplate();
            result.IsTemplate = result.View.IsTemplate;
            if (result.IsTemplate)
            {
                result.ComboBoxVisibility = System.Windows.Visibility.Hidden;
            }
            result.SetControlledBy();
            return result;
        }

        public void UpdateVisibility()
        {
            CheckVisibility();
        }

        public void UpdatePreselectedTemplate()
        {
            SetPreSelectedTemplate();
        }
        public void UpdateControlledBy()
        {
            SetControlledBy();
        }

        public void TurnVisibilityOn(Document document)
        {
            if (!View.IsTemplate && IsRvtControlledByTemplate(document)) return;
            using(Transaction tx = new Transaction(document, RevitLinkType.Name + " unhidden on " + View.Name))
            {
                tx.Start();
                View.UnhideElements(new List<ElementId>() { RevitLinkType.Id });
                tx.Commit();
            }
        }

        public void TurnVisibilityOff(Document document)
        {
            if (!View.IsTemplate && IsRvtControlledByTemplate(document)) return;
            using (Transaction tx = new Transaction(document, RevitLinkType.Name + " hidden on " + View.Name))
            {
                tx.Start();
                View.HideElements(new List<ElementId>() { RevitLinkType.Id });
                tx.Commit();
            }
        }

        public void ChangeViewTemplate(Document document)
        {
            ElementId currentTemplateId = View.ViewTemplateId;
            ElementId newTemplateId = ElementId.InvalidElementId;
            if (SelectedIndex >= 0) newTemplateId = ViewTemplates[SelectedIndex].Id;

            if (currentTemplateId != newTemplateId)
            {
                using (Transaction tx = new Transaction(document, "Changing template on " + View.Name))
                {
                    tx.Start();
                    View.ViewTemplateId = newTemplateId;
                    tx.Commit();
                }
            }
        }

        private void SetControlledBy()
        {
            if(IsTemplate)
            {
                ControlledBy = "Ansichtvorlage";
            }
            else
            {
                if(PreSelectedTemplate == null || PreSelectedTemplate.Id == ElementId.InvalidElementId)
                {
                    ControlledBy = "Ansicht";
                }
                else
                {
                    List<int> nonControlledParameters = PreSelectedTemplate.GetNonControlledTemplateParameterIds().Select(e => e.IntegerValue).ToList();
                    if (nonControlledParameters.Contains((int)BuiltInParameter.VIS_GRAPHICS_RVT_LINKS))
                    {
                        ControlledBy = "Ansicht";
                    }
                    else
                    {
                        ControlledBy = "Vorlage";
                    }
                }
            }
        }

        private void SetPreSelectedTemplate()
        {
            PreSelectedTemplate = ViewTemplates.Where(e => e.Id.IntegerValue == View.ViewTemplateId.IntegerValue).FirstOrDefault();
            SelectedIndex = ViewTemplates.FindIndex(e => e.Id.IntegerValue == View.ViewTemplateId.IntegerValue);
        }

        private bool IsRvtControlledByTemplate(Document doc)
        {
            bool result = false;
            ElementId viewTemplateId = View.ViewTemplateId;
            if (viewTemplateId != null && viewTemplateId != ElementId.InvalidElementId)
            {
                View viewTemplate = doc.GetElement(viewTemplateId) as View;
                List<int> nonControlledParameters = viewTemplate.GetNonControlledTemplateParameterIds().Select(e => e.IntegerValue).ToList();
                if (!nonControlledParameters.Contains((int)BuiltInParameter.VIS_GRAPHICS_RVT_LINKS))
                {
                    result = true;
                }
            }
            return result;
        }

        private void CheckVisibility()
        {
            IsVisible = !RevitLinkType.IsHidden(View);
        }

        private void SetViewType()
        {
            ViewType = Enum.GetName(typeof(ViewType), View.ViewType);
        }
    }
}
