using Functions;
using OwnerSearch;
using System.Windows;
using System.Windows.Forms;

namespace GUI
{
    /// <summary>
    /// Interaction logic for AokRaumbuchExport.xaml
    /// </summary>
    public partial class AokRaumbuchExport : Window
    {
        RaumBuchExport _raumBuchExport;

        public AokRaumbuchExport(RaumBuchExport raumBuchExport)
        {
            SetOwner();
            _raumBuchExport = raumBuchExport;
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch ownerSearch = WindowHandleSearch.MainWindowHandle;
            ownerSearch.SetAsOwner(this);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xls, *.xlsx)|*.xls;*.xlsx";
            openFileDialog.Title = "Bitte ExportDataModel(.xlsx) Auswählen!";
            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                _raumBuchExport.ExportDataModelPath = openFileDialog.FileName;
                if(!_raumBuchExport.SetExcelDataModel()) return;
                _raumBuchExport.SetExportedRooms();
                SaveLogButton.IsEnabled = true;
                ExportToExcel.IsEnabled = true;
            }
        }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            _raumBuchExport.SaveExportedRoomsAs();
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xls, *.xlsx)|*.xls;*.xlsx";
            openFileDialog.Title = "Bitte Export Excel Template Auswählen!";
            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                _raumBuchExport.ExportExcelTemplatePath = openFileDialog.FileName;
                _raumBuchExport.SaveToExcel();
            }
        }
    }
}
