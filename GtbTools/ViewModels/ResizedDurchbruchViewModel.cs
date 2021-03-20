using Autodesk.Revit.DB;
using GtbTools;
using Model;
using System.Collections.Generic;
using System.ComponentModel;

namespace ViewModels
{
    public class ResizedDurchbruchViewModel : INotifyPropertyChanged
    {
        public string ElementId { get; set; }
        public string Shape { get; set; }
        public string _diameter;
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
        public string _pipeDiameter;
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
        public string _offset;
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
        public string _width;
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
        public string _height;
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
        public string _depth;
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
        public string _openingMark;
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
        public string _systemType;
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
        public string _fireRating;
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


        public string OldDiameter { get; set; }
        public string OldWidth { get; set; }
        public string OldHeight { get; set; }
        public string OldDepth { get; set; }
        public string DateSaved { get; set; }

        public string OldPipeDiameter { get; set; }
        public string OldOffset { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ResizedDurchbruchViewModel()
        {

        }

        public static ResizedDurchbruchViewModel Initialize(DurchbruchModel durchbruchModel)
        {
            ResizedDurchbruchViewModel result = new ResizedDurchbruchViewModel();
            result.DurchbruchModel = durchbruchModel;
            result.SetElementId();
            result.SetShape();
            result.SetDimensions();
            result.SetMark();
            result.SetSystemTypeAndFireRating();
            result.SetViews();
            result.GetOldDimensions();
            result.SetDateSaved();
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

        private void SetDateSaved()
        {
            DateSaved = DurchbruchModel.OpeningMemory.OldDateSaved;
        }

        private void SetElementId()
        {
            ElementId = DurchbruchModel.ElementId.IntegerValue.ToString();
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
            }
            else
            {
                SystemType = "#NA";
            }
            if (DurchbruchModel.FireRating != null)
            {
                FireRating = DurchbruchModel.FireRating.AsString();
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

        private void GetOldDimensions()
        {
            if(DurchbruchModel.Shape == DurchbruchShape.Rectangular)
            {
                string[] dims = DurchbruchModel.OpeningMemory.OldDimensions.Split('x');
                OldWidth = dims[0];
                OldHeight = dims[1];
                OldDepth = dims[2];
                OldOffset = dims[3];
                OldDiameter = "---";
            }
            if (DurchbruchModel.Shape == DurchbruchShape.Round)
            {
                string[] dims = DurchbruchModel.OpeningMemory.OldDimensions.Split('x');
                OldDiameter = dims[0];
                OldDepth = dims[1];
                OldPipeDiameter = dims[2];
                OldOffset = dims[3];
                OldWidth = "---";
                OldHeight = "---";
            }
        }
}
}
