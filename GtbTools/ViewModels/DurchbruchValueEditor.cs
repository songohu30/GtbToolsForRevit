using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public static class DurchbruchValueEditor
    {
        public static bool ConvertToDouble(string text, out double number)
        {
            bool result = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number);

            if (!result)
            {
                TaskDialog.Show("Warning!", "Der eingefügte Wert muss eine Zahl sein!");
            }
            else
            {
                if (number < 0)
                {
                    TaskDialog.Show("Warning!", "Wert kann nicht kleiner als null sein!");
                }
            }
            return result;
        }
    }
}
