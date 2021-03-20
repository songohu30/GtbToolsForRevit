using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.UI;
using ViewModels;
using System.Threading;
using GUI;
using System.Windows.Threading;
using Functions;
using CuttingElementTool;
using ExternalLinkControl;

namespace GtbTools.Forms
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GtbDockPage : Page, Autodesk.Revit.UI.IDockablePaneProvider
    {
        public DurchbruchMemoryViewModel DurchbruchMemoryViewModel { get; set; }
        Functions.DurchbruchRotationFix DurchbruchRotationFix { get; set; }
        Functions.CopyParameterFromHost CopyParameterFromHost { get; set; }

        #region Data
        ExternalEvent _exEventCopyCoords;
        ExternalEvent _exEventOpenViews;
        ExternalEvent _exEventSaveCoords;
        ExternalEvent _exEventLoadCoords;
        ExternalEvent _exEventExcel;
        ExternalEvent _exEventSymbols;
        ExternalEvent _tagAllOpenings;
        ExternalEvent _cutOpeningMemory;
        ExternalEvent _mepExtract;
        ExternalEvent _copyElevations;
        ExternalEvent _raumbuchExEvent;
        ExternalEvent _rotateElements;
        RevitOpenedViews _revitOpenedViews;
        CuttingElementSearch _cuttingElementSearch;
        PipeFlowTagger _pipeFlowTagger;
        VentileFix _ventileFix;
        PipesInWallSearch _pipesInWallSearch;
        ExternalEvent _forceConnection;
        SystemTypeChanger _systemTypeChanger;
        ExternalLinkTool _externalLinkTool;
        CalculationSupport _calculationSupport;

        //private Guid m_targetGuid;
        //private DockPosition m_position = DockPosition.Bottom;
        //private int m_left = 1;
        //private int m_right = 1;
        //private int m_top = 1;
        //private int m_bottom = 1;
        #endregion
        public GtbDockPage(string plugInVersion, ExternalEvent exEventCopyCoords, ExternalEvent exEventOpenViews,
                            ExternalEvent exEventSaveCoords, ExternalEvent exEventLoadCoords, ExternalEvent exEventExcel,
                                ExternalEvent exEventSymbols, ExternalEvent tagAllOpenings, DurchbruchMemoryViewModel durchbruchMemoryViewModel,
                                    ExternalEvent cutOpeningMemory, ExternalEvent mepExtract, Functions.DurchbruchRotationFix rotationFix,
                                        ExternalEvent copyElevations, RevitOpenedViews revitOpenedViews, CopyParameterFromHost copyParameterFromHost,
                                        CuttingElementSearch cuttingElementSearch, PipeFlowTagger pipeFlowTagger, ExternalEvent raumbuchExEvent, VentileFix ventileFix,
                                            ExternalEvent rotateElements, PipesInWallSearch pipesInWallSearch, ExternalEvent forceConnection, SystemTypeChanger systemTypeChanger,
                                                ExternalLinkTool externalLinkTool, CalculationSupport calculationSupport)
        {
            _exEventCopyCoords = exEventCopyCoords;
            _exEventOpenViews = exEventOpenViews;
            _exEventLoadCoords = exEventLoadCoords;
            _exEventSaveCoords = exEventSaveCoords;
            _exEventExcel = exEventExcel;
            _exEventSymbols = exEventSymbols;
            _tagAllOpenings = tagAllOpenings;
            _cutOpeningMemory = cutOpeningMemory;
            _mepExtract = mepExtract;
            DurchbruchRotationFix = rotationFix;
            DurchbruchMemoryViewModel = durchbruchMemoryViewModel;
            _copyElevations = copyElevations;
            _revitOpenedViews = revitOpenedViews;
            CopyParameterFromHost = copyParameterFromHost;
            _cuttingElementSearch = cuttingElementSearch;
            _pipeFlowTagger = pipeFlowTagger;
            _raumbuchExEvent = raumbuchExEvent;
            _ventileFix = ventileFix;
            _rotateElements = rotateElements;
            _pipesInWallSearch = pipesInWallSearch;
            _forceConnection = forceConnection;
            _systemTypeChanger = systemTypeChanger;
            _externalLinkTool = externalLinkTool;
            _calculationSupport = calculationSupport;
                 
            InitializeComponent();
            LblVersion.Content += plugInVersion;
        }
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this as FrameworkElement;
            data.InitialState = new Autodesk.Revit.UI.DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Floating;
            data.InitialState.SetFloatingRectangle(new Autodesk.Revit.DB.Rectangle(100, 100, 360, 540));
        }

        private void DockableDialogs_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_OpenViews(object sender, RoutedEventArgs e)
        {
            _exEventOpenViews.Raise();
        }

        private void Button_Click_CopyCoords(object sender, RoutedEventArgs e)
        {
            _exEventCopyCoords.Raise();
        }

        private void Button_Click_SaveCoords(object sender, RoutedEventArgs e)
        {
            _exEventSaveCoords.Raise();
        }

        private void Button_Click_LoadCoords(object sender, RoutedEventArgs e)
        {
            _exEventLoadCoords.Raise();
        }

        private void Button_Click_ExcelDataImport(object sender, RoutedEventArgs e)
        {
            _exEventExcel.Raise();
        }

        private void SymbolMainWindow_Click(object sender, RoutedEventArgs e)
        {
            _exEventSymbols.Raise();
        }

        private void SelectAllFloor_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Click_TagAllOpenings(object sender, RoutedEventArgs e)
        {
            _tagAllOpenings.Raise();
        }

        private void SelectAllRoof_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _mepExtract.Raise();
        }

        private void CheckMemory_Click(object sender, RoutedEventArgs e)
        {
            _cutOpeningMemory.Raise();
        }

        private void ContextRefreshTest_Click(object sender, RoutedEventArgs e)
        {
            Thread windowThread = new Thread(delegate ()
            {
                DurchbruchMemoryViewModel.LoadContextEvent.Raise();
                DurchbruchMemoryViewModel.SignalEvent.WaitOne();
                DurchbruchMemoryViewModel.SignalEvent.Reset();
                if (DurchbruchMemoryViewModel.OptimisationChoice == OptimisationChoice.None) return;
                DurchbruchMemoryWindow durchbruchMemoryWindow = new DurchbruchMemoryWindow(DurchbruchMemoryViewModel);
                durchbruchMemoryWindow.Show();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void Btn_Click_GoToTestDir(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"H:\Revit\Makros\Umsetzung\Durchbruch Symbolen");
        }

        private void RotationFixButton_Click(object sender, RoutedEventArgs e)
        {
            Thread windowThread = new Thread(delegate ()
            {
                DurchbruchRotationFix.FixRotationEvent.Raise();
                //DurchbruchRotationFix.SignalEvent.WaitOne();
                //DurchbruchRotationFix.SignalEvent.Reset();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        private void CopyElevations_Click(object sender, RoutedEventArgs e)
        {
            _copyElevations.Raise();
        }

        private void Btn_Click_SaveAllOpenedViews(object sender, RoutedEventArgs e)
        {
            _revitOpenedViews.IsSaving = true;
            _revitOpenedViews.IsLoading = false;
            _revitOpenedViews.OneEvent.Raise();
        }

        private void Btn_Click_LoadAllSaved(object sender, RoutedEventArgs e)
        {
            _revitOpenedViews.IsLoading = true;
            _revitOpenedViews.IsSaving = false;
            _revitOpenedViews.OneEvent.Raise();
        }

        private void CopyPasteParameters_Click(object sender, RoutedEventArgs e)
        {
            CopyParameterFromHost.InitializeEvent.Raise();
        }

        private void FixDiameterBtn_Click(object sender, RoutedEventArgs e)
        {
            _cuttingElementSearch.ToolAction = CutElementToolAction.Initialize;
            _cuttingElementSearch.TheEvent.Raise();
        }

        private void Btn_Click_AnnotateVerticalStacks(object sender, RoutedEventArgs e)
        {
            _pipeFlowTagger.Action = PipeFlowTool.PipeFlowToolAction.Initialize;
            _pipeFlowTagger.StartEvent.Raise();
        }

        private void Btn_Click_AddTagHolder(object sender, RoutedEventArgs e)
        {
            _pipeFlowTagger.Action = PipeFlowTool.PipeFlowToolAction.InsertTagHolder;
            _pipeFlowTagger.StartEvent.Raise();
        }

        private void Btn_Click_AokRaumbuch(object sender, RoutedEventArgs e)
        {
            _raumbuchExEvent.Raise();
        }

        private void Btn_Click_VentileFix(object sender, RoutedEventArgs e)
        {
            _ventileFix.Action = VentileSizeFix.VentileFixAction.Initialize;
            _ventileFix.TheEvent.Raise();
        }

        private void Btn_Click_RotateElements(object sender, RoutedEventArgs e)
        {
            _rotateElements.Raise();
        }

        private void PipesInWallBtn_Click(object sender, RoutedEventArgs e)
        {
            _pipesInWallSearch.Action = PipesInWall.PipesInWallAction.Initialize;
            _pipesInWallSearch.TheEvent.Raise();
        }

        private void Btn_Click_ForceConnection(object sender, RoutedEventArgs e)
        {
            _forceConnection.Raise();
        }

        private void BtnClick_ChangeSystemType(object sender, RoutedEventArgs e)
        {
            _systemTypeChanger.Action = Action.Initialize;
            _systemTypeChanger.TheEvent.Raise();
        }

        private void BtnClick_ExternalLinkTool(object sender, RoutedEventArgs e)
        {
            _externalLinkTool.Action = ExternalLinkToolAction.Initialize;
            _externalLinkTool.TheEvent.Raise();
        }

        private void BtnClick_CalculationSupport(object sender, RoutedEventArgs e)
        {
            _calculationSupport.Action = Calculations.Action.Initialize;
            _calculationSupport.TheEvent.Raise();
        }
    }
}
