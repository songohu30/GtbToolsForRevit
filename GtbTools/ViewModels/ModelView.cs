/*
 * Created by SharpDevelop.
 * User: m.trawczynski
 * Date: 25.08.2020
 * Time: 09:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace GtbTools
{
	/// <summary>
	/// This class will be used to link with GTB dock panel. ModelView is used in OpenViewsTool class to create list for ZoomWindow.
	/// </summary>
	public class ModelView : INotifyPropertyChanged
	{
		public string Name { get; set; }
		public View View { get; set; }
		public bool IsSectionView { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
