using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExStorage
{
    class SymbolToolString
    {
        public int Discipline { get; set; }
        public int TopSymbol { get; set; }
        public int LRSymbol { get; set; }
        public int FBSymbol { get; set; }
        public int ABSymbol { get; set; }
        public int ManSymbol { get; set; }

        public SymbolToolString()
        {

        }

        public static SymbolToolString ReadJsonString(string jsonString)
        {
            SymbolToolString result = JsonConvert.DeserializeObject<SymbolToolString>(jsonString);
            return result;
        }

        public string CreateJsonString(OpeningExStorage openingExStorage)
        {
            if (Discipline == 0) Discipline = openingExStorage._discipline;
            if (TopSymbol == 0) TopSymbol = openingExStorage._topSymbol;
            if (LRSymbol == 0) LRSymbol = openingExStorage._lRSymbol;
            if (FBSymbol == 0) FBSymbol = openingExStorage._fBSymbol;
            if (ABSymbol == 0) ABSymbol = openingExStorage._aBSymbol;
            if (ManSymbol == 0) ManSymbol = openingExStorage._manSymbol;

            string result = JsonConvert.SerializeObject(this);
            return result;
        }
    }
}
