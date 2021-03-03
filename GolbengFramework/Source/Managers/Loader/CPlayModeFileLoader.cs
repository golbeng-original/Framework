using Golbeng.Framework.Commons;
using Golbeng.Framework.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Loader
{
	public class CPlayModeFileLoader : IFileLoader
	{
		private string _temporaryPath = "";

		public CPlayModeFileLoader(CResourceManager resourceManager, string temporaryPath) : base(resourceManager) 
		{
			_temporaryPath = temporaryPath;
		}

		private string CreateTempFile(string resourcePath)
		{
			var stream = ResourceManager.FindStreamResource(resourcePath);
			if (stream == null)
				return "";

			//FileInfo fileInfo = new FileInfo(resourcePath);
			//string fureFileName = fileInfo.Name;

			string tempPath = Path.Combine(_temporaryPath, resourcePath);
			FileInfo fileInfo = new FileInfo(tempPath);
			if (fileInfo.Directory.Exists == false)
				fileInfo.Directory.Create();

			if (fileInfo.Exists == true)
				fileInfo.Delete();

			try
			{
				using (var file = File.Create(tempPath))
				{
					stream.CopyTo(file);
					stream.Close();
				}

				return tempPath;
			}
			catch (Exception e)
			{
				ManagerProvider.Logger.Exception("CPlayModeFileLoader", $"{tempPath} CreateFile exception", e);

				if (fileInfo.Exists == true)
					fileInfo.Delete();

				return "";
			}
		}

		protected override Stream _Load(string resourcePath)
		{
			/*
			var loadablePath = CreateTempFile(resourcePath);
			if (string.IsNullOrEmpty(loadablePath) == true)
				return null;

			try
			{
				var result = LoadSqliteDB<T>(loadablePath);
				return result;
			}
			catch (Exception e)
			{
				Debug.LogError($"{loadablePath} LoadSqliteDB error ({e.Message})");
				return null;
			}
			finally
			{
				if (File.Exists(loadablePath) == true)
					File.Delete(loadablePath);
			}
			*/

			return null;
		}
	}
}
