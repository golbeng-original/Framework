using CommonPackage.Tables;
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
	public class CPlayModeTableLoader : ITableLoder
	{
		private string _temporaryPath = "";

		public CPlayModeTableLoader(CResourceManager resourceManager, string targetTablePath, string temporaryPath) : base(resourceManager, targetTablePath)
		{
			_temporaryPath = temporaryPath;
		}

		private string CreateTempFile(string tableName)
		{
			string dbAssetPath = Path.Combine(AssetsPath, tableName);

			var stream = ResourceManager.FindStreamResource(dbAssetPath);
			if (stream == null)
				return "";

			string tempPath = Path.Combine(_temporaryPath, tableName);
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
			catch(Exception e)
			{
				ManagerProvider.Logger.Exception("CPlayModeTableLoader", $"{tempPath} CreateFile exception", e);

				if (fileInfo.Exists == true)
					fileInfo.Delete();

				return "";
			}
		}

		public override HashSet<TblBase> LoadTable<T>()
		{
			var tableMeta = TableUtils.GetTableMeta<T>();
			if (tableMeta == null)
			{
				ManagerProvider.Logger.Error("CPlayModeTableLoader", $"{typeof(T).Name} tableMeta is null");
				return null;
			}
			
			var loadablePath = CreateTempFile(tableMeta.ClientDbName);
			if (string.IsNullOrEmpty(loadablePath) == true)
				return null;
			
			try
			{
				var result = LoadSqliteDB<T>(loadablePath);
				return result;
			}
			catch (Exception e)
			{
				ManagerProvider.Logger.Exception("CPlayModeTableLoader", $"{loadablePath} LoadSqliteDB exception", e);
				return null;
			}
			finally
			{
				if (File.Exists(loadablePath) == true)
					File.Delete(loadablePath);
			}
		}
	}
}
