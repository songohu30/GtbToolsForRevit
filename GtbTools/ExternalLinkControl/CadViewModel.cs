using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ExternalLinkControl
{
    public class CadViewModel : INotifyPropertyChanged
    {
        public ElementId ViewId { get; set; }
        public View View { get; set; }
        public CADLinkType CadLinkType { get; set; }
        public List<View> ViewTemplates { get; set; }
        public View PreSelectedTemplate { get; set; }

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

        public bool CategoryExistsInView = true;

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

        private CadViewModel()
        {

        }

        public static CadViewModel Initialize(View view, CADLinkType cadLinkType, List<View> viewTemplates)
        {
            CadViewModel result = new CadViewModel();
            result.ViewTemplates = viewTemplates;
            result.View = view;
            result.ViewId = view.Id;
            result.CadLinkType = cadLinkType;
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
            if (!View.IsTemplate && IsCadControlledByTemplate(document)) return;
            using (Transaction tx = new Transaction(document, CadLinkType.Name + " unhidden on " + View.Name))
            {
                tx.Start();
                View.SetCategoryHidden(CadLinkType.Category.Id, false);
                tx.Commit();
            }
        }

        public void TurnVisibilityOff(Document document)
        {
            if (!View.IsTemplate && IsCadControlledByTemplate(document)) return;
            using (Transaction tx = new Transaction(document, CadLinkType.Name + " hidden on " + View.Name))
            {
                tx.Start();
                View.SetCategoryHidden(CadLinkType.Category.Id, true);
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
            if (IsTemplate)
            {
                ControlledBy = "Ansichtvorlage";
            }
            else
            {
                if (PreSelectedTemplate == null || PreSelectedTemplate.Id == ElementId.InvalidElementId)
                {
                    ControlledBy = "Ansicht";
                }
                else
                {
                    List<int> nonControlledParameters = PreSelectedTemplate.GetNonControlledTemplateParameterIds().Select(e => e.IntegerValue).ToList();
                    if (nonControlledParameters.Contains((int)BuiltInParameter.VIS_GRAPHICS_IMPORT))
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

        private bool IsCadControlledByTemplate(Document doc)
        {
            bool result = false;
            ElementId viewTemplateId = View.ViewTemplateId;
            if (viewTemplateId != null && viewTemplateId != ElementId.InvalidElementId)
            {
                View viewTemplate = doc.GetElement(viewTemplateId) as View;
                List<int> nonControlledParameters = viewTemplate.GetNonControlledTemplateParameterIds().Select(e => e.IntegerValue).ToList();
                if (!nonControlledParameters.Contains((int)BuiltInParameter.VIS_GRAPHICS_IMPORT))
                {
                    result = true;
                }
            }
            return result;
        }

        private void CheckVisibility()
        {
            try
            {
                IsVisible = CadLinkType.Category.get_Visible(View);
            }
            catch
            {
                CategoryExistsInView = false;
                //TaskDialog.Show("Error!", View.Name + "- Can't identify visibility of: " + CadLinkType.Name);               
            }
        }

        private void SetViewType()
        {
            ViewType = Enum.GetName(typeof(ViewType), View.ViewType);
        }
    }
}
