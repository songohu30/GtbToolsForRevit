using Abstract;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Functions
{
    public class CalculationSupport : CustomFunction
    {
        public ExternalEvent TheEvent { get; set; }
        public ManualResetEvent SignalEvent { get; set; }
		public Calculations.Action Action { get; set; }

        UIDocument _uiDocument;

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                CalculationSupportWindow window = new CalculationSupportWindow(this);
                window.Show();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void Initialize(UIDocument uIDocument)
        {
            SignalEvent = new ManualResetEvent(false);
            _uiDocument = uIDocument;
        }

        public void SetEvent(ExternalEvent theEvent)
        {
            TheEvent = theEvent;
        }

        public void FindDisconnectedFitting()
        {
            Document doc = _uiDocument.Document;
            List<FamilyInstance> pFittings = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting).ToList();
            foreach (FamilyInstance fi in pFittings)
            {
                Parameter systemClass = fi.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
                string name = systemClass.AsString();
                if (name == "Abwasser") continue;
                MechanicalFitting mf = fi.MEPModel as MechanicalFitting;
                ConnectorManager cm = mf.ConnectorManager;
				if (cm == null) continue;
                ConnectorSet csun = cm.UnusedConnectors;
                if (csun == null) continue;
                if (csun.Size > 0)
                {
                    _uiDocument.Selection.SetElementIds(new List<ElementId>() { fi.Id });
                    _uiDocument.ShowElements(fi.Id);
                    return;
                }
            }
        }

        public void FindFittingsWithReducers()
        {
            Document doc = _uiDocument.Document;
            List<FamilyInstance> pFittings = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting).ToList();
            foreach (FamilyInstance fi in pFittings)
            {
                Parameter systemClass = fi.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
                string name = systemClass.AsString();
                if (name == "Abwasser") continue;

                MechanicalFitting mf = fi.MEPModel as MechanicalFitting;
                ConnectorManager cm = mf.ConnectorManager;
				if (cm == null) continue;
                foreach (Connector c in cm.Connectors)
                {
                    foreach (Connector cref in c.AllRefs)
                    {
                        Element e = cref.Owner;
                        if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                        {
                            FamilyInstance instance = e as FamilyInstance;
                            if (instance.Symbol.Family.Name.Contains("Übergang"))
                            {
                                _uiDocument.Selection.SetElementIds(new List<ElementId>() { fi.Id });
                                _uiDocument.ShowElements(fi.Id);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void ExtractPlumbingFixtures()
        {
			Document doc = _uiDocument.Document;
			List<FamilyInstance> fixtures = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures).ToList();
			List<Space> spaces = new FilteredElementCollector(doc).OfCategoryId(new ElementId((int)BuiltInCategory.OST_MEPSpaces)).Select(e => e as Space).ToList();
			List<FamilyInstance> usedFixtures = new List<FamilyInstance>();
			List<Space> orderedSpaces = spaces.OrderBy(e => e.Number).ToList();

			Dictionary<string, List<FamilyInstance>> list = new Dictionary<string, List<FamilyInstance>>();

			foreach (Space space in orderedSpaces)
			{
				foreach (FamilyInstance fi in fixtures)
				{
					LocationPoint lp = fi.Location as LocationPoint;
					XYZ point = lp.Point;
					if (space.IsPointInSpace(point))
					{
						if (list.ContainsKey(space.Name))
						{
							list[space.Name].Add(fi);
							usedFixtures.Add(fi);
						}
						else
						{
							list.Add(space.Name, new List<FamilyInstance>() { fi });
							usedFixtures.Add(fi);
						}
					}
				}
			}
			List<FamilyInstance> result = fixtures.Where(e => !usedFixtures.Contains(e)).ToList();
			TaskDialog.Show("Info", "Unassigned elements: " + result.Count.ToString());

			string text = "Room Name & No.;ElementId;FamilyName;FamilyType;SC_S90DurchflussKalt;SC_S90DurchflussWarm;SC_S90MindestfließdruckKalt;SC_S90MindestfließdruckWarm;SC_S90Dauerverbraucher;SC_S90Mindestfließdruck;SC_S90Durchfluss" + Environment.NewLine;
			foreach (KeyValuePair<string, List<FamilyInstance>> pair in list)
			{
				//text += pair.Key  + Environment.NewLine;
				foreach (FamilyInstance fi in pair.Value)
				{
					Parameter kaltFlow = fi.get_Parameter(new Guid("01c3713a-9ea8-45b9-b887-aedad8e42ac5"));
					Parameter warmFlow = fi.get_Parameter(new Guid("ee272d98-a8fd-432e-bf69-7e5a5fab3c5c"));
					Parameter kaltPres = fi.get_Parameter(new Guid("711ff1d3-b377-4914-9199-2c03a715a412"));
					Parameter warmPres = fi.get_Parameter(new Guid("2bf5f13e-bfee-4f00-ab2a-7abe43dec00b"));
					Parameter dauerVerbraucher = fi.get_Parameter(new Guid("6ff8aefe-d5c4-4eba-ab59-a108e4f0e3e7"));
					Parameter pdruck = fi.get_Parameter(new Guid("ce08b3f9-8ebf-4fbd-b17e-b34cb05403ae"));
					Parameter durschfluss = fi.get_Parameter(new Guid("563f59ae-5587-44bb-a343-55ba4dd75449"));
					string kalt = "NA";
					string warm = "NA";
					string kaltPre = "NA";
					string warmPre = "NA";
					string dauerVer = "NA";
					string druck = "NA";
					string durschflussStr = "NA";
					if (kaltFlow != null) kalt = kaltFlow.AsValueString();
					if (warmFlow != null) warm = warmFlow.AsValueString();
					if (kaltPres != null) kaltPre = kaltPres.AsValueString();
					if (warmPres != null) warmPre = warmPres.AsValueString();
					if (dauerVerbraucher != null) dauerVer = dauerVerbraucher.AsValueString();
					if (pdruck != null) druck = pdruck.AsValueString();
					if (durschfluss != null) durschflussStr = durschfluss.AsValueString();
					text += pair.Key + ";" + fi.Id.IntegerValue.ToString() + ";" + fi.Symbol.Family.Name + ";" + fi.Symbol.Name + ";" + kalt + ";" + warm + ";" + kaltPre + ";" + warmPre + ";" + dauerVer + ";" + druck + ";" + durschflussStr + Environment.NewLine;
				}
			}

			//text += "Room unassigned:" + Environment.NewLine;

			foreach (FamilyInstance fi in result)
			{
				Parameter kaltFlow = fi.get_Parameter(new Guid("01c3713a-9ea8-45b9-b887-aedad8e42ac5"));
				Parameter warmFlow = fi.get_Parameter(new Guid("ee272d98-a8fd-432e-bf69-7e5a5fab3c5c"));
				Parameter kaltPres = fi.get_Parameter(new Guid("711ff1d3-b377-4914-9199-2c03a715a412"));
				Parameter warmPres = fi.get_Parameter(new Guid("2bf5f13e-bfee-4f00-ab2a-7abe43dec00b"));
				Parameter dauerVerbraucher = fi.get_Parameter(new Guid("6ff8aefe-d5c4-4eba-ab59-a108e4f0e3e7"));
				Parameter pdruck = fi.get_Parameter(new Guid("ce08b3f9-8ebf-4fbd-b17e-b34cb05403ae"));
				Parameter durschfluss = fi.get_Parameter(new Guid("563f59ae-5587-44bb-a343-55ba4dd75449"));
				string kalt = "NA";
				string warm = "NA";
				string kaltPre = "NA";
				string warmPre = "NA";
				string dauerVer = "NA";
				string druck = "NA";
				string durschflussStr = "NA";
				if (kaltFlow != null) kalt = kaltFlow.AsValueString();
				if (warmFlow != null) warm = warmFlow.AsValueString();
				if (kaltPres != null) kaltPre = kaltPres.AsValueString();
				if (warmPres != null) warmPre = warmPres.AsValueString();
				if (dauerVerbraucher != null) dauerVer = dauerVerbraucher.AsValueString();
				if (pdruck != null) druck = pdruck.AsValueString();
				if (durschfluss != null) durschflussStr = durschfluss.AsValueString();
				text += "Room unassigned" + ";" + fi.Id.IntegerValue.ToString() + ";" + fi.Symbol.Family.Name + ";" + fi.Symbol.Name + ";" + kalt + ";" + warm + ";" + kaltPre + ";" + warmPre + ";" + dauerVer + ";" + druck + ";" + durschflussStr + Environment.NewLine;
			}

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
			string name = "EntnahmeArmaturExtract" + date;
			dlg.FileName = name; // Default file name
			dlg.DefaultExt = ".csv"; // Default file extension
			string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			dlg.InitialDirectory = initialDirectory;
			Nullable<bool> dialogResult = dlg.ShowDialog();
			if (dialogResult == true)
			{
				string filename = dlg.FileName;
				Encoding encoding = Encoding.UTF8;
				File.WriteAllText(filename, text, encoding);
			}

			//string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			//string path = Path.Combine(desktop, "extract_fixtures.csv");
			//Encoding encoding = Encoding.UTF8;
			//File.WriteAllText(path, text, encoding);
		}

		public void SelectFromClipboard()
        {
			string clipboard = System.Windows.Forms.Clipboard.GetText();
			string[] lines = clipboard.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			List<ElementId> ids = new List<ElementId>();
			foreach (string line in lines)
			{
				int convert;
				bool result = Int32.TryParse(line, out convert);
				if (result)
				{
					ElementId id = new ElementId(convert);
					ids.Add(id);
				}
			}
			if (ids.Count == 0)
			{
				TaskDialog.Show("Info", "Keine ID gefunden in clipboard");
				return;
			}
			_uiDocument.Selection.SetElementIds(ids);
		}

		public void ConnectDisconnected()
        {
			Document doc = _uiDocument.Document;
			Reference reference = _uiDocument.Selection.PickObject(ObjectType.Element);
			Element element = doc.GetElement(reference);

			List<Connector> unusedConnectors = new List<Connector>();

			List<Pipe> pipes = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(Pipe)).Select(e => e as Pipe).ToList();
			foreach (Pipe p in pipes)
			{
				ConnectorSet cset = p.ConnectorManager.UnusedConnectors;
				foreach (Connector c in cset)
				{
					unusedConnectors.Add(c);
				}
			}

			List<FamilyInstance> instances = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance).ToList();
			foreach (FamilyInstance fi in instances)
			{
				if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
				{
					ConnectorManager cm = (fi.MEPModel as MechanicalFitting).ConnectorManager;
					if (cm == null) continue;
					ConnectorSet cset = cm.UnusedConnectors;
					foreach (Connector c in cset)
					{
						unusedConnectors.Add(c);
					}
				}

				if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
				{
					ConnectorManager cm = fi.MEPModel.ConnectorManager;
					if (cm == null) continue;
					ConnectorSet cset = cm.UnusedConnectors;
					foreach (Connector c in cset)
					{
						unusedConnectors.Add(c);
					}
				}

				if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment)
				{
					ConnectorManager cm = fi.MEPModel.ConnectorManager;
					if (cm == null) continue;
					ConnectorSet cset = cm.UnusedConnectors;
					foreach (Connector c in cset)
					{
						unusedConnectors.Add(c);
					}
				}

				if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures)
				{
					ConnectorManager cm = fi.MEPModel.ConnectorManager;
					if (cm == null) continue;
					ConnectorSet cset = cm.UnusedConnectors;
					foreach (Connector c in cset)
					{
						unusedConnectors.Add(c);
					}
				}
			}

			switch (element.Category.Id.IntegerValue)
			{
				case (int)BuiltInCategory.OST_PipeCurves:
					ConnectPipe(doc, element, unusedConnectors);
					break;

				case (int)BuiltInCategory.OST_PipeFitting:
					ConnectFitting(doc, element, unusedConnectors);
					break;

				case (int)BuiltInCategory.OST_PipeAccessory:
					ConnectAccessory(doc, element, unusedConnectors);
					break;

				case (int)BuiltInCategory.OST_MechanicalEquipment:
					ConnectMeEq(doc, element, unusedConnectors);
					break;

				case (int)BuiltInCategory.OST_PlumbingFixtures:
					ConnectPlFi(doc, element, unusedConnectors);
					break;
				default:
					TaskDialog.Show("Info", "Element has 0 connectors!");
					break;
			}
		}

		private void ConnectPipe(Document doc, Element pipe, List<Connector> unconnectedList)
		{
			ConnectorSet cset = (pipe as Pipe).ConnectorManager.UnusedConnectors;
			if (cset.Size == 0)
			{
				TaskDialog.Show("Info", "Pipe has 0 opened connectors!");
				return;
			}

			foreach (Connector c1 in cset)
			{
				foreach (Connector c2 in unconnectedList)
				{
					if (c1.Id == c2.Id) continue;
					if (c1.Origin.DistanceTo(c2.Origin) < UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_MILLIMETERS))
					{
						Transaction tx = new Transaction(doc, "Connecting elements");
						tx.Start();
						c1.ConnectTo(c2);
						tx.Commit();
						return;
					}
				}
			}
			TaskDialog.Show("Info", "Can't find the right connector anywhere near!");
		}

		private void ConnectFitting(Document doc, Element fitting, List<Connector> unconnectedList)
		{
			ConnectorManager cm = ((fitting as FamilyInstance).MEPModel as MechanicalFitting).ConnectorManager;
			if (cm == null) return;
			ConnectorSet cset = cm.UnusedConnectors;
			if (cset.Size == 0)
			{
				TaskDialog.Show("Info", "Pipe fitting has 0 opened connectors!");
				return;
			}

			foreach (Connector c1 in cset)
			{
				foreach (Connector c2 in unconnectedList)
				{
					if (c1.Id == c2.Id) continue;
					if (c1.Origin.DistanceTo(c2.Origin) < UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_MILLIMETERS))
					{
						Transaction tx = new Transaction(doc, "Connecting elements");
						tx.Start();
						c1.ConnectTo(c2);
						tx.Commit();
						return;
					}
				}
			}
			TaskDialog.Show("Info", "Can't find the right connector anywhere near!");
		}

		private void ConnectAccessory(Document doc, Element accessory, List<Connector> unconnectedList)
		{
			ConnectorManager cm = (accessory as FamilyInstance).MEPModel.ConnectorManager;
			if (cm == null) return;
			ConnectorSet cset = cm.UnusedConnectors;
			if (cset.Size == 0)
			{
				TaskDialog.Show("Info", "Pipe accessory has 0 opened connectors!");
				return;
			}

			foreach (Connector c1 in cset)
			{
				foreach (Connector c2 in unconnectedList)
				{
					if (c1.Id == c2.Id) continue;
					if (c1.Origin.DistanceTo(c2.Origin) < UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_MILLIMETERS))
					{
						Transaction tx = new Transaction(doc, "Connecting elements");
						tx.Start();
						c1.ConnectTo(c2);
						tx.Commit();
						return;
					}
				}
			}
			TaskDialog.Show("Info", "Can't find the right connector anywhere near!");
		}

		private void ConnectMeEq(Document doc, Element me, List<Connector> unconnectedList)
		{
			ConnectorManager cm = (me as FamilyInstance).MEPModel.ConnectorManager;
			if (cm == null) return;
			ConnectorSet cset = cm.UnusedConnectors;
			if (cset.Size == 0)
			{
				TaskDialog.Show("Info", "Mechanical equipment has 0 opened connectors!");
				return;
			}

			foreach (Connector c1 in cset)
			{
				foreach (Connector c2 in unconnectedList)
				{
					if (c1.Id == c2.Id) continue;
					if (c1.Origin.DistanceTo(c2.Origin) < UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_MILLIMETERS))
					{
						Transaction tx = new Transaction(doc, "Connecting elements");
						tx.Start();
						c1.ConnectTo(c2);
						tx.Commit();
						return;
					}
				}
			}
			TaskDialog.Show("Info", "Can't find the right connector anywhere near!");
		}

		private void ConnectPlFi(Document doc, Element pf, List<Connector> unconnectedList)
		{
			ConnectorManager cm = (pf as FamilyInstance).MEPModel.ConnectorManager;
			if (cm == null) return;
			ConnectorSet cset = cm.UnusedConnectors;
			if (cset.Size == 0)
			{
				TaskDialog.Show("Info", "Plumbing fixture has 0 opened connectors!");
				return;
			}

			foreach (Connector c1 in cset)
			{
				foreach (Connector c2 in unconnectedList)
				{
					if (c1.Id == c2.Id) continue;
					if (c1.Origin.DistanceTo(c2.Origin) < UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_MILLIMETERS))
					{
						Transaction tx = new Transaction(doc, "Connecting elements");
						tx.Start();
						c1.ConnectTo(c2);
						tx.Commit();
						return;
					}
				}
			}
			TaskDialog.Show("Info", "Can't find the right connector anywhere near!");
		}
	}
}
