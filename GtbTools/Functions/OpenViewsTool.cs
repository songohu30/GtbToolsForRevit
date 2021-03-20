/*
 * Created by SharpDevelop.
 * User: m.trawczynski
 * Date: 08/31/2020
 * Time: 11:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Threading;

namespace GtbTools
{
	/// <summary>
	/// This class will be used to link with GTB dock panel. Implement InotifyProperty change on VS
	/// </summary>
	public class OpenViewsTool
	{
		public List<ModelView> ModelViewList {get; set;}
		public WindowResult WindowResult {get; set;}
		public Nullable<bool> CloseInactive {get; set;}

		ErrorLog _errorLog;
		
		UIDocument _uiDoc;
		Document _doc;
		View _activeView;
		
		public OpenViewsTool(UIDocument uiDoc, ErrorLog errorLog)
		{
			_errorLog = errorLog;
			_uiDoc = uiDoc;
			_doc = uiDoc.Document;
			_activeView = _doc.ActiveView;
			//CreateModelViewList();
		}
		
		public void OpenViews()
		{
			ViewCoordsTool vct = new ViewCoordsTool(_uiDoc);
			
			if(WindowResult != WindowResult.UserApply) return;
			foreach (ModelView mv in ModelViewList) 
			{
				if(mv.IsSelected) 
				{
					_uiDoc.ActiveView = mv.View;
					DoEvents();
				}
				_uiDoc.ActiveView = _activeView;
			}
		}
		
		public void CreateModelViewList()
		{
			_errorLog.WriteToLog("Creating model view list...:");
			List<View> viewList = GetFloorAndCeilingViews();
			ModelViewList = new List<ModelView>();
			foreach (View v in viewList)
			{
				ModelView mv = new ModelView();
				mv.Name = v.Name;
				mv.IsSelected = false;
				mv.View= v;
				ModelViewList.Add(mv);
				_errorLog.WriteToLog("View name is: " + v.Name + " View type is: " + v.ViewType.ToString());
			}
		}
		
		public List<ModelView> GetPrimaryViews()
		{
			List<ModelView> primaryViews = new List<ModelView>();
			foreach (ModelView modelView in ModelViewList) 
			{
				if(modelView.View.GetPrimaryViewId() != ElementId.InvalidElementId) continue;
				primaryViews.Add(modelView);
			}
			return primaryViews;
		}
		
		public List<ModelView> GetPrimaryViews(List<ModelView> modelViewList)
		{
			List<ModelView> primaryViews = new List<ModelView>();
			foreach (ModelView modelView in modelViewList) 
			{
				if(modelView.View.GetPrimaryViewId() != ElementId.InvalidElementId) continue;
				primaryViews.Add(modelView);
			}
			return primaryViews;
		}
		
		public List<ModelView> FilterViewList(List<ModelView> modelViewList, string filter)
		{
			List<ModelView> result = new List<ModelView>();
			
			foreach (ModelView mv in modelViewList) 
			{
				if(mv.Name.ToString().ToUpper().Contains(filter.ToUpper())) result.Add(mv);
				
			}			
			return result;
		}
		
		public void CloseInactiveViews()
		{
			List<UIView> uiViewList = _uiDoc.GetOpenUIViews().ToList();
			List<UIView> uiViewsRemove = new List<UIView>();
			List<UIView> uiViewsStay = new List<UIView>();
			foreach (ModelView modelView in ModelViewList) 
			{
				if(modelView.IsSelected)
				{
					uiViewsStay.Add(GetUIView(modelView.View));
				}
			}
			uiViewsStay.Add(GetUIView(_activeView));
			//uiViewsRemove = uiViewList.Except(uiViewsStay).ToList();
			
			uiViewsRemove = uiViewList.Where(x => !uiViewsStay.Any(y => x != null && x.ViewId.IntegerValue == y.ViewId.IntegerValue)).ToList();
			
			//TaskDialog.Show("test", uiViewsRemove.Count.ToString());
			foreach (UIView uiView in uiViewsRemove) 
			{
				if(uiView == null)
                {
					TaskDialog.Show("Warnung!", "Bitte schließen Sie die Ansichten nicht, bevor ich fertig bin!");
					continue;
                }
				uiView.Close();
			}
		}

		public static UIView GetUIView(View view, UIDocument uidoc)
		{
			UIView uiview = null;
			IList<UIView> uiviews = uidoc.GetOpenUIViews();
			foreach (UIView uv in uiviews)
			{
				if (uv.ViewId.Equals(view.Id))
				{
					uiview = uv;
					break;
				}
			}
			return uiview;
		}

		private UIView GetUIView(View view)
		{
			UIView uiview = null;
			IList<UIView> uiviews = _uiDoc.GetOpenUIViews();
			foreach( UIView uv in uiviews )
  			{
			    if( uv.ViewId.Equals( view.Id ) )
			    {
			      uiview = uv;
			      break;
			    }
			}
			return uiview;
		}
		
		private List<View> GetFloorAndCeilingViews()
		{
			FilteredElementCollector ficol = new FilteredElementCollector(_doc);
			ficol.OfClass(typeof(View)).ToList();
			List<View> viewList = new List<View>();
			foreach (View v in ficol) 
			{
				//if(v.GetPrimaryViewId() != ElementId.InvalidElementId) continue;
				if(v.IsTemplate) continue;
				if(v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.CeilingPlan || v.ViewType == ViewType.EngineeringPlan )
				{
					viewList.Add(v);
				}
			}
			return viewList;
		}
		
		public void DoEvents()
		{
    		DispatcherFrame frame = new DispatcherFrame();
    		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
        	new DispatcherOperationCallback(ExitFrame), frame);
    		Dispatcher.PushFrame(frame);
		}

		public object ExitFrame(object f)
		{
    		((DispatcherFrame)f).Continue = false;
   
    		return null;
		}
	}
	public enum WindowResult
	{
        None,
		UserApply,
		UserClosed
	}
}
