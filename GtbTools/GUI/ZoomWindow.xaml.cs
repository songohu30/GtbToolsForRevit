/*
 * Created by SharpDevelop.
 * User: m.trawczynski
 * Date: 24.08.2020
 * Time: 15:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

namespace GtbTools
{
	/// <summary>
	/// Interaction logic for ZoomWindow.xaml - must apply inotify property changed
	/// </summary>
	public partial class ZoomWindow : Window
	{
		public OpenViewsTool OpenViewsTool {get; set;}
		List<ModelView> _currentItems;
			
		public ZoomWindow(OpenViewsTool openViewsTool)
		{
			OpenViewsTool = openViewsTool;
			InitializeComponent();
			DataContext = this;
			Topmost = true;
            
		}
		
		public void FilterList(string filter)
		{
			List<ModelView> filteredList = OpenViewsTool.FilterViewList(OpenViewsTool.ModelViewList, filter);
			if(ChbxShowPrimaryViewsOnly.IsChecked == true)
			{
				List<ModelView> primaryViews = OpenViewsTool.GetPrimaryViews(filteredList);
				ListBox.ItemsSource = primaryViews;
				_currentItems = primaryViews;
			}
			else
			{
				ListBox.ItemsSource = filteredList;
				_currentItems = filteredList;
			}
		}
		
		void button1_Click(object sender, RoutedEventArgs e)
		{
			foreach (ModelView modelView in OpenViewsTool.ModelViewList) 
			{
				modelView.IsSelected = false;
			}
			ListBox.ItemsSource = _currentItems;			
		}
		
		void button2_Click(object sender, RoutedEventArgs e)
		{
			OpenViewsTool.WindowResult = WindowResult.UserApply;
			OpenViewsTool.CloseInactive = ChbxCloseInactive.IsChecked;
			Close();
		}
		
		void button3_Click(object sender, RoutedEventArgs e)
		{
			FilterList(filterBox.Text);
		}
		
		void primary_Checkbox_Checked(object sender, RoutedEventArgs e)
		{
			FilterList(filterBox.Text);
		}
		
		void primary_Checkbox_Unchecked(object sender, RoutedEventArgs e)
		{
			FilterList(filterBox.Text);
		}
	}
}