using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningSymbol
{
    public class ControlledSymbol
    {
        public int ID { get; set; }
        public RectangularOpening RectangularOpening { get; set; }
        public RoundOpening RoundOpening { get; set; }
        public ControlledSymbolType ControlledSymbolType {get; set;}
        public bool IsValid { get; set; }

        private ControlledSymbol()
        {

        }

        #region overrides to use class as dictionary key
        public override int GetHashCode()
        {
            return ID;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as ControlledSymbol);
        }
        public bool Equals(ControlledSymbol obj)
        {
            return obj != null && obj.ID == this.ID;
        }
        #endregion

        public static ControlledSymbol Initialize(RectangularOpening rectangularOpening)
        {
            ControlledSymbol result = new ControlledSymbol();
            result.ControlledSymbolType = ControlledSymbolType.Rectangular;
            result.RectangularOpening = rectangularOpening;
            result.SetID();
            result.Validate();
            return result;
        }

        public static ControlledSymbol Initialize(RoundOpening roundOpening)
        {
            ControlledSymbol result = new ControlledSymbol();
            result.ControlledSymbolType = ControlledSymbolType.Round;
            result.RoundOpening = roundOpening;
            result.SetID();
            result.Validate();
            return result;
        }

        private void Validate()
        {
            if (ControlledSymbolType == ControlledSymbolType.Rectangular)
            {
                if(RectangularOpening.SymbolVisibility == SymbolVisibility.None)
                {
                    IsValid = false;
                }
                else
                {
                    IsValid = true;
                }
            }
            if (ControlledSymbolType == ControlledSymbolType.Round)
            {
                if (RoundOpening.SymbolVisibility == SymbolVisibility.None)
                {
                    IsValid = false;
                }
                else
                {
                    IsValid = true;
                }
            }
        }

        private void SetID()
        {
            if(ControlledSymbolType == ControlledSymbolType.Rectangular)
            {
                ID = RectangularOpening.FamilyInstance.Id.IntegerValue + (int)RectangularOpening.SymbolVisibility;
            }
            if (ControlledSymbolType == ControlledSymbolType.Round)
            {
                ID = RoundOpening.FamilyInstance.Id.IntegerValue + (int)RoundOpening.SymbolVisibility;
            }
        }
    }
}
