using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExStorage
{
    public class OpeningExStorage
    {
        public int Discipline { get; set; }
        public int TopSymbol { get; set; }
        public int LRSymbol { get; set; }
        public int FBSymbol { get; set; }
        public int ABSymbol { get; set; }
        public int ManSymbol { get; set; }

        bool _checkboxNotFound = false;

        Schema _schema;
        FamilyInstance _familyInstance;
        public int _discipline;
        public int _topSymbol;
        public int _lRSymbol;
        public int _fBSymbol;
        public int _aBSymbol;
        public int _manSymbol;
        public bool _disciplineX { get; set; }
        public bool _topSymbolX { get; set; }
        public bool _lRSymbolX { get; set; }
        public bool _fBSymbolX { get; set; }
        public bool _aBSymbolX { get; set; }
        public bool _manSymbolX { get; set; }

        public OpeningExStorage(FamilyInstance familyInstance)
        {
            _familyInstance = familyInstance;
        }

        public bool CompareData()
        {
            if (_checkboxNotFound) return false;

            _disciplineX = false;
            if (Discipline == 1 && _discipline == 0) _disciplineX = true;
            if (Discipline == 2 && _discipline == 1) _disciplineX = true;

            _topSymbolX = false;
            if (TopSymbol == 1 && _topSymbol == 0) _topSymbolX = true;
            if (TopSymbol == 2 && _topSymbol == 1) _topSymbolX = true;

            _lRSymbolX = false;
            if (LRSymbol == 1 && _lRSymbol == 0) _lRSymbolX = true;
            if (LRSymbol == 2 && _lRSymbol == 1) _lRSymbolX = true;

            _fBSymbolX = false;
            if (FBSymbol == 1 && _fBSymbol == 0) _fBSymbolX = true;
            if (FBSymbol == 2 && _fBSymbol == 1) _fBSymbolX = true;

            _aBSymbolX = false;
            if (ABSymbol == 1 && _aBSymbol == 0) _aBSymbolX = true;
            if (ABSymbol == 2 && _aBSymbol == 1) _aBSymbolX = true;

            _manSymbolX = false;
            if (ManSymbol == 1 && _manSymbol == 0) _manSymbolX = true;
            if (ManSymbol == 2 && _manSymbol == 1) _manSymbolX = true;
            return (_disciplineX || _topSymbolX || _lRSymbolX || _fBSymbolX || _aBSymbolX || _manSymbolX);
        }

        public void ReadCurrentSettings()
        {
            Parameter parARC = _familyInstance.LookupParameter("TWP");
            Parameter parTop = _familyInstance.LookupParameter("Durchbruch durchgeschnitten (TWP, Top Symbol)");
            Parameter parLR = _familyInstance.LookupParameter("Durchbruch durchgeschnitten (TWP, LR Symbol)");
            Parameter parFB = _familyInstance.LookupParameter("Durchbruch durchgeschnitten (TWP, FB Symbol)");
            Parameter parOben = _familyInstance.LookupParameter("Über Schnitt Ebene (TGA, Grundrisse)");
            Parameter manSym = _familyInstance.LookupParameter("Manuelles Symbol");

            if (parARC == null || parTop == null || parLR == null || parFB == null || parOben == null || manSym == null)
            {
                _checkboxNotFound = true;
                return;
            }

            _discipline = parARC.AsInteger();
            _topSymbol = parTop.AsInteger();
            _lRSymbol = parLR.AsInteger();
            _fBSymbol = parFB.AsInteger();
            _aBSymbol = parOben.AsInteger();
            _manSymbol = manSym.AsInteger();
        }

        public void ReadExternalStorage()
        {
        if (_checkboxNotFound) return;
        GetSchema();
        Entity retrievedEntity = _familyInstance.GetEntity(_schema);
            try
            {
                string jsonString = retrievedEntity.Get<string>(_schema.GetField("symbolTool"));
                SymbolToolString sts = SymbolToolString.ReadJsonString(jsonString);
                Discipline = sts.Discipline;
                TopSymbol = sts.TopSymbol;
                LRSymbol = sts.LRSymbol;
                FBSymbol = sts.FBSymbol;
                ABSymbol = sts.ABSymbol;
                ManSymbol = sts.ManSymbol;
            }
            catch
            {
                Discipline = -1;
                TopSymbol = -1;
                LRSymbol = -1;
                FBSymbol = -1;
                ABSymbol = -1;
                ManSymbol = -1;
            }
        }
        private void GetSchema()
        {
            GtbSchema gtbSchema = new GtbSchema();
            gtbSchema.SetGtbSchema();
            _schema = gtbSchema.Schema;
        }
    }
}
