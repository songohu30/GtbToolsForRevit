using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excel
{
    public static class Report
    {
        public static string Content { get; set; } = "";

        public static void AddToContent(string text)
        {
            Content += text + Environment.NewLine;
        }
        public static void SaveReport()
        {

        }
    }
}
