using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using ExStorage;

namespace OpeningSymbol
{
    public class RoundOpening
    {
        public FamilyInstance FamilyInstance { get; set; }
        public SymbolVisibility SymbolVisibility { get; set; }
        public OpeningHost OpeningHost { get; set; }
        public bool DisciplineUserModified { get; set; }
        public OpeningExStorage OpeningExStorage { get; set; }

        ViewDirection _viewDirection;
        ViewDiscipline _viewDiscipline;
        bool _isCutByView;
        public PlanViewLocation PlanViewLocation { get; set; }
        XYZ _xyz;
        double _x;
        double _y;
        double _z;
        double _absoluteCutPlane;
        double _absoluteOpeningLevel;
        double _diameter;
        double _depth;

        private void ReadCompareExternalStorage()
        {
            OpeningExStorage = new OpeningExStorage(FamilyInstance);
            OpeningExStorage.ReadExternalStorage();
            OpeningExStorage.ReadCurrentSettings();
            OpeningExStorage.CompareData();
        }

        private RoundOpening()
        {

        }

        public static RoundOpening Initialize(FamilyInstance familyInstance, ViewDirection viewDirection, ViewDiscipline viewDiscipline, bool isCutByView)
        {
            RoundOpening result = new RoundOpening();
            result.FamilyInstance = familyInstance;
            result._viewDirection = viewDirection;
            result._viewDiscipline = viewDiscipline;
            result._isCutByView = isCutByView;
            result.SetInstanceXYZ();
            result.FindElementHost();
            result.SetSymbolVisibility();
            result.ReadCompareExternalStorage();
            return result;
        }

        public static RoundOpening Initialize(FamilyInstance familyInstance, ViewDirection viewDirection, ViewDiscipline viewDiscipline, double absoluteCutPlane)
        {
            RoundOpening result = new RoundOpening();
            result.FamilyInstance = familyInstance;
            result._viewDirection = viewDirection;
            result._viewDiscipline = viewDiscipline;
            result._absoluteCutPlane = absoluteCutPlane;
            result.SetInstanceXYZ();
            result.FindElementHost();
            result.SetOpeningDimensions();
            result.CheckCutPlane();
            result.SetSymbolVisibility();
            result.ReadCompareExternalStorage();
            return result;
        }

        public static RoundOpening Initialize(FamilyInstance familyInstance)
        {
            RoundOpening result = new RoundOpening();
            result.FamilyInstance = familyInstance;
            result.FindElementHost();
            return result;
        }

        private void SetOpeningDimensions()
        {
            Parameter parDiameter = FamilyInstance.LookupParameter("Diameter");
            Parameter parDepth = FamilyInstance.LookupParameter("Depth");
            _diameter = parDiameter.AsDouble() * 304.8;
            _depth = parDepth.AsDouble() * 304.8;
            LocationPoint lp = FamilyInstance.Location as LocationPoint;
            _absoluteOpeningLevel = lp.Point.Z * 304.8;
        }

        private void CheckCutPlane()
        {
            _isCutByView = false;
            if(OpeningHost == OpeningHost.Wall)
            {
                if(Math.Abs(_absoluteCutPlane - _absoluteOpeningLevel) < 0.5*_diameter)
                {
                    _isCutByView = true;
                }
            }
            if (OpeningHost == OpeningHost.FloorOrCeiling)
            {
                if (_absoluteCutPlane - _absoluteOpeningLevel < 0 && Math.Abs(_absoluteCutPlane - _absoluteOpeningLevel) < _depth)
                {
                    _isCutByView = true;
                    PlanViewLocation = PlanViewLocation.CutByPlane;
                }
                if(_absoluteOpeningLevel - _absoluteCutPlane > _depth)
                {
                    PlanViewLocation = PlanViewLocation.AboveCutPlane;
                }
                if(_absoluteCutPlane - _absoluteOpeningLevel > 0)
                {
                    PlanViewLocation = PlanViewLocation.BelowCutPlane;
                }
            }
        }

        private void SetInstanceXYZ()
        {
            _xyz = FamilyInstance.HandOrientation;
            _x = _xyz.X;
            _y = _xyz.Y;
            _z = _xyz.Z;
        }

