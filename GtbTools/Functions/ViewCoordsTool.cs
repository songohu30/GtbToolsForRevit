/*
 * Erstellt mit SharpDevelop.
 * Benutzer: m.trawczynski
 * Datum: 31.08.2020
 * Zeit: 14:19
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Windows;

namespace GtbTools
{
	/// <summary>
	/// To be used in actual dll
	/// </summary>
	public class ViewCoordsTool
	{
		UIDocument _uiDoc;
		string _coordinatesString;
		
		public ViewCoordsTool(UIDocument uiDoc)
		{
			_uiDoc = uiDoc;
		}
		
		public void ApplyCoordsToViews()
		{
			IList<XYZ> coords = GetActiveViewPQCoords();
			XYZ p = coords[0];
			XYZ q = coords[1];
			
			IList<UIView> viewList = _uiDoc.GetOpenUIViews();
			
			foreach (UIView uiview in viewList) 
			{	
				if(IsFloorOrCeilingView(uiview))
				{
					uiview.ZoomAndCenterRectangle(p,q);
				}
			}
		}
		
		public void SaveCurrentCoordinatesAs()
		{
			View view1 = _uiDoc.Document.ActiveView;
			IList<UIView> uiviews = _uiDoc.GetOpenUIViews();
			UIView uiview1 = null;
			foreach( UIView uv in uiviews )
  			{
			    if( uv.ViewId.Equals( view1.Id ) )
			    {
			      uiview1 = uv;
			      break;
			    }
			}		
						
			Rectangle rect = uiview1.GetWindowRectangle();
			IList<XYZ> corners = uiview1.GetZoomCorners();
			XYZ p = corners[0];
			XYZ q = corners[1];
			
			string content = p.X.ToString() + ":"+ p.Y.ToString() + ":"+ p.Z.ToString() + ":"+ q.X.ToString() + ":"+ q.Y.ToString() + ":" +q.Z.ToString();
			
		    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		    string savePath = Path.Combine(path, "CoordsPQ.txt");
		    _coordinatesString = content;
			
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			
			string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
			string name = "Koord_" + date;
			
			dlg.FileName = name; // Default file name
			dlg.DefaultExt = ".txt"; // Default file extension
			dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
            string initialDirectory = Path.Combine(@"H:\Revit\Makros\Gemeinsam genutzte Dateien", Environment.UserName);
            if (Directory.Exists(initialDirectory)) dlg.InitialDirectory = initialDirectory;
            Nullable<bool> result =  dlg.ShowDialog();
			
			if (result == true)
			{
			    string filename = dlg.FileName;
			    File.WriteAllText(filename, content);
			}			
		}

		public void Save3dCoordinatesAs()
        {
			string content = "3D view settings:" + Environment.NewLine;
			Document doc = _uiDoc.Document;
			IList<UIView> uiviews = _uiDoc.GetOpenUIViews();
			UIView activeUiView = null;
			View activeview = doc.ActiveView;
			foreach (UIView uiv in uiviews)
			{
				View view = doc.GetElement(uiv.ViewId) as View;
				if (view.Id.IntegerValue == activeview.Id.IntegerValue) activeUiView = uiv;
			}
			IList<XYZ> zoomRectangle = activeUiView.GetZoomCorners();
			string corner1 = zoomRectangle[0].X.ToString() + ":" + zoomRectangle[0].Y.ToString() + ":" + zoomRectangle[0].Z.ToString();
			string corner2 = zoomRectangle[1].X.ToString() + ":" + zoomRectangle[1].Y.ToString() + ":" + zoomRectangle[1].Z.ToString();

			View3D active3dView = activeview as View3D;
			ViewOrientation3D viewOrientation = active3dView.GetOrientation();
			string eyePosition = viewOrientation.EyePosition.X.ToString() + ":" + viewOrientation.EyePosition.Y.ToString() + ":" + viewOrientation.EyePosition.Z.ToString();
			string forwardDirection = viewOrientation.ForwardDirection.X.ToString() + ":" + viewOrientation.ForwardDirection.Y.ToString() + ":" + viewOrientation.ForwardDirection.Z.ToString();
			string upDirection = viewOrientation.UpDirection.X.ToString() + ":" + viewOrientation.UpDirection.Y.ToString() + ":" + viewOrientation.UpDirection.Z.ToString();

			content += corner1 + Environment.NewLine + corner2 + Environment.NewLine;
			content += eyePosition + Environment.NewLine + forwardDirection + Environment.NewLine + upDirection + Environment.NewLine;


			if (active3dView.IsSectionBoxActive)
			{
				BoundingBoxXYZ box = active3dView.GetSectionBox();				
				string minBox = box.Min.X.ToString() + ":" + box.Min.Y.ToString() + ":" + box.Min.Z.ToString();
				string maxBox = box.Max.X.ToString() + ":" + box.Max.Y.ToString() + ":" + box.Max.Z.ToString();

				string transformVector = box.Transform.Origin.X.ToString() + ":" + box.Transform.Origin.Y.ToString() + ":" + box.Transform.Origin.Z.ToString();
				content += minBox + Environment.NewLine + maxBox + Environment.NewLine + transformVector;
			}

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			string date = DateTime.Now.ToString("dd.MM.yy HH-mm-ss");
			string name = "3D_Koord_" + date;
			dlg.FileName = name; // Default file name
			dlg.DefaultExt = ".txt"; // Default file extension
			dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
			string initialDirectory = Path.Combine(@"H:\Revit\Makros\Gemeinsam genutzte Dateien", Environment.UserName);
			if (Directory.Exists(initialDirectory)) dlg.InitialDirectory = initialDirectory;
			Nullable<bool> result = dlg.ShowDialog();
			if (result == true)
			{
				string filename = dlg.FileName;
				File.WriteAllText(filename, content);
			}
		}

		public void Load3dCoordinatesFrom()
		{
			char decSeparator = GetThreadDecimalSeparator();
			if (decSeparator != ',' && decSeparator != '.')
			{
				TaskDialog.Show("Error", "Uknown decimal separator");
			}

			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

			string initialDirectory = Path.Combine(@"H:\Revit\Makros\Gemeinsam genutzte Dateien", Environment.UserName);
			if (Directory.Exists(initialDirectory)) dlg.InitialDirectory = initialDirectory;

			Nullable<bool> result = dlg.ShowDialog();

			if (result == true)
			{
				string savePath = dlg.FileName;
				string content = File.ReadAllText(savePath);
				if (decSeparator == '.')
				{
					content = content.Replace(',', '.');
				}
				if (decSeparator == ',')
				{
					content = content.Replace('.', ',');
				}
				string[] coordArray = content.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

				XYZ p;
				XYZ q;
				XYZ eyePosition;
				XYZ forwardDirection;
				XYZ upDirection;
				XYZ minBox = null;
				XYZ maxBox = null;
				XYZ origin = null;

				try
                {
					string[] corner1 = coordArray[1].Split(':');
					string[] corner2 = coordArray[2].Split(':');
					string[] eyePosArray = coordArray[3].Split(':');
					string[] forDirArray = coordArray[4].Split(':');
					string[] upDirArray = coordArray[5].Split(':');

					p = new XYZ(Convert.ToDouble(corner1[0]), Convert.ToDouble(corner1[1]), Convert.ToDouble(corner1[2]));
					q = new XYZ(Convert.ToDouble(corner2[0]), Convert.ToDouble(corner2[1]), Convert.ToDouble(corner2[2]));
					eyePosition = new XYZ(Convert.ToDouble(eyePosArray[0]), Convert.ToDouble(eyePosArray[1]), Convert.ToDouble(eyePosArray[2]));
					forwardDirection = new XYZ(Convert.ToDouble(forDirArray[0]), Convert.ToDouble(forDirArray[1]), Convert.ToDouble(forDirArray[2]));
					upDirection = new XYZ(Convert.ToDouble(upDirArray[0]), Convert.ToDouble(upDirArray[1]), Convert.ToDouble(upDirArray[2]));

					if(coordArray.Length == 9)
                    {
						string[] minBoxArray = coordArray[6].Split(':');
						string[] maxBoxArray = coordArray[7].Split(':');
						string[] originArray = coordArray[8].Split(':');
						minBox = new XYZ(Convert.ToDouble(minBoxArray[0]), Convert.ToDouble(minBoxArray[1]), Convert.ToDouble(minBoxArray[2]));
						maxBox = new XYZ(Convert.ToDouble(maxBoxArray[0]), Convert.ToDouble(maxBoxArray[1]), Convert.ToDouble(maxBoxArray[2]));
						origin = new XYZ(Convert.ToDouble(originArray[0]), Convert.ToDouble(originArray[1]), Convert.ToDouble(originArray[2]));
					}
				}
				catch
                {
					TaskDialog.Show("Error", "Can't read coordinates from file. Coords must be of 3d type.");
					return;
				}

				View3D active3dView = _uiDoc.Document.ActiveView as View3D;
				ViewOrientation3D viewOrientation3D = new ViewOrientation3D(eyePosition, upDirection, forwardDirection);
				active3dView.SetOrientation(viewOrientation3D);

				IList<UIView> uiviews = _uiDoc.GetOpenUIViews();
				UIView activeUiView = null;
				View activeview = _uiDoc.ActiveView;
				foreach (UIView uiv in uiviews)
				{
					View view = _uiDoc.Document.GetElement(uiv.ViewId) as View;
					if (view.Id.IntegerValue == activeview.Id.IntegerValue) activeUiView = uiv;
				}
				activeUiView.ZoomAndCenterRectangle(p, q);

				using (Transaction tx = new Transaction(_uiDoc.Document, "3d section box edit"))
				{
					tx.Start();
					if (coordArray.Length == 9)
					{
						BoundingBoxXYZ box = new BoundingBoxXYZ();
						box.Min = minBox;
						box.Max = maxBox;
						Transform transform = Transform.CreateTranslation(origin);

						box.Transform = transform;
						active3dView.SetSectionBox(box);
						active3dView.IsSectionBoxActive = true;
					}
					else
					{
						active3dView.IsSectionBoxActive = false;
					}
					tx.Commit();
				}
			}
		}
		
		public void LoadCoordinatesFrom()
		{
			char decSeparator = GetThreadDecimalSeparator();
			if(decSeparator != ',' && decSeparator != '.')
            {
				TaskDialog.Show("Error", "Uknown decimal separator");
            }
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            string initialDirectory = Path.Combine(@"H:\Revit\Makros\Gemeinsam genutzte Dateien", Environment.UserName);
            if (Directory.Exists(initialDirectory)) dlg.InitialDirectory = initialDirectory;

            Nullable<bool> result = dlg.ShowDialog();
			
			if (result == true)
			{
			    string savePath = dlg.FileName;
			   	string content = File.ReadAllText(savePath);
				if(decSeparator == '.')
                {
					content = content.Replace(',', '.');
                }
				if (decSeparator == ',')
				{
					content = content.Replace('.', ',');
				}
				string[] coordArray = content.Split(':');
				XYZ p;
				XYZ q;
				try
                {
					p = new XYZ(Convert.ToDouble(coordArray[0]), Convert.ToDouble(coordArray[1]), Convert.ToDouble(coordArray[2]));
					q = new XYZ(Convert.ToDouble(coordArray[3]), Convert.ToDouble(coordArray[4]), Convert.ToDouble(coordArray[5]));
				}
				catch
                {
					TaskDialog.Show("Error", "Can't read coordinates from file. Coords must be of 2d type.");
					return;
                }

				View view2 = _uiDoc.Document.ActiveView;
				UIView uiview2 = null;
				IList<UIView> uiviews = _uiDoc.GetOpenUIViews();

				foreach( UIView uv in uiviews )
	  			{
				    if( uv.ViewId.Equals( view2.Id ) )
				    {
				      uiview2 = uv;
				      break;
				    }
				}
				uiview2.ZoomAndCenterRectangle(p, q);	
			}
		}
		
		private char GetThreadDecimalSeparator()
        {
			return Convert.ToChar(Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		}

		private IList<XYZ> GetActiveViewPQCoords()
		{
			View activeView = _uiDoc.Document.ActiveView;
			IList<XYZ> result = new List<XYZ>();
			UIView uiview = null;
			IList<UIView> uiviews = _uiDoc.GetOpenUIViews();
			foreach( UIView uv in uiviews )
  			{
			    if( uv.ViewId.Equals( activeView.Id ) )
			    {
			      uiview = uv;
			      break;
			    }
			}					
			Rectangle rect = uiview.GetWindowRectangle();
			result = uiview.GetZoomCorners();
			return result;
		}
		
		private bool IsFloorOrCeilingView(UIView uiView)
		{
			bool result = false;
			View v = _uiDoc.Document.GetElement(uiView.ViewId) as View;
            if(v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.CeilingPlan || v.ViewType == ViewType.EngineeringPlan || v.ViewType == ViewType.AreaPlan) result = true;
			return result;
		}
	}
}
