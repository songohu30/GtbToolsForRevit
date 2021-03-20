using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipesInWall
{
    public class WallFamily
    {
        public string FamilyName { get; set; }
        public List<WallType> WallTypes { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
