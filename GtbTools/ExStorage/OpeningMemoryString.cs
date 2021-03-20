using Autodesk.Revit.DB.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExStorage
{
    public class OpeningMemoryString
    {
        public string Position {get; set;}
        public string Dimensions { get; set; }
        public string Date { get; set; }

        private OpeningMemoryString()
        {

        }

        public static OpeningMemoryString ReadJsonString(string jsonString)
        {
            OpeningMemoryString result = JsonConvert.DeserializeObject<OpeningMemoryString>(jsonString);
            return result;
        }

        public static string CreateJsonString(string position, string dimensions, string date)
        {
            OpeningMemoryString openingMemoryString = new OpeningMemoryString() { Position = position, Dimensions = dimensions, Date = date };
            string result = JsonConvert.SerializeObject(openingMemoryString);
            return result;
        }
    }
}
