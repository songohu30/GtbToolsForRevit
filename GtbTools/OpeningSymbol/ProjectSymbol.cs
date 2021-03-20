using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpeningSymbol
{
    public class ProjectSymbol
    {
        public FamilyInstance OpeningInstance { get; set; }
        public List<View> TopSymbol { get; set; }
        public List<View> FrontBackSymbol { get; set; }
        public List<View> LeftRightSymbol { get; set; }
        public bool OnMultipleViews { get; set; } = false;

        public ProjectSymbol()
        {

        }

        public void InitiateLists()
        {
            TopSymbol = new List<View>();
            FrontBackSymbol = new List<View>();
            LeftRightSymbol = new List<View>();
        }

        public void AddViewToList(RectangularOpening rectangularOpening, View view)
        {
            if(rectangularOpening.SymbolVisibility == SymbolVisibility.TopSymbol)
            {
                TopSymbol.Add(view);
                if (TopSymbol.Count > 1) OnMultipleViews = true;
            }
            if(rectangularOpening.SymbolVisibility == SymbolVisibility.FrontBackSymbol)
            {
                FrontBackSymbol.Add(view);
                if (FrontBackSymbol.Count > 1) OnMultipleViews = true;
            }
            if(rectangularOpening.SymbolVisibility == SymbolVisibility.RightLeftSymbol)
            {
                LeftRightSymbol.Add(view);
                if (LeftRightSymbol.Count > 1) OnMultipleViews = true;
            }
        }

        public string GetReport()
        {
            string result = "";
            if(TopSymbol.Count > 1)
            {
                result += "!!! Warning: ";
            }
            return result;
        }
    }
}
