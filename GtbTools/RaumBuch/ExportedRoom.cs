using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace RaumBuch
{
    public class ExportedRoom
    {
        public string MepRoomNumber { get; set; }
        public string MepSpaceNumber { get; set; }
        public Space MepRoom { get; set; }
        public Dictionary<string, List<FamilyInstance>> ExportItems { get; set; }
        public Dictionary<string, List<FamilyInstance>> SortedItems { get; set; }


        List<FamilyInstance> _allElements;
        List<FamilyInstance> _spaceElements;



        public ExportedRoom(Space mepRoom, List<FamilyInstance> allElements)
        {
            _allElements = allElements;
            MepRoom = mepRoom;
            MepRoomNumber = MepRoom.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NUMBER).AsString();
            MepSpaceNumber = MepRoom.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString();
        }

        public void SortContainingElements(Dictionary<string, List<int>> excelDataModel)
        {
            ExportItems = new Dictionary<string, List<FamilyInstance>>();
            GetSpaceElements();

            foreach (KeyValuePair<string, List<int>> pair in excelDataModel)
            {
                List<FamilyInstance> list = _spaceElements.Where(e => pair.Value.Contains(e.Symbol.Id.IntegerValue)).ToList();
                ExportItems.Add(pair.Key, list);
            }
            SortElements();
        }

        private void GetSpaceElements()
        {
            _spaceElements = new List<FamilyInstance>();
            foreach (FamilyInstance fi in _allElements)
            {
                LocationPoint lp = fi.Location as LocationPoint;
                if(lp != null)
                {
                    XYZ origin = lp.Point;
                    if (MepRoom.IsPointInSpace(origin)) _spaceElements.Add(fi);
                }
            }
            foreach (FamilyInstance fi in _spaceElements)
            {
                _allElements.Remove(fi);
            }
        }

        private void SortElements()
        {
            SortedItems = new Dictionary<string, List<FamilyInstance>>();
            foreach (FamilyInstance fi in _spaceElements)
            {
                if(SortedItems.ContainsKey(fi.Category.Name))
                {
                    SortedItems[fi.Category.Name].Add(fi);
                }
                else
                {
                    List<FamilyInstance> list = new List<FamilyInstance>() { fi };
                    SortedItems.Add(fi.Category.Name, list);
                }
            }
        }
    }
}
