using CommonPackage.Tables;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Loader
{
	public class CPcTableLoader : ITableLoder
	{
		public CPcTableLoader(string assestRootPath) : base(assestRootPath) { }

		public override HashSet<TblBase> LoadTable<T>()
		{
			var result = _LoadTable<T>();
			result.Wait();

			return result.Result;
		}
		private async Task<HashSet<TblBase>> _LoadTable<T>() where T : new()
		{
			var tableMeta = TableUtils.GetTableMeta<T>();
			if (tableMeta == null)
				return null;

			string loadablePath = Path.Combine(AssetsPath, tableMeta.DbName);
			if (File.Exists(loadablePath) == false)
				return null;

			return await LoadSqliteDB<T>(loadablePath);
		}

	}
}
