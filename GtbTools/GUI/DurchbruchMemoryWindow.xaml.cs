using OwnerSearch;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ViewModels;
using System.Collections.Generic;
using System.Windows.Data;
using Autodesk.Revit.DB;
using System.Windows.Input;

namespace GUI
{
    /// <summary>
    /// Interaction logic for DurchbruchMemoryWindow.xaml
    /// </summary>
    public partial class DurchbruchMemoryWindow : Window
    {
        public DurchbruchMemoryViewModel DurchbruchMemoryViewModel { get; set; }

        public DurchbruchMemoryWindow(DurchbruchMemoryViewModel durchbruchMemoryViewModel)
        {
            SetOwner();
            DurchbruchMemoryViewModel = durchbruchMemoryViewModel;
            InitializeComponent();
            DataGridNew.DataContext = this.DurchbruchMemoryViewModel;
            DataGridMoved.DataContext = this.DurchbruchMemoryViewModel;
            DataGridResized.DataContext = this.DurchbruchMemoryViewModel;
            DataGridMovedAndResized.DataContext = this.DurchbruchMemoryViewModel;
            ComboBoxFilter.ItemsSource = CreateComboBoxFilters();
            SetLabels();
        }

        private void SetDataContext()
        {
            DataGridNew.DataContext = this.DurchbruchMemoryViewModel;
            DataGridMoved.DataContext = this.DurchbruchMemoryViewModel;
            DataGridResized.DataContext = this.DurchbruchMemoryViewModel;
            DataGridMovedAndResized.DataContext = this.DurchbruchMemoryViewModel;
        }

        private List<string> CreateComboBoxFilters()
        {
            List<string> result = new List<string>();
            result.Add("No filter");
            result.Add("Systemtyp");
            result.Add("Feuerklasse");
            result.Add("Opening Mark");
            return result;
        }

        private void SetLabels()
        {
            string newLbl = String.Format("{0} new Durchbruche", DurchbruchMemoryViewModel.NewDurchbruche.Count);
            string movedLbl = String.Format("{0} moved Durchbruche", DurchbruchMemoryViewModel.MovedDurchbruche.Count);
            string resLbl = String.Format("{0} resized Durchbruche", DurchbruchMemoryViewModel.ResizedDurchbruche.Count);
            string movresLbl = String.Format("{0} moved and resized", DurchbruchMemoryViewModel.MovedAndResizedDurchbruche.Count);
            lblNew.Content = newLbl;
            lblMoved.Content = movedLbl;
            lblResized.Content = resLbl;
            lblMovRes.Content = movresLbl;
            if (DurchbruchMemoryViewModel.NewDurchbruche.Count > 0) lblNew.FontWeight = FontWeights.Bold;
            if (DurchbruchMemoryViewModel.MovedDurchbruche.Count > 0) lblMoved.FontWeight = FontWeights.Bold;
            if (DurchbruchMemoryViewModel.ResizedDurchbruche.Count > 0) lblResized.FontWeight = FontWeights.Bold;
            if (DurchbruchMemoryViewModel.MovedAndResizedDurchbruche.Count > 0) lblMovRes.FontWeight = FontWeights.Bold;
        }

        private void SetOwner()
        {
            WindowHandleSearch revitHandleSearch = WindowHandleSearch.MainWindowHandle;
            revitHandleSearch.SetAsOwner(this);
        }

        private void Btn_Click_ClearAll(object sender, RoutedEventArgs e)
        {
            DataGridResized.UnselectAll();
            DataGridNew.UnselectAll();
            DataGridMoved.UnselectAll();
            DataGridMovedAndResized.UnselectAll();
        }

        private void Btn_Click_SaveNew(object sender, RoutedEventArgs e)
        {
            DurchbruchMemoryViewModel.MemorySaveOption = MemorySaveOption.New;
            DurchbruchMemoryViewModel.SaveDataToExStorageEvent.Raise();
            DurchbruchMemoryViewModel.SignalEvent.WaitOne();
            DurchbruchMemoryViewModel.SignalEvent.Reset();
            SetDataContext();
            SetLabels();
        }

