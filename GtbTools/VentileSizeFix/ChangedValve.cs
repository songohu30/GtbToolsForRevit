using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VentileSizeFix
{
    public class ChangedValve
    {
        public ElementId ElementId { get; set; }
        public FamilyInstance FamilyInstance { get; set; }
        public Family Family { get; set; }
        public FamilySymbol CurrentSymbol { get; set; }
        public FamilySymbol NewSymbol { get; set; }

        List<ElementId> _familySymbols;
        Document _doc;

        private ChangedValve()
        {

        }

        public static ChangedValve Initialize(FamilyInstance familyInstance, Document doc)
        {
            ChangedValve result = new ChangedValve();
            result.FamilyInstance = familyInstance;
            result._doc = doc;
            result.SetProperties();
            result.SetFields();
            result.SetNewSymbol();
            return result;
        }

        public void ChangeType()
        {
            if(NewSymbol != null) FamilyInstance.Symbol = NewSymbol;
        }

        private void SetProperties()
        {
            ElementId = FamilyInstance.Id;
            Family = FamilyInstance.Symbol.Family;
            CurrentSymbol = FamilyInstance.Symbol;
        }

        private void SetFields()
        {
            _familySymbols = Family.GetFamilySymbolIds().ToList();  
        }

        private void SetNewSymbol()
        {
            MEPModel mepModel = FamilyInstance.MEPModel;
            ConnectorSet csValve = mepModel.ConnectorManager.Connectors;
            FamilyInstance transition = null;
            double valveConnector = 0;
            foreach (Connector ca in csValve)
            {
                valveConnector = ca.Radius;
                foreach (Connector cb in ca.AllRefs)
                {
                    Element element = cb.Owner;
                    if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        transition = cb.Owner as FamilyInstance;
                    }
                }
            }
            ConnectorSet csTransition = (transition.MEPModel as MechanicalFitting).ConnectorManager.Connectors;
            double searchedRadius = 0;
            foreach (Connector c in csTransition)
            {
                if (c.Radius != valveConnector) searchedRadius = c.Radius;
            }
            string check = (searchedRadius * 2 * 304.8).ToString("F0");
            ElementId searchedId = null;
            foreach (ElementId id in _familySymbols)
            {
                FamilySymbol fs = _doc.GetElement(id) as FamilySymbol;
                if (fs.Name.Contains(check)) searchedId = id;
            }
            if (searchedId != null) NewSymbol = _doc.GetElement(searchedId) as FamilySymbol;
        }
    }
}
