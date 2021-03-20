using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExStorage
{
    public class OpeningMemory
    {
        public string NewPosition { get; set; }
        public string NewDimensions { get; set; }
        public string NewDateSaved { get; set; }
        public string OldPosition { get; set; }
        public string OldDimensions { get; set; }
        public string OldDateSaved { get; set; }
        public GtbSchema GtbSchema { get; set; }
        public bool IsDimChanged { get; set; }
        public bool IsPosChanged { get; set; }
        public bool IsNew { get; set; } = false;

        Schema _schema;
        public FamilyInstance _familyInstance;

        private OpeningMemory()
        {

        }

        public static OpeningMemory Initialize(FamilyInstance familyInstance)
        {
            OpeningMemory result = new OpeningMemory();
            result._familyInstance = familyInstance;
            result.ReadCurrentSettings();
            result.ReadExternalStorage();
            result.CompareData();
            return result;
        }

        private void CompareData()
        {
            if (OldDateSaved == "-1" || OldDateSaved == "")
            {
                IsNew = true;
                return;
            }
            if(OldPosition == NewPosition)
            {
                IsPosChanged = false;
            }
            else
            {
                IsPosChanged = true;
            }
            if (OldDimensions == NewDimensions)
            {
                IsDimChanged = false;
            }
            else
            {
                IsDimChanged = true;
            }
        }

        //public void SavePositionTostorage()
        //{
        //    GtbSchema.SetEntityField(_familyInstance, "positionXYZ", NewPosition);
        //}

        //public void SaveDimensionsTostorage()
        //{
        //    GtbSchema.SetEntityField(_familyInstance, "dimensions", NewDimensions);
        //}

        //public void SaveDateTostorage()
        //{
        //    GtbSchema.SetEntityField(_familyInstance, "dateSaved", NewDateSaved);
        //}

        public void SaveDataToStorage()
        {
            string jsonString = OpeningMemoryString.CreateJsonString(NewPosition, NewDimensions, NewDateSaved);
            GtbSchema.SetEntityField(_familyInstance, "openingMemory", jsonString);
        }

        public void UpdateCurrentSettings()
        {
            ReadCurrentSettings();
        }

        public void UpdateFull()
        {
            ReadCurrentSettings();
            ReadExternalStorage();
            CompareData();
        }

        private void ReadCurrentSettings()
        {
            LocationPoint locationPoint = _familyInstance.Location as LocationPoint;
            NewPosition = locationPoint.Point.X.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) 
                            + ";" + locationPoint.Point.Y.ToString("F4", System.Globalization.CultureInfo.InvariantCulture) 
                                + ";" + locationPoint.Point.Z.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
            Parameter depth = _familyInstance.get_Parameter(new Guid("17a96ef5-1311-49f2-a0d1-4fe5f3f3854b"));
            Parameter diameter = _familyInstance.get_Parameter(new Guid("9c805bcc-ebc9-4d4c-8d73-26970789417a"));
            Parameter width = _familyInstance.get_Parameter(new Guid("46982e85-76c3-43fb-828f-ddf7a643566f"));
            Parameter height = _familyInstance.get_Parameter(new Guid("8eb274b3-fc0c-43e0-a46b-236bf59f292d"));

            Parameter offSet = _familyInstance.get_Parameter(new Guid("12f574e0-19fb-46bd-9b7e-0f329356db8a"));
            Parameter pipeDiameter = _familyInstance.LookupParameter("D");
            if (depth != null && width != null && height != null)
            {
                double widthMetric = width.AsDouble() * 304.8;
                double heightMetric = height.AsDouble() * 304.8;
                double depthMetric = depth.AsDouble() * 304.8;
                double cutOffsetMetric = offSet.AsDouble() * 304.8;
                NewDimensions = widthMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) 
                                    + "x" + heightMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) 
                                        + "x" + depthMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
                                            + "x" + cutOffsetMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            }
            if (depth != null && diameter != null)
            {
                double depthMetric = depth.AsDouble() * 304.8;
                double diameterMetric = diameter.AsDouble() * 304.8;
                double pipeDiameterMetric = pipeDiameter.AsDouble() * 304.8;
                double cutOffsetMetric = offSet.AsDouble() * 304.8;
                NewDimensions = diameterMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) 
                                    + "x" + depthMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
                                        +"x" + pipeDiameterMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
                                            + "x" + cutOffsetMetric.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            }
            NewDateSaved = DateTime.Now.ToString("dd-MM-yyyy");
        }

        private void ReadExternalStorage()
        {
            GetSchema();
            Entity retrievedEntity = _familyInstance.GetEntity(_schema);
            if(retrievedEntity.IsValid())
            {
                string jsonString = retrievedEntity.Get<string>(_schema.GetField("openingMemory"));
                OpeningMemoryString openingMemoryString = OpeningMemoryString.ReadJsonString(jsonString);
                OldPosition = openingMemoryString.Position;
                OldDimensions = openingMemoryString.Dimensions;
                OldDateSaved = openingMemoryString.Date;
            }
            else
            {
                OldPosition = "";
                OldDimensions = "";
                OldDateSaved = "";
            }
        }

        private void GetSchema()
        {
            GtbSchema = new GtbSchema();
            GtbSchema.SetGtbSchema();
            _schema = GtbSchema.Schema;
        }
    }
}
