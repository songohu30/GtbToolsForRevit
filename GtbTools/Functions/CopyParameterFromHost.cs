using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public class CopyParameterFromHost
    {
        public Parameter HostSourceParameter { get; set; } //different types
        public Parameter HostDestinationParameter { get; set; } //text type
        public Parameter SelfSourceParameter { get; set; } //different types
        public Parameter SelfDestinationParameter { get; set; } //text type
        public List<FamilySymbol> GenericSymbols { get; set; }
        public FamilySymbol HostCopySelectedSymbol { get; set; }
        public FamilySymbol SelfCopySelectedSymbol { get; set; }
        public List<FamilyInstance> OpeningInstancesHost { get; set; }
        public List<FamilyInstance> OpeningInstancesSelf { get; set; }
        public ExternalEvent InitializeEvent { get; set; }
        public bool IsInitialized { get; set; } = false;


        List<FamilyInstance> _genericModelInstances { get; set; }
        Document _document;
        string _sourceParameterName;
        string _destParameterName;
        bool? _isTypeHost;
        public bool _hostClicked = false;
        public bool _selfClicked = false;

        public CopyParameterFromHost()
        {

        }

        public void Initialize(Document document)
        {
            _document = document;
            SetAllGenericInstances();
            SetGenericSymbols();
            IsInitialized = true;
        }

        public void SetParameterNames(string source, string destination, bool? isType)
        {
            _sourceParameterName = source;
            _destParameterName = destination;
            _isTypeHost = isType;
        }

        public void SetGenericSymbols()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_document);
            GenericSymbols = ficol.OfClass(typeof(FamilySymbol))
                        .Select(x => x as FamilySymbol)
                            .Where(y => y.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
        }

        public void DisplayWindow()
        {
            CopyPasteWindow copyPasteWindow = new CopyPasteWindow(this);
            copyPasteWindow.Show();
        }

        public void SetEvents(ExternalEvent externalEvent)
        {
            InitializeEvent = externalEvent;
        }

        public Parameter GetHostParameter(FamilyInstance opening)
        {
            Parameter parameter = null;
            Element element = opening.Host;
            if(element != null)
            {
                parameter = element.LookupParameter(_sourceParameterName);
            }
            return parameter;
        }

        public Parameter GetHostTypeParameter(FamilyInstance opening)
        {
            Parameter parameter = null;
            Element element = opening.Host;
            if (element != null)
            {
                ElementId typeId = element.GetTypeId();
                Element type = _document.GetElement(typeId);
                parameter = type.LookupParameter(_sourceParameterName);
            }
            return parameter;
        }

        public Parameter GetGenericInstanceDestinationParameter(FamilyInstance opening)
        {
            Parameter parameter = opening.LookupParameter(_destParameterName);
            return parameter;           
        }

        public Parameter GetGenericInstanceSourceParameter(FamilyInstance opening)
        {
            Parameter parameter = opening.LookupParameter(_sourceParameterName);
            return parameter;
        }

        public void CopyParametersHost()
        {
            using(Transaction tx = new Transaction(_document, "Copy paste parameters"))
            {
                tx.Start();
                foreach (FamilyInstance familyInstance in OpeningInstancesHost)
                {
                    Parameter source;
                    if (_isTypeHost == true)
                    {
                        source = GetHostTypeParameter(familyInstance);
                    }
                    else
                    {
                        source = GetHostParameter(familyInstance);
                    }
                    Parameter destination = GetGenericInstanceDestinationParameter(familyInstance);
                    if (source == null || destination == null) continue;
                    if (destination.IsReadOnly) continue;
                    string value;
                    if(source.StorageType == StorageType.String)
                    {
                        value = source.AsString();
                    }
                    else
                    {
                        value = source.AsValueString();
                    }
                    destination.Set(value);
                }
                tx.Commit();
            }
        }

        public void CopyParametersSelf()
        {
            using (Transaction tx = new Transaction(_document, "Copy paste parameters"))
            {
                tx.Start();
                foreach (FamilyInstance familyInstance in OpeningInstancesSelf)
                {
                    Parameter source = GetGenericInstanceSourceParameter(familyInstance);
                    Parameter destination = GetGenericInstanceDestinationParameter(familyInstance);
                    if (source == null || destination == null) continue;
                    if (destination.IsReadOnly) continue;
                    string value;
                    if (source.StorageType == StorageType.String)
                    {
                        value = source.AsString();
                    }
                    else
                    {
                        value = source.AsValueString();
                    }
                    destination.Set(value);
                }
                tx.Commit();
            }
        }


        public void SetOpeningInstancesHost()
        {
            OpeningInstancesHost = new List<FamilyInstance>();
            OpeningInstancesHost = _genericModelInstances.Where(x => x.Symbol.Id.IntegerValue == HostCopySelectedSymbol.Id.IntegerValue).ToList();
        }

        public void SetOpeningInstancesSelf()
        {
            OpeningInstancesSelf = new List<FamilyInstance>();
            OpeningInstancesSelf = _genericModelInstances.Where(x => x.Symbol.Id.IntegerValue == SelfCopySelectedSymbol.Id.IntegerValue).ToList();
        }

        public bool CheckHostSourceParameter(string parameterName)
        {
            bool result = false;
            Parameter check = HostCopySelectedSymbol.LookupParameter(parameterName);
            if (check != null)
            {
                HostDestinationParameter = check;
                result = true;
            }
            return result;
        }

        public bool CheckHostDestinationParameter(string parameterName)
        {
            bool result = false;
            Parameter check = HostCopySelectedSymbol.LookupParameter(parameterName);
            if(check != null)
            {
                HostDestinationParameter = check;
                result = true;
            }
            return result;
        }

        public bool CheckSelfSourceParameter(string parameterName)
        {
            bool result = false;
            Parameter check = HostCopySelectedSymbol.LookupParameter(parameterName);
            if (check != null)
            {
                HostDestinationParameter = check;
                result = true;
            }
            return result;
        }

        public bool CheckSelfDestinationParameter(string parameterName)
        {
            bool result = false;
            Parameter check = HostCopySelectedSymbol.LookupParameter(parameterName);
            if (check != null)
            {
                HostDestinationParameter = check;
                result = true;
            }
            return result;
        }

        private void SetAllGenericInstances()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_document);
            _genericModelInstances = ficol.OfClass(typeof(FamilyInstance))
                        .Select(x => x as FamilyInstance)
                            .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
        }
    }
}
