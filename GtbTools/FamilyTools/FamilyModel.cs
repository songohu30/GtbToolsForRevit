using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTools
{
    public class FamilyModel
    {
        public bool ParametersExist { get; set; } = false;
        public Document FamilyDocument { get; set; }

        Parameter _partType;
        Family _family;
        int _connectorNumber;
        List<ConnectorElement> _connectors;

        private FamilyModel()
        {

        }

        public static FamilyModel Initialize(Family family, Document familyDocument)
        {
            FamilyModel result = new FamilyModel();
            result._family = family;
            result.FamilyDocument = familyDocument;
            //if (result.IsShared())
            //{
            //    familyDocument.Close(false);
            //    return null;
            //}
            if (!result.SetPartType())
            {
                familyDocument.Close(false);
                return null;
            }
            result.CheckParameters();
            if (result.ParametersExist)
            {
                familyDocument.Close(false);
                return null;
            }
            result.SetNoOfConnectors();
            if (!result.CheckConnectorNumber())
            {
                familyDocument.Close(false);
                return null;
            }
            return result;
        }

        private void CheckParameters()
        {
            FamilyManager fmgr = FamilyDocument.FamilyManager;
            FamilyParameter c1 = fmgr.get_Parameter(new Guid("a8476c7b-9575-4eb3-9e0e-3c3f57d104e7"));
            FamilyParameter c2 = fmgr.get_Parameter(new Guid("09264bc7-2de5-4edf-a47e-53dad64868f6"));
            FamilyParameter c3 = fmgr.get_Parameter(new Guid("983091c7-b669-4809-a421-9fbca08f9daf"));
            if (_partType.AsValueString() == "Tee" && c1 != null && c2 != null && c3 != null) ParametersExist = true;
            if (_partType.AsValueString() == "Cap" && c1 != null) ParametersExist = true;
            if (_partType.AsValueString() == "Elbow" && c1 != null && c2 != null) ParametersExist = true;
            if (_partType.AsValueString() == "Transition" && c1 != null && c2 != null) ParametersExist = true;
            if (_partType.AsValueString() == "Union" && c1 != null && c2 != null) ParametersExist = true;
            if (_partType.AsValueString() == "Flange" && c1 != null && c2 != null) ParametersExist = true;

            //if (c1 != null && c2 != null && _partType.AsValueString() == "Elbow" || _partType.AsValueString() == "Transition"
            //        || _partType.AsValueString() == "Union" || _partType.AsValueString() == "Flange") ParametersExist = true;
        }

        public void InsertParameters(ExternalDefinition connector1, ExternalDefinition connector2, ExternalDefinition connector3)
        {
            FamilyManager fmgr = FamilyDocument.FamilyManager;
            FamilyParameter c1 = fmgr.get_Parameter(new Guid("a8476c7b-9575-4eb3-9e0e-3c3f57d104e7"));
            FamilyParameter c2 = fmgr.get_Parameter(new Guid("09264bc7-2de5-4edf-a47e-53dad64868f6"));
            FamilyParameter c3 = fmgr.get_Parameter(new Guid("983091c7-b669-4809-a421-9fbca08f9daf"));
            using (Transaction tx = new Transaction(FamilyDocument, "Adding parameters to family"))
            {
                tx.Start();
                for (int i = 0; i < _connectorNumber; i++)
                {
                    if (i == 0 && c1 == null) fmgr.AddParameter(connector1, BuiltInParameterGroup.PG_GEOMETRY, true);
                    if (i == 1 && c2 == null) fmgr.AddParameter(connector2, BuiltInParameterGroup.PG_GEOMETRY, true);
                    if (i == 2 && c3 == null) fmgr.AddParameter(connector3, BuiltInParameterGroup.PG_GEOMETRY, true);
                    //if (i == 3) fmgr.AddParameter(connector4, BuiltInParameterGroup.PG_GEOMETRY, true);
                }
                tx.Commit();
            }
        }

        public void SetValues()
        {
            FamilyManager fmgr = FamilyDocument.FamilyManager;
            FamilyParameterSet familyParameters = fmgr.Parameters;

            using (Transaction tx = new Transaction(FamilyDocument, "Setting diameter"))
            {
                for (int i = 0; i < _connectorNumber; i++)
                {
                    if (i == 0)
                    {
                        FamilyParameter c1 = fmgr.get_Parameter(new Guid("a8476c7b-9575-4eb3-9e0e-3c3f57d104e7"));
                        ConnectorElement ce = _connectors[i];
                        IList<Parameter> ceParameters = ce.GetOrderedParameters();

                        foreach (Parameter p in ceParameters)
                        {
                            foreach (FamilyParameter fp in familyParameters)
                            {
                                ParameterSet asoParameters = fp.AssociatedParameters;
                                foreach (Parameter asoPar in asoParameters)
                                {
                                    if (fp.Definition.ParameterType == ParameterType.Angle) continue;
                                    if(p.Id.IntegerValue == asoPar.Id.IntegerValue && asoPar.Element.Id.IntegerValue == ce.Id.IntegerValue)
                                    {
                                        string formula = "";
                                        if (asoPar.Id.IntegerValue == (int)BuiltInParameter.CONNECTOR_RADIUS) formula = fp.Definition.Name + "*2";
                                        if (asoPar.Id.IntegerValue == (int)BuiltInParameter.CONNECTOR_DIAMETER) formula = fp.Definition.Name;
                                        tx.Start();
                                        fmgr.SetFormula(c1, formula);
                                        tx.Commit();
                                    }
                                }
                            }
                        }
                    }

                    if (i == 1)
                    {
                        FamilyParameter c2 = fmgr.get_Parameter(new Guid("09264bc7-2de5-4edf-a47e-53dad64868f6"));
                        ConnectorElement ce = _connectors[i];
                        IList<Parameter> ceParameters = ce.GetOrderedParameters();

                        foreach (Parameter p in ceParameters)
                        {
                            foreach (FamilyParameter fp in familyParameters)
                            {
                                ParameterSet asoParameters = fp.AssociatedParameters;
                                foreach (Parameter asoPar in asoParameters)
                                {
                                    if (fp.Definition.ParameterType == ParameterType.Angle) continue;
                                    if (p.Id.IntegerValue == asoPar.Id.IntegerValue && asoPar.Element.Id.IntegerValue == ce.Id.IntegerValue)
                                    {
                                        string formula = "";
                                        if (asoPar.Id.IntegerValue == (int)BuiltInParameter.CONNECTOR_RADIUS) formula = fp.Definition.Name + "*2";
                                        if (asoPar.Id.IntegerValue == (int)BuiltInParameter.CONNECTOR_DIAMETER) formula = fp.Definition.Name;
                                        tx.Start();
                                        fmgr.SetFormula(c2, formula);
                                        tx.Commit();
                                    }
                                }
                            }
                        }
                    }

                    if (i == 2)
                    {
                        FamilyParameter c3 = fmgr.get_Parameter(new Guid("983091c7-b669-4809-a421-9fbca08f9daf"));
                        ConnectorElement ce = _connectors[i];
                        IList<Parameter> ceParameters = ce.GetOrderedParameters();

                        foreach (Parameter p in ceParameters)
                        {
                            foreach (FamilyParameter fp in familyParameters)
                            {
                                ParameterSet asoParameters = fp.AssociatedParameters;
                                foreach (Parameter asoPar in asoParameters)
                                {
                                    if (fp.Definition.ParameterType == ParameterType.Angle) continue;
                                    if (p.Id.IntegerValue == asoPar.Id.IntegerValue && asoPar.Element.Id.IntegerValue == ce.Id.IntegerValue)
                                    {
                                        string formula = "";
                                        if (asoPar.Id.IntegerValue == (int)BuiltInParameter.CONNECTOR_RADIUS) formula = fp.Definition.Name + "*2";
                                        if (asoPar.Id.IntegerValue == (int)BuiltInParameter.CONNECTOR_DIAMETER) formula = fp.Definition.Name;
                                        tx.Start();
                                        fmgr.SetFormula(c3, formula);
                                        tx.Commit();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsShared()
        {
            bool result = false;
            Parameter shared = _family.get_Parameter(BuiltInParameter.FAMILY_SHARED);
            if (shared.AsInteger() == 1) result = true;
            return result;
        }

        private bool SetPartType()
        {
            bool result = false;
            _partType = _family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);
            if (_partType.AsValueString() == "Elbow" || _partType.AsValueString() == "Cap" || _partType.AsValueString() == "Flange"
                || _partType.AsValueString() == "Tee" || _partType.AsValueString() == "Transition" || _partType.AsValueString() == "Union") result = true;
            return result;
        }

        private void SetNoOfConnectors()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(FamilyDocument);
            _connectors = ficol.OfClass(typeof(ConnectorElement)).Select(e => e as ConnectorElement).ToList();
            _connectorNumber = _connectors.Count;
        }
        private bool CheckConnectorNumber()
        {
            bool result = true;
            // Elbow, Cap, Cross, Flange, Tee, Transition, Union
            string info = _family.Name + Environment.NewLine;
            if (_partType.AsValueString() == "Elbow" && _connectorNumber != 2)
            {
                info += String.Format("Part type is elbow but number of connectors = {0}", _connectorNumber);
                TaskDialog.Show("Warning", info);
                result = false;
            }
            if (_partType.AsValueString() == "Cap" && _connectorNumber != 1)
            {
                info += String.Format("Part type is cap but number of connectors = {0}", _connectorNumber);
                TaskDialog.Show("Warning", info);
                result = false;
            }
            if (_partType.AsValueString() == "Cross" && _connectorNumber != 4)
            {
                info += String.Format("Part type is cross but number of connectors = {0}", _connectorNumber);
                TaskDialog.Show("Warning", info);
                result = false;
            }
            if (_partType.AsValueString() == "Flange" && _connectorNumber != 2)
            {
                info += String.Format("Part type is flange but number of connectors = {0}", _connectorNumber);
                TaskDialog.Show("Warning", info);
                result = false;
            }
            if (_partType.AsValueString() == "Tee" && _connectorNumber != 3)
            {
                info += String.Format("Part type is tee but number of connectors = {0}", _connectorNumber);
                TaskDialog.Show("Warning", info);
                result = false;
            }
            if (_partType.AsValueString() == "Transition" && _connectorNumber != 2)
            {
                info += String.Format("Part type is transition but number of connectors = {0}", _connectorNumber);
                TaskDialog.Show("Warning", info);
                result = false;
            }
            if (_partType.AsValueString() == "Union" && _connectorNumber != 2)
            {
                info += String.Format("Part type is union but number of connectors = {0}", _connectorNumber);
                TaskDialog.Show("Warning", info);
                result = false;
            }
            return result;
        }
    }
}