        private void Btn_Click_SaveAll(object sender, RoutedEventArgs e)
        {
            DurchbruchMemoryViewModel.MemorySaveOption = MemorySaveOption.All;
            DurchbruchMemoryViewModel.SaveDataToExStorageEvent.Raise();
            DurchbruchMemoryViewModel.SignalEvent.WaitOne();
            DurchbruchMemoryViewModel.SignalEvent.Reset();
            SetDataContext();
            SetLabels();
        }

        private void BtnClick_NewDurchBruchViews(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            NewDurchbruchViewModel sendingClass = (NewDurchbruchViewModel)button.DataContext;
            DurchbruchViews durchbruchViews = new DurchbruchViews(sendingClass.Views, this);
            durchbruchViews.DurchbruchMemoryViewModel = DurchbruchMemoryViewModel;
            durchbruchViews.ShowDialog();
        }

        private void BtnClick_MovedDurchBruchViews(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MovedAndResizedDbViewModel sendingClass = (MovedAndResizedDbViewModel)button.DataContext;
            DurchbruchViews durchbruchViews = new DurchbruchViews(sendingClass.Views, this);
            durchbruchViews.DurchbruchMemoryViewModel = DurchbruchMemoryViewModel;
            durchbruchViews.ShowDialog();
        }

        private void BtnClick_ResizedDurchBruchViews(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ResizedDurchbruchViewModel sendingClass = (ResizedDurchbruchViewModel)button.DataContext;
            DurchbruchViews durchbruchViews = new DurchbruchViews(sendingClass.Views, this);
            durchbruchViews.DurchbruchMemoryViewModel = DurchbruchMemoryViewModel;
            durchbruchViews.ShowDialog();
        }

        private void BtnClick_MovedAndResized(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MovedAndResizedDbViewModel sendingClass = (MovedAndResizedDbViewModel)button.DataContext;
            DurchbruchViews durchbruchViews = new DurchbruchViews(sendingClass.Views, this);
            durchbruchViews.DurchbruchMemoryViewModel = DurchbruchMemoryViewModel;
            durchbruchViews.ShowDialog();
        }

        private void DataGridResized_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResizedDurchbruchViewModel item = (ResizedDurchbruchViewModel)DataGridResized.SelectedItem;
            if (item == null) return;
            DurchbruchMemoryViewModel.CurrentSelection = item.DurchbruchModel.ElementId;
            DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.ShowElement;
            DurchbruchMemoryViewModel.SignalEvent.Set();
            DurchbruchMemoryViewModel.ShowElementEvent.Raise();
        }

