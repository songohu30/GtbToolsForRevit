using Autodesk.Revit.DB;
using GtbTools;
using Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ViewModels
{
    public class NewDurchbruchViewModel : INotifyPropertyChanged
    {
        public string ElementId { get; set; }
        public string Shape { get; set; }
        private string _diameter;
        public string Diameter //Total durchbruch diameter
        {
            get => _diameter;
            set
            {
                if (_diameter != value)
                {
                    _diameter = value;
                    OnPropertyChanged(nameof(Diameter));
                }
            }
        }
        private string _pipeDiameter;
        public string PipeDiameter
        {
            get => _pipeDiameter;
            set
            {
                if (_pipeDiameter != value)
                {
                    _pipeDiameter = value;
                    OnPropertyChanged(nameof(PipeDiameter));
                }
            }
        } //editable in datagrid
        private string _offset;
        public string Offset //editable in datagrid
        {
            get => _offset;
            set
            {
                if (_offset != value)
                {
                    _offset = value;
                    OnPropertyChanged(nameof(Offset));
                }
            }
        }
        private string _width;
        public string Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }
        private string _height;
        public string Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }
        private string _depth;
        public string Depth
        {
            get => _depth;
            set
            {
                if (_depth != value)
                {
                    _depth = value;
                    OnPropertyChanged(nameof(Depth));
                }
            }
        }
        public List<ModelView> Views { get; set; }
        private string _openingMark;
        public string OpeningMark
        {
            get => _openingMark;
            set
            {
                if (_openingMark != value)
                {
                    _openingMark = value;
                    OnPropertyChanged(nameof(OpeningMark));
                }
            }
        }
        private string _systemType;
        public string SystemType
        {
            get => _systemType;
            set
            {
                if (_systemType != value)
                {
                    _systemType = value;
                    OnPropertyChanged(nameof(SystemType));
                }
            }
        }
        private string _fireRating;
        public string FireRating
        {
            get => _fireRating;
            set
            {
                if (_fireRating != value)
                {
                    _fireRating = value;
                    OnPropertyChanged(nameof(FireRating));
                }
            }
        }
        public DurchbruchModel DurchbruchModel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private NewDurchbruchViewModel()
        {

        }

        public static NewDurchbruchViewModel Initialize(DurchbruchModel durchbruchModel)
        {
            NewDurchbruchViewModel result = new NewDurchbruchViewModel();
            result.DurchbruchModel = durchbruchModel;
            result.SetElementId();
            result.SetShape();
            result.SetDimensions();
            result.SetMark();
            result.SetSystemTypeAndFireRating();
            result.SetViews();
            return result;
        }

        public void UpdateDurchbruch()
        {
            DurchbruchModel.OpeningMemory.UpdateCurrentSettings();
            SetDimensions();
            SetMark();
            SetSystemTypeAndFireRating();
        }

        public void UpdateDurchbruchDeep()
        {
            DurchbruchModel.UpdateMemory();
            DurchbruchModel.UpdateStatus();
            SetDimensions();
            SetMark();
            SetSystemTypeAndFireRating();
        }

        private void SetElementId()
        {
            ElementId =  DurchbruchModel.ElementId.IntegerValue.ToString();
        }

        private void SetShape()
        {
            if (DurchbruchModel.Shape == DurchbruchShape.Rectangular) Shape = "Rectangular";
            if (DurchbruchModel.Shape == DurchbruchShape.Round) Shape = "Round";
        }

        private void SetDimensions()
        {
            //F1 + culture: One decimal place and culture for decimal separator
            double depthMetric = DurchbruchModel.Depth.AsDouble() * 304.8;
            double offsetMetric = DurchbruchModel.CutOffset.AsDouble() * 304.8;
            Offset = offsetMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            Depth = depthMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            if (DurchbruchModel.Shape == DurchbruchShape.Round)
            {
                Width = "---";
                Height = "---";
                double diameterMetric = DurchbruchModel.Diameter.AsDouble() * 304.8;
                Diameter = diameterMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                double pipeDiameterMetric = DurchbruchModel.PipeDiameter.AsDouble() * 304.8;
                PipeDiameter = pipeDiameterMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (DurchbruchModel.Shape == DurchbruchShape.Rectangular)
            {
                double widthMetric = DurchbruchModel.Width.AsDouble() * 304.8;
                double heightMetric = DurchbruchModel.Height.AsDouble() * 304.8;
                Width = widthMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                Height = heightMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                Diameter = "---";
                PipeDiameter = "---";
            }
        }

        private void SetMark()
        {
            OpeningMark = DurchbruchModel.OpeningMark.AsString();
        }

        private void SetSystemTypeAndFireRating()
        {
            if (DurchbruchModel.SystemType != null)
            {
                SystemType = DurchbruchModel.SystemType.AsString();
                if (SystemType == null) SystemType = "";
            }
            else
            {
                SystemType = "#NA";
            }
            if (DurchbruchModel.FireRating != null)
            {
                FireRating = DurchbruchModel.FireRating.AsString();
                if (FireRating == null) FireRating = "";
            }
            else
            {
                FireRating = "#NA";
            }
        }

        private void SetViews()
        {
            Views = new List<ModelView>();
            foreach (View view in DurchbruchModel.Views)
            {
                ModelView modelView = new ModelView
                {
                    Name = view.Name,
                    View = view,
                    IsSelected = true
                };
                Views.Add(modelView);
            }
        }
    }
}
