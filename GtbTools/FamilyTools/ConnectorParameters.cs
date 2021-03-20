using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTools
{
    public class ConnectorParameters
    {
        List<FamilyModel> _familyModels;
        List<Family> _pipeFittingFamilies;
        Document _doc;
        DefinitionFile _definitionFile;
        ExternalDefinition connector1 = null;
        ExternalDefinition connector2 = null;
        ExternalDefinition connector3 = null;
        string _processedFamilies;

        public ConnectorParameters(Document document, DefinitionFile definitionFile)
        {
            _doc = document;
            _definitionFile = definitionFile;
        }

        public void Initialize(int stepNo)
        {
            _processedFamilies = "";
            if (!SetConnectorParameters()) return;
            SetPipeFittingFamilies(stepNo);
            SetFamilyModels();
        }

        public void SetParameters()
        {
            if (_familyModels == null || _familyModels.Count == 0) return;
            try
            {
                foreach(FamilyModel fm in _familyModels)
                {
                    fm.InsertParameters(connector1, connector2, connector3);
                    if (!fm.ParametersExist)
                    {
                        fm.SetValues();
                        fm.FamilyDocument.LoadFamily(_doc, new FamilyOption());
                    }
                    fm.FamilyDocument.Close(false);
                }
                string successInfo = "Successfully added parameters to:" + Environment.NewLine + Environment.NewLine + _processedFamilies;
                TaskDialog.Show("Info", successInfo);
            }
            catch (Exception ex)
            {
                string failureInfo = "Failed to add parameters to:" + Environment.NewLine + Environment.NewLine + _processedFamilies;
                TaskDialog.Show("Error", failureInfo + Environment.NewLine + ex.ToString());
            }     
        }

        private void SetFamilyModels()
        {
            _familyModels = new List<FamilyModel>();
            foreach (Family family in _pipeFittingFamilies)
            {
                Document familyDocument = _doc.EditFamily(family);
                FamilyModel familyModel = FamilyModel.Initialize(family, familyDocument);
                if (familyModel != null)
                {
                    _familyModels.Add(familyModel);
                    _processedFamilies += family.Name + Environment.NewLine;
                }
            }
            if (_familyModels.Count == 0) TaskDialog.Show("Info", "There are 0 new pipe families to proccess!");
        }

        private bool SetConnectorParameters()
        {
            bool result = false;

            DefinitionGroups myGroups = _definitionFile.Groups;
            DefinitionGroup myGroup = myGroups.get_Item("Rohr");
            if (myGroup != null)
            {
                connector1 = myGroup.Definitions.get_Item("Rohrformteil_Connector_1") as ExternalDefinition;
                connector2 = myGroup.Definitions.get_Item("Rohrformteil_Connector_2") as ExternalDefinition;
                connector3 = myGroup.Definitions.get_Item("Rohrformteil_Connector_3") as ExternalDefinition;
                //connector4 = myGroup.Definitions.get_Item("Connector4_GTB") as ExternalDefinition;
            }
            if (connector1 == null || connector2 == null || connector3 == null)
            {
                TaskDialog.Show("Info", "Missing shared parameters (Rohrformteil_Connector_1,2,3!");
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        private void SetPipeFittingFamilies(int stepNo)
        {
            int maxStepNo = stepNo;
            FilteredElementCollector ficol = new FilteredElementCollector(_doc);
            _pipeFittingFamilies = ficol.OfClass(typeof(Family)).Select(e => e as Family)
                                            .Where(e => e.FamilyCategoryId.IntegerValue == (int)BuiltInCategory.OST_PipeFitting
                                                && !SkipFamily(e) && CheckPartType(e) && !ParametersExist(e) && HasConnectors(e)).ToList();
            if (stepNo > _pipeFittingFamilies.Count) maxStepNo = _pipeFittingFamilies.Count;
            _pipeFittingFamilies = _pipeFittingFamilies.OrderBy(e => e.Name).ToList().Take(stepNo).ToList();
        }
        private bool CheckPartType(Family family)
        {
            bool result = false;
            Parameter partType = family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);
            if (partType.AsValueString() == "Elbow" || partType.AsValueString() == "Cap" || partType.AsValueString() == "Flange"
                || partType.AsValueString() == "Tee" || partType.AsValueString() == "Transition" || partType.AsValueString() == "Union") result = true;
            return result;
        }

        private bool SkipFamily(Family family)
        {
            return family.Name.ToUpper().Contains("SKIP");
        }

        private bool HasConnectors(Family family)
        {
            bool result = false;
            Document familyDocument = _doc.EditFamily(family);
            FilteredElementCollector ficol = new FilteredElementCollector(familyDocument);
            List<ConnectorElement> connectors = ficol.OfClass(typeof(ConnectorElement)).Select(e => e as ConnectorElement).ToList();
            familyDocument.Close(false);
            if (connectors.Count > 0) result = true;
            return result;
        }

        private bool ParametersExist(Family family)
        {
            bool result = false;
            Parameter partType = family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);
            Document familyDocument = _doc.EditFamily(family);
            FamilyManager fmgr = familyDocument.FamilyManager;
            FamilyParameter c1 = fmgr.get_Parameter(new Guid("a8476c7b-9575-4eb3-9e0e-3c3f57d104e7"));
            FamilyParameter c2 = fmgr.get_Parameter(new Guid("09264bc7-2de5-4edf-a47e-53dad64868f6"));
            FamilyParameter c3 = fmgr.get_Parameter(new Guid("983091c7-b669-4809-a421-9fbca08f9daf"));
            if (partType.AsValueString() == "Tee" && c1 != null && c2 != null && c3 != null) result = true;
            if (partType.AsValueString() == "Cap" && c1 != null) result = true;
            if (partType.AsValueString() == "Elbow" && c1 != null && c2 != null) result = true;
            if (partType.AsValueString() == "Transition" && c1 != null && c2 != null) result = true;
            if (partType.AsValueString() == "Union" && c1 != null && c2 != null) result = true;
            if (partType.AsValueString() == "Flange" && c1 != null && c2 != null) result = true;
            familyDocument.Close(false);
            return result;
        }

        class FamilyOption : IFamilyLoadOptions
        {
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }
    }
}
