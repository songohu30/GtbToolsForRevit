/*
 * Created by SharpDevelop.
 * User: m.trawczynski
 * Date: 24.08.2020
 * Time: 15:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace GtbMakros
{
	/// <summary>
	/// Description of ErrorLog.
	/// </summary>
	public class ErrorLog
	{
		public string Name {get; private set;}
		public string PersonalDirPath {get; private set;}
		public string FilePath{get; private set;}
		public bool LogCreated {get; private set;}
		
		
		public ErrorLog()
		{
			CreatePersonalDirectory();
			SetLogName();
			SetFilePath();
			CreateLog();
		}
		
		public void WriteToLog(string content)
		{
			if(LogCreated) File.AppendAllText(FilePath, content + Environment.NewLine);			
		}
		
		public void DeleteLog()
		{
			if(LogCreated) File.Delete(FilePath);
		}
		
		private void CreateLog()
		{
			if(LogCreated)
			{
				try 
				{
					File.WriteAllText(FilePath, "Log initiated:" + Environment.NewLine + Environment.NewLine);
				} 
				catch (Exception ex) 
				{
					TaskDialog.Show("Error", ex.ToString());
					LogCreated = false;
				}	
			}	
		}
		
		private void SetFilePath()
		{
			if(LogCreated)
			{
				FilePath = Path.Combine(PersonalDirPath, Name);
			}
		}
		
		private void SetLogName()
		{
			string date = DateTime.Now.ToString("dd-MM-yy HH.mm.ss");
			Name = "Log_" + date + ".log";
			//TaskDialog.Show("test", date);
		}
		
		private void CreatePersonalDirectory()
		{
			string dirPath = @"H:\Revit\Makros\Gemeinsam genutzte Dateien";
			string personalName = Environment.UserName;
			string personalDirPath = Path.Combine(dirPath, Environment.UserName);
			if(!CheckServerDirectoryAccess(dirPath))
			{
				TaskDialog.Show("Error", "Can't access disk H:" + Environment.NewLine + "ErrorLog will not be created");
				LogCreated = false;
				return;
			}
			if(Directory.Exists(personalDirPath) != true)
			{
				try 
				{
					Directory.CreateDirectory(personalDirPath);
					LogCreated = true;
					PersonalDirPath = personalDirPath;
				} 
				catch (Exception ex)
				{				
					TaskDialog.Show("Error", ex.ToString());
					LogCreated = false;
				}
			}
			else
			{
				PersonalDirPath = personalDirPath;
				LogCreated = true;
			}
		}
		
		private bool CheckServerDirectoryAccess(string path)
		{
			bool result = false;
			if(Directory.Exists(path)) result = true;
			return result;
		}
	}
}