        private void DataGridMoved_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MovedAndResizedDbViewModel item = (MovedAndResizedDbViewModel)DataGridMoved.SelectedItem;
            if (item == null) return;
            DurchbruchMemoryViewModel.CurrentSelection = item.DurchbruchModel.ElementId;
            DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.ShowElement;
            DurchbruchMemoryViewModel.SignalEvent.Set();
            DurchbruchMemoryViewModel.ShowElementEvent.Raise();
            //raise event to create a ball or a line
        }

        private void DataGridNew_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NewDurchbruchViewModel item = (NewDurchbruchViewModel)DataGridNew.SelectedItem;
            if (item == null) return;
            DurchbruchMemoryViewModel.CurrentSelection = item.DurchbruchModel.ElementId;
            DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.ShowElement;
            DurchbruchMemoryViewModel.SignalEvent.Set();
            DurchbruchMemoryViewModel.ShowElementEvent.Raise();
        }

        private void DataGridMovedAndResized_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MovedAndResizedDbViewModel item = (MovedAndResizedDbViewModel)DataGridMovedAndResized.SelectedItem;
            if (item == null) return;
            DurchbruchMemoryViewModel.CurrentSelection = item.DurchbruchModel.ElementId;
            DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.ShowElement;
            DurchbruchMemoryViewModel.SignalEvent.Set();
            DurchbruchMemoryViewModel.ShowElementEvent.Raise();
        }

        private void OnChecked(object sender, RoutedEventArgs e)
        {
            DataGridCell item = (DataGridCell)sender;
            MovedAndResizedDbViewModel sendingClass = (MovedAndResizedDbViewModel)item.DataContext;
            DurchbruchMemoryViewModel.CurrentItem = sendingClass;
            DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.ShowPosition;
            DurchbruchMemoryViewModel.SignalEvent.Set();
            DurchbruchMemoryViewModel.ShowElementEvent.Raise();
        }

        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            DataGridCell item = (DataGridCell)sender;
            MovedAndResizedDbViewModel sendingClass = (MovedAndResizedDbViewModel)item.DataContext;
            DurchbruchMemoryViewModel.CurrentItem = sendingClass;
            DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.DeletePosition;
            DurchbruchMemoryViewModel.SignalEvent.Set();
            DurchbruchMemoryViewModel.ShowElementEvent.Raise();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DurchbruchMemoryViewModel.OldPositionMarkers.Count > 0)
            {
                DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.DeleteRemainingMarkers;
                DurchbruchMemoryViewModel.SignalEvent.Set();
                DurchbruchMemoryViewModel.ShowElementEvent.Raise();
            }
        }

        private void DataGridResized_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //MessageBox.Show("Ended editing");
            DataGrid item = (DataGrid)sender;
            ResizedDurchbruchViewModel sendingClass = (ResizedDurchbruchViewModel)item.SelectedItem;
            DurchbruchMemoryViewModel.CurrentModel = sendingClass.DurchbruchModel;
            string id = sendingClass.ElementId;
            string shape = sendingClass.Shape;
            string pipeDiameter = sendingClass.PipeDiameter;
            string offset = sendingClass.Offset;

            //MessageBox.Show(id + ", " + shape + ", " + pipeDiameter + ", " + offset);
            //regex
            //value changed

            double metricOffset = sendingClass.DurchbruchModel.CutOffset.AsDouble() * 304.8;
            string offsetString = metricOffset.ToString("F1", CultureInfo.InvariantCulture);

            if (shape == "Rectangular")
            {
                //MessageBox.Show("Editing is not allowed for rectangular openings!", "Info");
                sendingClass.Offset = offsetString;
                sendingClass.PipeDiameter = "---";
                return;
            }

            double metricPipeDiameter = sendingClass.DurchbruchModel.PipeDiameter.AsDouble() * 304.8;
            string pipeDiameterString = metricPipeDiameter.ToString("F1", CultureInfo.InvariantCulture);

            if (offset != offsetString)
            {
                //MessageBox.Show("Changed offset");
                double number;
                bool result = double.TryParse(offset, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.Offset = offsetString;
                    return;
                }
                if(number < 0)
                {
                    MessageBox.Show("Wert kann nicht kleiner als null sein!");
                    sendingClass.Offset = offsetString;
                }
                else
                {
                    string newOffset = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Offset = newOffset;
                    //calculate new total D in gridtable
                    double diameterNumber = Convert.ToDouble(pipeDiameter, CultureInfo.InvariantCulture);
                    double totalDiameter = diameterNumber + 2 * number;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewOffset;
                    DurchbruchMemoryViewModel.CurrentModel.NewOffset = number;
                    DurchbruchMemoryViewModel.ChangeOffsetEvent.Raise();
                }

            }
            if (pipeDiameter != pipeDiameterString)
            {
                //MessageBox.Show("Changed pipe diameter");
                double number;
                bool result = double.TryParse(pipeDiameter, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.PipeDiameter = pipeDiameterString; ;
                    return;
                }
                if (number > 0)
                {
                    string newPipeDiameter = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.PipeDiameter = newPipeDiameter;
                    //calculate new total D in gridtable
                    double offsetNumber = Convert.ToDouble(offset, CultureInfo.InvariantCulture);
                    double totalDiameter = number + 2 * offsetNumber;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewDiameter;
                    DurchbruchMemoryViewModel.CurrentModel.NewDiameter = number;
                    DurchbruchMemoryViewModel.ChangeDiameterEvent.Raise();
                }
                else
                {
                    MessageBox.Show("Wert muss größer als null sein!");
                    sendingClass.PipeDiameter = pipeDiameterString;
                }
            }
        }

        private void DataGridNew_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //MessageBox.Show("Ended editing");
            DataGrid item = (DataGrid)sender;
            NewDurchbruchViewModel sendingClass = (NewDurchbruchViewModel)item.SelectedItem;
            DurchbruchMemoryViewModel.CurrentModel = sendingClass.DurchbruchModel;
            string id = sendingClass.ElementId;
            string shape = sendingClass.Shape;
            string pipeDiameter = sendingClass.PipeDiameter;
            string offset = sendingClass.Offset;

            //MessageBox.Show(id + ", " + shape + ", " + pipeDiameter + ", " + offset);
            //regex
            //value changed

            double metricOffset = sendingClass.DurchbruchModel.CutOffset.AsDouble() * 304.8;
            string offsetString = metricOffset.ToString("F1", CultureInfo.InvariantCulture);

            if (shape == "Rectangular")
            {
                //MessageBox.Show("Editing is not allowed for rectangular openings!", "Info");
                sendingClass.Offset = offsetString;
                sendingClass.PipeDiameter = "---";
                return;
            }

            double metricPipeDiameter = sendingClass.DurchbruchModel.PipeDiameter.AsDouble() * 304.8;
            string pipeDiameterString = metricPipeDiameter.ToString("F1", CultureInfo.InvariantCulture);

            if (offset != offsetString)
            {
                //MessageBox.Show("Changed offset");
                double number;
                bool result = double.TryParse(offset, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.Offset = offsetString;
                    return;
                }
                if (number < 0)
                {
                    MessageBox.Show("Wert kann nicht kleiner als null sein!");
                    sendingClass.Offset = offsetString;
                }
                else
                {
                    string newOffset = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Offset = newOffset;
                    //calculate new total D in gridtable
                    double diameterNumber = Convert.ToDouble(pipeDiameter, CultureInfo.InvariantCulture);
                    double totalDiameter = diameterNumber + 2 * number;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewOffset;
                    DurchbruchMemoryViewModel.CurrentModel.NewOffset = number;
                    DurchbruchMemoryViewModel.ChangeOffsetEvent.Raise();
                }

            }
            if (pipeDiameter != pipeDiameterString)
            {
                //MessageBox.Show("Changed pipe diameter");
                double number;
                bool result = double.TryParse(pipeDiameter, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.PipeDiameter = pipeDiameterString; ;
                    return;
                }
                if (number > 0)
                {
                    string newPipeDiameter = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.PipeDiameter = newPipeDiameter;
                    //calculate new total D in gridtable
                    double offsetNumber = Convert.ToDouble(offset, CultureInfo.InvariantCulture);
                    double totalDiameter = number + 2 * offsetNumber;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewDiameter;
                    DurchbruchMemoryViewModel.CurrentModel.NewDiameter = number;
                    DurchbruchMemoryViewModel.ChangeDiameterEvent.Raise();
                }
                else
                {
                    MessageBox.Show("Wert muss größer als null sein!");
                    sendingClass.PipeDiameter = pipeDiameterString;
                }
            }
        }

        private void DataGridMoved_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //MessageBox.Show("Ended editing");
            DataGrid item = (DataGrid)sender;
            MovedAndResizedDbViewModel sendingClass = (MovedAndResizedDbViewModel)item.SelectedItem;
            DurchbruchMemoryViewModel.CurrentModel = sendingClass.DurchbruchModel;
            string id = sendingClass.ElementId;
            string shape = sendingClass.Shape;
            string pipeDiameter = sendingClass.PipeDiameter;
            string offset = sendingClass.Offset;

            //MessageBox.Show(id + ", " + shape + ", " + pipeDiameter + ", " + offset);
            //regex
            //value changed

            double metricOffset = sendingClass.DurchbruchModel.CutOffset.AsDouble() * 304.8;
            string offsetString = metricOffset.ToString("F1", CultureInfo.InvariantCulture);

            if (shape == "Rectangular")
            {
                //MessageBox.Show("Editing is not allowed for rectangular openings!", "Info");
                sendingClass.Offset = offsetString;
                sendingClass.PipeDiameter = "---";
                return;
            }

            double metricPipeDiameter = sendingClass.DurchbruchModel.PipeDiameter.AsDouble() * 304.8;
            string pipeDiameterString = metricPipeDiameter.ToString("F1", CultureInfo.InvariantCulture);

            if (offset != offsetString)
            {
                //MessageBox.Show("Changed offset");
                double number;
                bool result = double.TryParse(offset, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.Offset = offsetString;
                    return;
                }
                if (number < 0)
                {
                    MessageBox.Show("Wert kann nicht kleiner als null sein!");
                    sendingClass.Offset = offsetString;
                }
                else
                {
                    string newOffset = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Offset = newOffset;
                    //calculate new total D in gridtable
                    double diameterNumber = Convert.ToDouble(pipeDiameter, CultureInfo.InvariantCulture);
                    double totalDiameter = diameterNumber + 2 * number;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewOffset;
                    DurchbruchMemoryViewModel.CurrentModel.NewOffset = number;
                    DurchbruchMemoryViewModel.ChangeOffsetEvent.Raise();
                }

            }
            if (pipeDiameter != pipeDiameterString)
            {
                //MessageBox.Show("Changed pipe diameter");
                double number;
                bool result = double.TryParse(pipeDiameter, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.PipeDiameter = pipeDiameterString; ;
                    return;
                }
                if (number > 0)
                {
                    string newPipeDiameter = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.PipeDiameter = newPipeDiameter;
                    //calculate new total D in gridtable
                    double offsetNumber = Convert.ToDouble(offset, CultureInfo.InvariantCulture);
                    double totalDiameter = number + 2 * offsetNumber;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewDiameter;
                    DurchbruchMemoryViewModel.CurrentModel.NewDiameter = number;
                    DurchbruchMemoryViewModel.ChangeDiameterEvent.Raise();
                }
                else
                {
                    MessageBox.Show("Wert muss größer als null sein!");
                    sendingClass.PipeDiameter = pipeDiameterString;
                }
            }
        }

        private void DataGridMovedAndResized_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //MessageBox.Show("Ended editing");
            DataGrid item = (DataGrid)sender;
            MovedAndResizedDbViewModel sendingClass = (MovedAndResizedDbViewModel)item.SelectedItem;
            DurchbruchMemoryViewModel.CurrentModel = sendingClass.DurchbruchModel;
            string id = sendingClass.ElementId;
            string shape = sendingClass.Shape;
            string pipeDiameter = sendingClass.PipeDiameter;
            string offset = sendingClass.Offset;

            //MessageBox.Show(id + ", " + shape + ", " + pipeDiameter + ", " + offset);
            //regex
            //value changed

            double metricOffset = sendingClass.DurchbruchModel.CutOffset.AsDouble() * 304.8;
            string offsetString = metricOffset.ToString("F1", CultureInfo.InvariantCulture);

            if (shape == "Rectangular")
            {
                //MessageBox.Show("Editing is not allowed for rectangular openings!", "Info");
                sendingClass.Offset = offsetString;
                sendingClass.PipeDiameter = "---";
                return;
            }

            double metricPipeDiameter = sendingClass.DurchbruchModel.PipeDiameter.AsDouble() * 304.8;
            string pipeDiameterString = metricPipeDiameter.ToString("F1", CultureInfo.InvariantCulture);

            if (offset != offsetString)
            {
                //MessageBox.Show("Changed offset");
                double number;
                bool result = double.TryParse(offset, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.Offset = offsetString;
                    return;
                }
                if (number < 0)
                {
                    MessageBox.Show("Wert kann nicht kleiner als null sein!");
                    sendingClass.Offset = offsetString;
                }
                else
                {
                    string newOffset = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Offset = newOffset;
                    //calculate new total D in gridtable
                    double diameterNumber = Convert.ToDouble(pipeDiameter, CultureInfo.InvariantCulture);
                    double totalDiameter = diameterNumber + 2 * number;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewOffset;
                    DurchbruchMemoryViewModel.CurrentModel.NewOffset = number;
                    DurchbruchMemoryViewModel.ChangeOffsetEvent.Raise();
                }

            }
            if (pipeDiameter != pipeDiameterString)
            {
                //MessageBox.Show("Changed pipe diameter");
                double number;
                bool result = double.TryParse(pipeDiameter, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                if (!result)
                {
                    MessageBox.Show("Der eingefügte Wert muss eine Zahl sein!");
                    sendingClass.PipeDiameter = pipeDiameterString; ;
                    return;
                }
                if (number > 0)
                {
                    string newPipeDiameter = number.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.PipeDiameter = newPipeDiameter;
                    //calculate new total D in gridtable
                    double offsetNumber = Convert.ToDouble(offset, CultureInfo.InvariantCulture);
                    double totalDiameter = number + 2 * offsetNumber;
                    string totalDiameterString = totalDiameter.ToString("F1", CultureInfo.InvariantCulture);
                    sendingClass.Diameter = totalDiameterString;
                    //raise event here to change offset
                    DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.SetNewDiameter;
                    DurchbruchMemoryViewModel.CurrentModel.NewDiameter = number;
                    DurchbruchMemoryViewModel.ChangeDiameterEvent.Raise();
                }
                else
                {
                    MessageBox.Show("Wert muss größer als null sein!");
                    sendingClass.PipeDiameter = pipeDiameterString;
                }
            }
        }

        private void Btn_Click_SaveSelected(object sender, RoutedEventArgs e)
        {
            if(MyTabs.SelectedIndex == 0)
            {
                DurchbruchMemoryViewModel.SelectedToSave = new List<Model.DurchbruchModel>();
                var selectedItems = DataGridNew.SelectedItems;
                foreach (var item in selectedItems)
                {
                    NewDurchbruchViewModel model = item as NewDurchbruchViewModel;
                    DurchbruchMemoryViewModel.SelectedToSave.Add(model.DurchbruchModel);
                }
            }
            if (MyTabs.SelectedIndex == 1)
            {
                DurchbruchMemoryViewModel.SelectedToSave = new List<Model.DurchbruchModel>();
                var selectedItems = DataGridMoved.SelectedItems;
                foreach (var item in selectedItems)
                {
                    MovedAndResizedDbViewModel model = item as MovedAndResizedDbViewModel;
                    DurchbruchMemoryViewModel.SelectedToSave.Add(model.DurchbruchModel);
                }
            }
            if (MyTabs.SelectedIndex == 2)
            {
                DurchbruchMemoryViewModel.SelectedToSave = new List<Model.DurchbruchModel>();
                var selectedItems = DataGridResized.SelectedItems;
                foreach (var item in selectedItems)
                {
                    ResizedDurchbruchViewModel model = item as ResizedDurchbruchViewModel;
                    DurchbruchMemoryViewModel.SelectedToSave.Add(model.DurchbruchModel);
                }
            }
            if (MyTabs.SelectedIndex == 3)
            {
                DurchbruchMemoryViewModel.SelectedToSave = new List<Model.DurchbruchModel>();
                var selectedItems = DataGridMovedAndResized.SelectedItems;
                foreach (var item in selectedItems)
                {
                    MovedAndResizedDbViewModel model = item as MovedAndResizedDbViewModel;
                    DurchbruchMemoryViewModel.SelectedToSave.Add(model.DurchbruchModel);
                }
            }
            DurchbruchMemoryViewModel.MemorySaveOption = MemorySaveOption.Selected;
            DurchbruchMemoryViewModel.SaveDataToExStorageEvent.Raise();
            DurchbruchMemoryViewModel.SignalEvent.WaitOne();
            DurchbruchMemoryViewModel.SignalEvent.Reset();
            SetDataContext();
            SetLabels();
        }

        private void Btn_Click_Filter(object sender, RoutedEventArgs e)
        {
            if (MyTabs.SelectedIndex == 0) FilterDatagridNew();
            if (MyTabs.SelectedIndex == 1) FilterDatagridMoved();
            if (MyTabs.SelectedIndex == 2) FilterDatagridResized();
            if (MyTabs.SelectedIndex == 3) FilterDatagridMovedAndResized();
        }
        private void FilterDatagridNew()
        {
            var collection = DataGridNew.Items;
            if(ComboBoxFilter.SelectedIndex == 0)
            {
                collection.Filter = null;
            }
            if (ComboBoxFilter.SelectedIndex == 1)
            {
                var myFilter = new Predicate<object>(item => ((NewDurchbruchViewModel)item).SystemType.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 2)
            {
                var myFilter = new Predicate<object>(item => ((NewDurchbruchViewModel)item).FireRating.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 3)
            {
                var myFilter = new Predicate<object>(item => ((NewDurchbruchViewModel)item).OpeningMark.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
        }

        private void FilterDatagridMoved()
        {
            var collection = DataGridMoved.Items;
            if (ComboBoxFilter.SelectedIndex == 0)
            {
                collection.Filter = null;
            }
            if (ComboBoxFilter.SelectedIndex == 1)
            {
                var myFilter = new Predicate<object>(item => ((MovedAndResizedDbViewModel)item).SystemType.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 2)
            {
                var myFilter = new Predicate<object>(item => ((MovedAndResizedDbViewModel)item).FireRating.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 3)
            {
                var myFilter = new Predicate<object>(item => ((MovedAndResizedDbViewModel)item).OpeningMark.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
        }

        private void FilterDatagridResized()
        {
            var collection = DataGridResized.Items;
            if (ComboBoxFilter.SelectedIndex == 0)
            {
                collection.Filter = null;
            }
            if (ComboBoxFilter.SelectedIndex == 1)
            {
                var myFilter = new Predicate<object>(item => ((ResizedDurchbruchViewModel)item).SystemType.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 2)
            {
                var myFilter = new Predicate<object>(item => ((ResizedDurchbruchViewModel)item).FireRating.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 3)
            {
                var myFilter = new Predicate<object>(item => ((ResizedDurchbruchViewModel)item).OpeningMark.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
        }

        private void FilterDatagridMovedAndResized()
        {
            var collection = DataGridMovedAndResized.Items;
            if (ComboBoxFilter.SelectedIndex == 0)
            {
                collection.Filter = null;
            }
            if (ComboBoxFilter.SelectedIndex == 1)
            {
                var myFilter = new Predicate<object>(item => ((MovedAndResizedDbViewModel)item).SystemType.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 2)
            {
                var myFilter = new Predicate<object>(item => ((MovedAndResizedDbViewModel)item).FireRating.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
            if (ComboBoxFilter.SelectedIndex == 3)
            {
                var myFilter = new Predicate<object>(item => ((MovedAndResizedDbViewModel)item).OpeningMark.ToUpper().Contains(TextBoxFilter.Text.ToUpper()));
                collection.Filter = myFilter;
            }
        }

        private void SelectMultipleItems()
        {
            if (MyTabs.SelectedIndex == 0)
            {
                DurchbruchMemoryViewModel.SelectedIds = new List<ElementId>();
                var selectedItems = DataGridNew.SelectedItems;
                foreach (var item in selectedItems)
                {
                    NewDurchbruchViewModel model = item as NewDurchbruchViewModel;
                    DurchbruchMemoryViewModel.SelectedIds.Add(model.DurchbruchModel.ElementId);
                }
            }
            if (MyTabs.SelectedIndex == 1)
            {
                DurchbruchMemoryViewModel.SelectedIds = new List<ElementId>();
                var selectedItems = DataGridMoved.SelectedItems;
                foreach (var item in selectedItems)
                {
                    MovedAndResizedDbViewModel model = item as MovedAndResizedDbViewModel;
                    DurchbruchMemoryViewModel.SelectedIds.Add(model.DurchbruchModel.ElementId);
                }
            }
            if (MyTabs.SelectedIndex == 2)
            {
                DurchbruchMemoryViewModel.SelectedIds = new List<ElementId>();
                var selectedItems = DataGridResized.SelectedItems;
                foreach (var item in selectedItems)
                {
                    ResizedDurchbruchViewModel model = item as ResizedDurchbruchViewModel;
                    DurchbruchMemoryViewModel.SelectedIds.Add(model.DurchbruchModel.ElementId);
                }
            }
            if (MyTabs.SelectedIndex == 3)
            {
                DurchbruchMemoryViewModel.SelectedIds = new List<ElementId>();
                var selectedItems = DataGridMovedAndResized.SelectedItems;
                foreach (var item in selectedItems)
                {
                    MovedAndResizedDbViewModel model = item as MovedAndResizedDbViewModel;
                    DurchbruchMemoryViewModel.SelectedIds.Add(model.DurchbruchModel.ElementId);
                }
            }
        }

        private void Btn_Click_MultipleSelection(object sender, RoutedEventArgs e)
        {
            SelectMultipleItems();
            DurchbruchMemoryViewModel.DurchbruchMemoryAction = DurchbruchMemoryAction.ShowElements;
            DurchbruchMemoryViewModel.SignalEvent.Set();
            DurchbruchMemoryViewModel.ShowElementEvent.Raise();
        }

        private void Btn_Click_RefreshContext(object sender, RoutedEventArgs e)
        {
            DurchbruchMemoryViewModel.UpdateDurchbrucheLight();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell.Content is TextBlock)
            {
                string copy = (cell.Content as TextBlock).Text;
                Clipboard.SetText(copy);
                TextBoxFilter.Text = copy;
            }
            e.Handled = true;
        }
    }
}
