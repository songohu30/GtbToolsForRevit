using FamilyTools;
using OwnerSearch;
using System.Windows;
using System.Windows.Media;

namespace GUI
{
    /// <summary>
    /// Interaction logic for FamilyToolsWindow.xaml
    /// </summary>
    public partial class FamilyToolsWindow : Window
    {
        CheckboxLabelReplace _checkboxLabelReplace;
        ConnectorParameters _connectorParameters;
        public FamilyToolsWindow(CheckboxLabelReplace checkboxLabelReplace, ConnectorParameters connectorParameters)
        {
            _checkboxLabelReplace = checkboxLabelReplace;
            _connectorParameters = connectorParameters;
            SetOwner();
            InitializeComponent();
        }

        private void SetOwner()
        {
            WindowHandleSearch search = WindowHandleSearch.MainWindowHandle;
            search.SetAsOwner(this);
        }

        private void Btn_Click_AddParameter(object sender, RoutedEventArgs e)
        {
            _checkboxLabelReplace.AddParameter();
        }

        private void Btn_Click_FindTypesByName(object sender, RoutedEventArgs e)
        {
            _checkboxLabelReplace.AlignTypesWithSymbol();
        }

        private void Btn_Click_DeleteSetLabels(object sender, RoutedEventArgs e)
        {
            _checkboxLabelReplace.DeleteAndSetLabel();
        }

        private void Btn_Click_3in1(object sender, RoutedEventArgs e)
        {
            _checkboxLabelReplace.AddParameter();
            _checkboxLabelReplace.AlignTypesByVisibility();
            _checkboxLabelReplace.DeleteAndSetLabel();
        }

        private void Btn_Click_FindTypesByVisibility(object sender, RoutedEventArgs e)
        {
            _checkboxLabelReplace.AlignTypesByVisibility();
        }

        private void Btn_Click_ConSharedPar(object sender, RoutedEventArgs e)
        {
            bool result = int.TryParse(StepNo.Text, out int stepNo);
            if(result == true)
            {
                _connectorParameters.Initialize(stepNo);
                _connectorParameters.SetParameters();
            }
            else
            {
                StepNo.Foreground = Brushes.Red;
            }
        }
    }
}
