using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GtbTools.Forms;
using ViewModels;

namespace GtbTools
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class App : IExternalApplication
    {
        #region Configuration and assembly version
#if DEBUG2018 || RELEASE2018 
        public const string AssemblyYear = "2018";
#elif DEBUG2019 || RELEASE2019
        public const string AssemblyYear = "2019";
#elif DEBUG2020 || RELEASE2020
        public const string AssemblyYear = "2020";
#elif DEBUG2021 || RELEASE2021
        public const string AssemblyYear = "2021";
#endif
        public const string AssemblyMinorVersion = "3";
        public const string AssemblyBuildVersion = "8";
        public const string AssemblyRevisionVersion = "2";
        #endregion

        public static string PlugInVersion
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}",
                                AssemblyYear,
                                AssemblyMinorVersion,
                                AssemblyBuildVersion,
                                AssemblyRevisionVersion);
            }
        }       

        public static string ExecutingAssemblyPath { get { return Assembly.GetExecutingAssembly().Location; } }
        public ErrorLog ErrorLog { get; set; }

        //References to some functions are fetched to dockpanel when it is registered, further methods are called by events
        public DurchbruchMemoryViewModel DurchbruchMemoryViewModel { get; set; }
        public Functions.DurchbruchRotationFix DurchbruchRotationFix { get; set; }
        public Functions.RevitOpenedViews RevitOpenedViews { get; set; }
        public Functions.CopyParameterFromHost CopyParameterFromHost { get; set; }
        public Functions.CuttingElementSearch CuttingElementSearch { get; set; }
        public Functions.PipeFlowTagger PipeFlowTagger { get; set; }
        public Functions.VentileFix VentileFix { get; set; }
        public Functions.PipesInWallSearch PipesInWallSearch { get; set; }
        public Functions.SystemTypeChanger SystemTypeChanger { get; set; }
        public Functions.ExternalLinkTool ExternalLinkTool { get; set; }
        public Functions.CalculationSupport CalculationSupport { get; set; }

        //fields to cotnrol show hide button and dock panel visibility
        RibbonItem _button;
        ExternalEvent _toggleEvent;

        //Provides access to current UIControlledApplication instance
        internal static App _app = null;
        public static App Instance
        {
            get { return _app; }
        }

        public Result OnStartup(UIControlledApplication application)
        {
            _app = this;
            ErrorLog = new ErrorLog(PlugInVersion);
            DurchbruchMemoryViewModel = new DurchbruchMemoryViewModel();
            DurchbruchRotationFix = new Functions.DurchbruchRotationFix();
            RevitOpenedViews = new Functions.RevitOpenedViews();
            CopyParameterFromHost = new Functions.CopyParameterFromHost();
            CuttingElementSearch = new Functions.CuttingElementSearch();
            PipeFlowTagger = new Functions.PipeFlowTagger();
            VentileFix = new Functions.VentileFix();
            PipesInWallSearch = new Functions.PipesInWallSearch();
            SystemTypeChanger = new Functions.SystemTypeChanger();
            ExternalLinkTool = new Functions.ExternalLinkTool();
            CalculationSupport = new Functions.CalculationSupport();
            string path = Assembly.GetExecutingAssembly().Location;

            //Creating ribbon in Add-ins tab
            RibbonPanel gtbPanel = application.CreateRibbonPanel("GTB - Berlin");

            //Pushbutton to hide show dockpanel
            PushButtonData pushButtonGtbPanelControl = new PushButtonData( "GTB", "Anzeigen", path, "GtbTools.ShowHideDock");
            pushButtonGtbPanelControl.LargeImage = GetEmbeddedImage("Resources.GtbInactive.png");
            _button = gtbPanel.AddItem(pushButtonGtbPanelControl);

            //Pushbutton to start familyedit window
            PushButtonData pushButtonFamilyEdit = new PushButtonData("Family Edit", "Family Edit", path, "GtbTools.FamilyEditButton");
            pushButtonFamilyEdit.LargeImage = GetEmbeddedImage("Resources.FamilyEdit.png");
            gtbPanel.AddItem(pushButtonFamilyEdit);

            RegisterDockableWindow(application);

            //creating external event to change button state and text
            IExternalEventHandler handler_event = new ExternalEventShowHideDock();
            _toggleEvent = ExternalEvent.Create(handler_event);            

            //when user closes dock panel using "x", button state and text must be changed
            application.DockableFrameVisibilityChanged += OnDockableFrameVisibilityChanged;
            //when documents is loaded dockpanel may be visible or not therefore button state and text must be also aligned with current visibility
            application.ViewActivated += OnDockableFrameVisibilityChanged;
            
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            ErrorLog.DeleteLog();
            return Result.Succeeded;
        }

        /// <summary>
        /// Method to show hide dock panel. Button state and text are changed by event
        /// </summary>
        public void Toggle(UIApplication application)
        {
            if(_button.ItemText == "Anzeigen")
            {
                ShowDockableWindow(application);
            }
            else
            {
                HideDockableWindow(application);
            }
        }

        /// <summary>
        /// Aligns dockpanel button state and text with dockpanel current visibility
        /// </summary>
        public void SwitchDockPanelButton(UIApplication application)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{9F702FC8-EC07-4A80-846F-04AFA5AC8820}"));
            DockablePane dp;

            //When user cancels loading document DockablePaneId is invalid
            if (DockablePane.PaneExists(dpid))
            {
                dp = application.GetDockablePane(dpid);
            }
            else
            {
                return;
            }

            if (dp.IsShown() && _button.ItemText == "Anzeigen")
            {
                _button.ItemText = "Ausblenden";
                PushButton pb = _button as PushButton;
                pb.LargeImage = GetEmbeddedImage("Resources.GtbActive.png");
            }
            if (!dp.IsShown() && _button.ItemText == "Ausblenden")
            {
                _button.ItemText = "Anzeigen";
                PushButton pb = _button as PushButton;
                pb.LargeImage = GetEmbeddedImage("Resources.GtbInactive.png");
            }
        }

        /// <summary>
        /// Event to raise iexternal event
        /// </summary>
        private void OnDockableFrameVisibilityChanged(object sender, EventArgs e)
        {
            _toggleEvent.Raise();
        }

        /// <summary>
        /// Registering dock panel, creating events and asigning events to functions
        /// </summary>
        private void RegisterDockableWindow(UIControlledApplication app)
        {
            IExternalEventHandler handler_event = new ExternalEventApplyCoordsToViews();
            ExternalEvent exEvent = ExternalEvent.Create(handler_event);

            IExternalEventHandler handler_event2 = new ExternalEventOpenViews();
            ExternalEvent exEvent2 = ExternalEvent.Create(handler_event2);

            IExternalEventHandler handler_event3 = new ExternalEventSaveCoords();
            ExternalEvent exEvent3 = ExternalEvent.Create(handler_event3);

            IExternalEventHandler handler_event4 = new ExternalEventOpenCoords();
            ExternalEvent exEvent4 = ExternalEvent.Create(handler_event4);

            IExternalEventHandler handler_event5 = new ExternalEventExcelDataImporter();
            ExternalEvent exEvent5 = ExternalEvent.Create(handler_event5);

            IExternalEventHandler handler_event6 = new ExternalEventSymbolHandler();
            ExternalEvent exEvent6 = ExternalEvent.Create(handler_event6);

            IExternalEventHandler handler_event7 = new ExternalEventTagAllOpenings();
            ExternalEvent exEvent7 = ExternalEvent.Create(handler_event7);

            IExternalEventHandler handler_event8 = new ExternalEventUpdateViewModel();
            ExternalEvent exEvent8 = ExternalEvent.Create(handler_event8);

            IExternalEventHandler showElementIExEventHandler = new ExternalEventShowElement();
            ExternalEvent showElementEventHandler = ExternalEvent.Create(showElementIExEventHandler);

            IExternalEventHandler openViewExEventHandler = new ExternalEventShowOnView();
            ExternalEvent openViewEventHandler = ExternalEvent.Create(openViewExEventHandler);

            IExternalEventHandler saveDataExEventHandler = new ExternalEventSaveData();
            ExternalEvent saveDataEventHandler = ExternalEvent.Create(saveDataExEventHandler);

            IExternalEventHandler changeValue1ExEventHandler = new ExternalEventChangeDurchbruchDiameter();
            ExternalEvent changeValueEvent1 = ExternalEvent.Create(changeValue1ExEventHandler);

            IExternalEventHandler changeValue2ExEventHandler = new ExternalEventChangeDurchbruchOffset();
            ExternalEvent changeValueEvent2 = ExternalEvent.Create(changeValue2ExEventHandler);

            DurchbruchMemoryViewModel.SetExternalEvents(exEvent8, showElementEventHandler, openViewEventHandler, saveDataEventHandler, changeValueEvent1, changeValueEvent2);

            IExternalEventHandler handler_event9 = new ExternalEventCutOpeningMemory();
            ExternalEvent exEvent9 = ExternalEvent.Create(handler_event9);

            IExternalEventHandler handler_event10 = new ExternalEventMepExtract();
            ExternalEvent exEvent10 = ExternalEvent.Create(handler_event10);

            IExternalEventHandler fixRotationEventHandler = new ExternalEventFixRotationIssue();
            ExternalEvent fixRotationExEvent = ExternalEvent.Create(fixRotationEventHandler);

            IExternalEventHandler handler_event11 = new ExternalEventCopyElevations();
            ExternalEvent exEvent11 = ExternalEvent.Create(handler_event11);

            DurchbruchRotationFix.SetExternalEvents(fixRotationExEvent);

            IExternalEventHandler handler12 = new ExternalEventRevitOpenedViews();
            ExternalEvent exEvent12 = ExternalEvent.Create(handler12);
            RevitOpenedViews.SetEvent(exEvent12);

            IExternalEventHandler eventHandlerCopyParameter = new ExternalEventCopyParameter();
            ExternalEvent exEventCopyParameter = ExternalEvent.Create(eventHandlerCopyParameter);
            CopyParameterFromHost.SetEvents(exEventCopyParameter);

            IExternalEventHandler eventHandlerFixDiameter = new ExternalEventFixDiameter();
            ExternalEvent exEventFixDiameter = ExternalEvent.Create(eventHandlerFixDiameter);
            CuttingElementSearch.SetEvent(exEventFixDiameter);

            IExternalEventHandler evHandlerAnnotateStacks = new ExternalEventAnnotateStacks();
            ExternalEvent exEventAnnotateStacks = ExternalEvent.Create(evHandlerAnnotateStacks);
            PipeFlowTagger.SetEvent(exEventAnnotateStacks);

            IExternalEventHandler raumbuch = new ExternalEventRaumBuch();
            ExternalEvent raumbuchExEvent = ExternalEvent.Create(raumbuch);

            IExternalEventHandler ventileFix = new ExEventVentileFix();
            ExternalEvent ventileFixEvent = ExternalEvent.Create(ventileFix);
            VentileFix.SetStartEvent(ventileFixEvent);

            IExternalEventHandler rotateElements = new ExEventRotateElements();
            ExternalEvent rotateElementsExEvent = ExternalEvent.Create(rotateElements);

            IExternalEventHandler pipesInWallSearch = new ExEventPipesInWall();
            ExternalEvent pipiesInWallExEvent = ExternalEvent.Create(pipesInWallSearch);
            PipesInWallSearch.SetEvent(pipiesInWallExEvent);

            IExternalEventHandler forceConnection = new ExEventForceConnection();
            ExternalEvent forceConnectionEvent = ExternalEvent.Create(forceConnection);

            IExternalEventHandler systemTypeChange = new ExEventPipingSystem();
            ExternalEvent pipingSystemExEvent = ExternalEvent.Create(systemTypeChange);
            SystemTypeChanger.SetEvent(pipingSystemExEvent);

            IExternalEventHandler externalLinks = new ExEventExternalLinks();
            ExternalEvent externalLinksExEvent = ExternalEvent.Create(externalLinks);
            ExternalLinkTool.SetEvent(externalLinksExEvent);

            IExternalEventHandler calculationSupport = new ExEventCalculationSupport();
            ExternalEvent exEventCalculationSupport = ExternalEvent.Create(calculationSupport);
            CalculationSupport.SetEvent(exEventCalculationSupport);

            GtbDockPage GtbDockableWindow = new GtbDockPage(PlugInVersion, exEvent, exEvent2, exEvent3, exEvent4, exEvent5, exEvent6, exEvent7, 
                                                                DurchbruchMemoryViewModel, exEvent9, exEvent10, DurchbruchRotationFix, exEvent11,
                                                                    RevitOpenedViews, CopyParameterFromHost, CuttingElementSearch, PipeFlowTagger,
                                                                        raumbuchExEvent, VentileFix, rotateElementsExEvent, PipesInWallSearch, forceConnectionEvent, SystemTypeChanger,
                                                                        ExternalLinkTool, CalculationSupport);

            DockablePaneId dpid = new DockablePaneId(new Guid("{9F702FC8-EC07-4A80-846F-04AFA5AC8820}"));
            
            app.RegisterDockablePane(dpid, "GTB-Berlin", GtbDockableWindow as IDockablePaneProvider);
        }

        /// <summary>
        /// Shows dock panel
        /// </summary>
        private void ShowDockableWindow(UIApplication application)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{9F702FC8-EC07-4A80-846F-04AFA5AC8820}"));
            DockablePane dp = application.GetDockablePane(dpid);
            
            dp.Show();
        }

        /// <summary>
        /// Hides dock panel
        /// </summary>
        private void HideDockableWindow(UIApplication application)
        {
            DockablePaneId dpid = new DockablePaneId(new Guid("{9F702FC8-EC07-4A80-846F-04AFA5AC8820}"));
            DockablePane dp = application.GetDockablePane(dpid);
            dp.Hide();
        }

        /// <summary>
        /// Gets bitmap source from embedded image
        /// </summary>
        private BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(name);
                return BitmapFrame.Create(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
