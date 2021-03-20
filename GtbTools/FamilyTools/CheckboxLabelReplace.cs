using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTools
{
    public class CheckboxLabelReplace
    {
        Document _doc;

        public CheckboxLabelReplace(Document doc)
        {
            _doc = doc;
        }

		public void AddParameter()
		{
			if (!_doc.IsFamilyDocument)
			{
				TaskDialog.Show("Info", "Not a family document");
				return;
			}
			FamilyManager fmgr = _doc.FamilyManager;
			FamilyParameter familyParameter = fmgr.get_Parameter("Symbol");
			if(familyParameter != null)
            {
				TaskDialog.Show("Info", "Symbol parameter already exists!");
				return;
            }
			Category category = Category.GetCategory(_doc, BuiltInCategory.OST_GenericAnnotation);
			try
			{
				using (Transaction tx = new Transaction(_doc, "Inserting parameter"))
				{
					tx.Start();
					fmgr.AddParameter("Symbol", BuiltInParameterGroup.INVALID, category, false);
					tx.Commit();
				}
			}
			catch (Exception ex)
			{
				TaskDialog.Show("Error", ex.ToString());
			}
		}

		public void AlignTypesWithSymbol()
		{
			if (!_doc.IsFamilyDocument)
			{
				TaskDialog.Show("Info", "Not a family document");
				return;
			}

			FilteredElementCollector ficol = new FilteredElementCollector(_doc);
			List<Element> elements = ficol.OfClass(typeof(FamilySymbol)).ToList();

			FamilyManager fmgr = _doc.FamilyManager;
			FamilyParameter familyParameter = fmgr.get_Parameter("Symbol");
			if(familyParameter == null)
            {
				TaskDialog.Show("Info", "Can't find Symbol parameter!");
				return;
            }
			FamilyTypeSet types = fmgr.Types;
			string errorList = "Passenden symbol nicht gefunden:";
			Transaction tx = new Transaction(_doc, "Search types, delete checkboxes");
			tx.Start();
			foreach (FamilyType type in types)
			{
				string typeName = type.Name;
				Element el = elements.Where(e => typeName.ToUpper().Contains(e.Name.ToUpper())).FirstOrDefault();
				FamilySymbol fs = el as FamilySymbol;
				if (el != null)
				{
					fmgr.CurrentType = type;
					fmgr.Set(familyParameter, el.Id);
					FindRemoveParameter(fmgr, el.Name);
				}
				else
				{
					errorList += Environment.NewLine + typeName;
				}

			}
			tx.Commit();
			if (errorList != "Passenden symbol nicht gefunden:")
			{
				TaskDialog.Show("Info", errorList);
			}
		}

		private void FindRemoveParameter(FamilyManager fmgr, string name)
		{
			foreach (FamilyParameter par in fmgr.Parameters)
			{
				if (par.Definition.ParameterType == ParameterType.YesNo)
				{
					if(par.Definition.Name.ToUpper().Contains(name.ToUpper()))
                    {
						fmgr.RemoveParameter(par);
					}
				}
			}
		}

		public void DeleteAndSetLabel()
		{
			if (!_doc.IsFamilyDocument)
			{
				TaskDialog.Show("Info", "Not a family document");
				return;
			}
			FamilyManager fmgr = _doc.FamilyManager;
			FamilyParameter familyParameter = fmgr.get_Parameter("Symbol");
			if (familyParameter == null)
			{
				TaskDialog.Show("Info", "Can't find Symbol parameter!");
				return;
			}
			FilteredElementCollector ficol = new FilteredElementCollector(_doc);
			List<Element> elements = ficol.OfClass(typeof(FamilyInstance)).ToList();
			int count = elements.Count;
			FamilyInstance instanceZero = null;
			Transaction tx = new Transaction(_doc, "Delete instances, set label");
			tx.Start();
			for (int i = 0; i < count; i++)
			{
				if (i == 0)
				{
					instanceZero = elements[0] as FamilyInstance;
				}
				else
				{
					_doc.Delete(elements[i].Id);
				}
			}
			Parameter label = instanceZero.get_Parameter(BuiltInParameter.ELEM_TYPE_LABEL);
			label.Set(familyParameter.Id);
			Parameter visible = instanceZero.get_Parameter(BuiltInParameter.IS_VISIBLE_PARAM);
			visible.DissociateFromGlobalParameter();
			visible.Set(1);
			tx.Commit();
		}

		public void AlignTypesByVisibility()
        {
			if (!_doc.IsFamilyDocument)
			{
				TaskDialog.Show("Info", "Not a family document");
				return;
			}
			FilteredElementCollector ficol = new FilteredElementCollector(_doc);
			List<Element> elements = ficol.OfClass(typeof(FamilyInstance)).ToList();
			FamilyManager fmgr = _doc.FamilyManager;
			FamilyParameter symbolParameter = fmgr.get_Parameter("Symbol");
			if (symbolParameter == null)
			{
				TaskDialog.Show("Info", "Can't find Symbol parameter!");
				return;
			}
			FamilyTypeSet types = fmgr.Types;
			using (Transaction tx = new Transaction(_doc, "Search types, delete checkboxes"))
			{
				tx.Start();
				foreach (FamilyType type in types)
				{
					fmgr.CurrentType = type;
					List<Element> sorted = elements.Where(e => e.get_Parameter(BuiltInParameter.IS_VISIBLE_PARAM).AsInteger() == 1).ToList();
					if (sorted.Count == 1)
					{
						FamilyInstance fi = sorted[0] as FamilyInstance;
						FamilySymbol fs = fi.Symbol;
						Parameter visible = fi.get_Parameter(BuiltInParameter.IS_VISIBLE_PARAM);
						visible.DissociateFromGlobalParameter();						
						fmgr.Set(symbolParameter, fs.Id);
						List<FamilyParameter> checkboxes = fmgr.GetParameters().Where(e => e.Definition.ParameterType == ParameterType.YesNo).ToList();
                        foreach (FamilyParameter fp in checkboxes)
                        {
							if (type.AsInteger(fp) == 1) fmgr.RemoveParameter(fp);
                        }
						visible.Set(0);
					}
					if (sorted.Count == 0)
					{
						TaskDialog.Show("Info", "Family type: " + type.Name + " has 0 visible elements!");
					}
					if (sorted.Count > 1)
					{
						string visibleElements = "";
                        foreach (var item in sorted)
                        {
							visibleElements += item.Name + Environment.NewLine;
                        }
						TaskDialog.Show("Info", "Family type: " + type.Name + " has " + sorted.Count + " visible elements!" + Environment.NewLine + visibleElements);
					}
				}
				tx.Commit();
			}
		}
	}
}
