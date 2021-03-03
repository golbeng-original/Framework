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
	public class CEditModeTableLoader : ITableLoder
	{
		public CEditModeTableLoader(CResourceManager resourceManager, string targetTablePath) : base(resourceManager, targetTablePath) {}

		public override HashSet<TblBase> LoadTable<T>()
		{
			var tableMeta = TableUtils.GetTableMeta<T>();
			if (tableMeta == null)
				return null;

			string dbAssetPath = Path.Combine(AssetsPath, tableMeta.ClientDbName);
			var loadPath = ResourceManager.FindAssetFullPath(dbAssetPath);

			try
			{
				var result = LoadSqliteDB<T>(loadPath);
				return result;
			}
			catch (Exception e)
			{
				ManagerProvider.Logger.Exception("CEditModeTableLoder", $"{loadPath} LoadSqliteDB Exception", e);
				return null;
			}
		}
	}
}