        private void FindElementHost()
        {
            Element host = FamilyInstance.Host;
            if (host == null)
            {
                OpeningHost = OpeningHost.NotAssociated;
                return;
            }
            switch (host.Category.Id.IntegerValue)
            {
                case (int)BuiltInCategory.OST_Floors:
                    OpeningHost = OpeningHost.FloorOrCeiling;
                    break;
                case (int)BuiltInCategory.OST_Walls:
                    OpeningHost = OpeningHost.Wall;
                    break;
                case (int)BuiltInCategory.OST_Ceilings:
                    OpeningHost = OpeningHost.FloorOrCeiling;
                    break;
                case (int)BuiltInCategory.OST_Roofs:
                    OpeningHost = OpeningHost.Roof;
                    break;
                default:
                    OpeningHost = OpeningHost.Unknown;
                    break;
            }
        }

        private void SetSymbolVisibility()
        {
            if (_viewDirection == ViewDirection.SectionH)
            {
                if (OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    if (Math.Abs(_x) == 1) SymbolVisibility = SymbolVisibility.FrontBackSymbol;
                    if (Math.Abs(_y) == 1) SymbolVisibility = SymbolVisibility.RightLeftSymbol;
                }
                if (OpeningHost == OpeningHost.Wall)
                {
                    if (Math.Abs(_x) == 1) SymbolVisibility = SymbolVisibility.TopSymbol;
                    if (Math.Abs(_y) == 1) SymbolVisibility = SymbolVisibility.RightLeftSymbol;
                    if (Math.Abs(_z) == 1) SymbolVisibility = SymbolVisibility.FrontBackSymbol;
                }
            }
            if (_viewDirection == ViewDirection.SectionV)
            {
                if (OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    if (Math.Abs(_x) == 1) SymbolVisibility = SymbolVisibility.RightLeftSymbol;
                    if (Math.Abs(_y) == 1) SymbolVisibility = SymbolVisibility.FrontBackSymbol;
                }
                if (OpeningHost == OpeningHost.Wall)
                {
                    if (Math.Abs(_x) == 1) SymbolVisibility = SymbolVisibility.RightLeftSymbol;
                    if (Math.Abs(_y) == 1) SymbolVisibility = SymbolVisibility.TopSymbol;
                    if (Math.Abs(_z) == 1) SymbolVisibility = SymbolVisibility.FrontBackSymbol;
                }
            }
            if (_viewDirection == ViewDirection.PlanDown)
            {
                if (OpeningHost == OpeningHost.FloorOrCeiling) SymbolVisibility = SymbolVisibility.TopSymbol;
                if (OpeningHost == OpeningHost.Wall)
                {
                    if (Math.Abs(_z) == 1) SymbolVisibility = SymbolVisibility.RightLeftSymbol;
                    if (Math.Abs(_x) == 1 || Math.Abs(_y) == 1) SymbolVisibility = SymbolVisibility.FrontBackSymbol;
                }
            }
            if (_viewDirection == ViewDirection.PlanUp)
            {
                if (OpeningHost == OpeningHost.FloorOrCeiling) SymbolVisibility = SymbolVisibility.TopSymbol;
                if (OpeningHost == OpeningHost.Wall)
                {
                    if (Math.Abs(_z) == 1) SymbolVisibility = SymbolVisibility.RightLeftSymbol;
                    if (Math.Abs(_x) == 1 || Math.Abs(_y) == 1) SymbolVisibility = SymbolVisibility.FrontBackSymbol;
                }
            }
        }
        /// <summary>
        /// Requires revit transaction to run properly
        /// </summary>
        public void SwitchSymbol(GtbSchema gtbSchema)
        {
            Parameter parARC = FamilyInstance.LookupParameter("TWP");
            Parameter parTop = FamilyInstance.LookupParameter("Durchbruch durchgeschnitten (TWP, Top Symbol)");
            Parameter parLR = FamilyInstance.LookupParameter("Durchbruch durchgeschnitten (TWP, LR Symbol)");
            Parameter parFB = FamilyInstance.LookupParameter("Durchbruch durchgeschnitten (TWP, FB Symbol)");
            Parameter parOben = FamilyInstance.LookupParameter("Über Schnitt Ebene (TGA, Grundrisse)");
            SymbolToolString sts = new SymbolToolString();

            //Settings for ARC
            if (_viewDiscipline == ViewDiscipline.TWP)
            {
                parARC.Set(1);
                //gtbSchema.SetEntityField(FamilyInstance, "discipline", 1);
                sts.Discipline = 1;

                if (OpeningHost == OpeningHost.Wall && _isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(1);
                        //gtbSchema.SetEntityField(FamilyInstance, "fbSymbol", 1);
                        sts.FBSymbol = 1;
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(1);
                        //gtbSchema.SetEntityField(FamilyInstance, "lrSymbol", 1);
                        sts.LRSymbol = 1;
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(1);
                        //gtbSchema.SetEntityField(FamilyInstance, "topSymbol", 1);
                        sts.TopSymbol = 1;
                    }
                }
                if (OpeningHost == OpeningHost.Wall && !_isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(0);
                        //gtbSchema.SetEntityField(FamilyInstance, "fbSymbol", 2);
                        sts.FBSymbol = 2;
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(0);
                        //gtbSchema.SetEntityField(FamilyInstance, "lrSymbol", 2);
                        sts.LRSymbol = 2;
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(0);
                        //gtbSchema.SetEntityField(FamilyInstance, "topSymbol", 2);
                        sts.TopSymbol = 2;
                    }
                }
                if (OpeningHost == OpeningHost.FloorOrCeiling && _isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(1);
                        //gtbSchema.SetEntityField(FamilyInstance, "fbSymbol", 1);
                        sts.FBSymbol = 1;
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(1);
                        //gtbSchema.SetEntityField(FamilyInstance, "lrSymbol", 1);
                        sts.LRSymbol = 1;
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(1);
                        //gtbSchema.SetEntityField(FamilyInstance, "topSymbol", 1);
                        sts.TopSymbol = 1;
                    }
                }
                if (OpeningHost == OpeningHost.FloorOrCeiling && !_isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(0);
                        //gtbSchema.SetEntityField(FamilyInstance, "fbSymbol", 2);
                        sts.FBSymbol = 2;
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(0);
                        //gtbSchema.SetEntityField(FamilyInstance, "lrSymbol", 2);
                        sts.LRSymbol = 2;
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(0);
                        //gtbSchema.SetEntityField(FamilyInstance, "topSymbol", 2);
                        sts.TopSymbol = 2;
                    }
                }
            }

            //Settings for TGA
            if (_viewDiscipline == ViewDiscipline.TGA)
            {
                if (OpeningExStorage._disciplineX) DisciplineUserModified = true;
                parARC.Set(0);
                //gtbSchema.SetEntityField(FamilyInstance, "discipline", 2);
                sts.Discipline = 2;

                if (OpeningHost == OpeningHost.Wall)
                {
                    parOben.Set(0);
                    //gtbSchema.SetEntityField(FamilyInstance, "abSymbol", 2);
                    sts.ABSymbol = 2;
                }
                if (OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    if (_viewDirection == ViewDirection.PlanDown)
                    {
                        if(PlanViewLocation == PlanViewLocation.AboveCutPlane)
                        {
                            parOben.Set(1);
                            //gtbSchema.SetEntityField(FamilyInstance, "abSymbol", 1);
                            sts.ABSymbol = 1;
                        }
                        else
                        {
                            parOben.Set(0);
                            //gtbSchema.SetEntityField(FamilyInstance, "abSymbol", 2);
                            sts.ABSymbol = 2;
                        }
                    }
                    if (_viewDirection == ViewDirection.PlanUp)
                    {
                        if(PlanViewLocation == PlanViewLocation.BelowCutPlane)
                        {
                            parOben.Set(1);
                            //gtbSchema.SetEntityField(FamilyInstance, "abSymbol", 1);
                            sts.ABSymbol = 1;
                        }
                        else
                        {
                            parOben.Set(0);
                            //gtbSchema.SetEntityField(FamilyInstance, "abSymbol", 2);
                            sts.ABSymbol = 2;
                        }
                    }
                }
            }
            string symbolToolString = sts.CreateJsonString(OpeningExStorage);
            gtbSchema.SetEntityField(FamilyInstance, "symbolTool", symbolToolString);
        }

        public void SwitchSymbolLight()
        {
            Parameter parARC = FamilyInstance.get_Parameter(new Guid("20fc21fc-4af8-45fd-b412-aec0a0e7b8d5"));
            Parameter parTop = FamilyInstance.get_Parameter(new Guid("4fb37f53-ace7-4ff2-9e26-8cb09e41292e"));
            Parameter parLR = FamilyInstance.get_Parameter(new Guid("4381cf1b-088e-4127-9fd9-2c57366ab8d8"));
            Parameter parFB = FamilyInstance.get_Parameter(new Guid("6d06390a-c816-4108-8536-35d468a7ee3f"));
            Parameter parOben = FamilyInstance.get_Parameter(new Guid("19896ed8-279b-41d2-bf81-b77387d55c76"));
            Parameter manualSymbol = FamilyInstance.get_Parameter(new Guid("d985a238-7abd-4bee-a8c9-fcb2540b1eda"));

            if (manualSymbol.AsInteger() == 1) return;

            //Settings for ARC
            if (_viewDiscipline == ViewDiscipline.TWP)
            {
                parARC.Set(1);

                if (OpeningHost == OpeningHost.Wall && _isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(1);
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(1);
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(1);
                    }
                }
                if (OpeningHost == OpeningHost.Wall && !_isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(0);
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(0);
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(0);
                    }
                }
                if (OpeningHost == OpeningHost.FloorOrCeiling && _isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(1);
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(1);
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(1);
                    }
                }
                if (OpeningHost == OpeningHost.FloorOrCeiling && !_isCutByView)
                {
                    if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
                    {
                        parFB.Set(0);
                    }
                    if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
                    {
                        parLR.Set(0);
                    }
                    if (SymbolVisibility == SymbolVisibility.TopSymbol)
                    {
                        parTop.Set(0);
                    }
                }
            }

            //Settings for TGA
            if (_viewDiscipline == ViewDiscipline.TGA)
            {
                if (OpeningExStorage._disciplineX) DisciplineUserModified = true;
                parARC.Set(0);

                if (OpeningHost == OpeningHost.Wall)
                {
                    parOben.Set(0);
                }
                if (OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    if (_viewDirection == ViewDirection.PlanDown)
                    {
                        if (PlanViewLocation == PlanViewLocation.AboveCutPlane)
                        {
                            parOben.Set(1);
                        }
                        else
                        {
                            parOben.Set(0);
                        }
                    }
                    if (_viewDirection == ViewDirection.PlanUp)
                    {
                        if (PlanViewLocation == PlanViewLocation.BelowCutPlane)
                        {
                            parOben.Set(1);
                        }
                        else
                        {
                            parOben.Set(0);
                        }
                    }
                }
            }
        }

        public List<string> GetManualChanges()
        {
            List<string> result = new List<string>();

            if (OpeningExStorage._disciplineX)
            {
                string info = "Manual change of discipline!";
                result.Add(info);
            }

            if (SymbolVisibility == SymbolVisibility.FrontBackSymbol)
            {
                if (OpeningExStorage._fBSymbolX)
                {
                    string info = "Manual change of front back symbol";
                    result.Add(info);
                }
            }
            if (SymbolVisibility == SymbolVisibility.RightLeftSymbol)
            {
                if (OpeningExStorage._lRSymbolX)
                {
                    string info = "Manual change of left right symbol";
                    result.Add(info);
                }
            }
            if (SymbolVisibility == SymbolVisibility.TopSymbol)
            {
                if (OpeningExStorage._topSymbolX)
                {
                    string info = "Manual change of top symbol (TWP)";
                    result.Add(info);
                }
                if (OpeningExStorage._aBSymbolX)
                {
                    string info = "Manual change of top symbol (TGA)";
                    result.Add(info);
                }
            }
            return result;
        }
    }
}
