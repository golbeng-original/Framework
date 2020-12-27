using CommonPackage.Tables;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Golbeng.Framework.Loader
{
	public class CAndroidTableLoader : ITableLoder
	{
		private string _loadPath = "";

		public CAndroidTableLoader(string assestRootPath, string loadPath) : base(assestRootPath)
		{
			_loadPath = loadPath;
		}

		public override HashSet<TblBase> LoadTable<T>()
		{
			var result = _LoadTable<T>();
			result.Wait();

			return result.Result;
		}

		private async Task<HashSet<TblBase>> _LoadTable<T>() where T : class, new()
		{
			var tableMeta = TableUtils.GetTableMeta<T>();
			if (tableMeta == null)
				return null;

			string loadablePath = Path.Combine(_loadPath, tableMeta.DbName);

			if (File.Exists(loadablePath) == true)
				File.Delete(loadablePath);

			string streamingAssetsPath = Path.Combine(AssetsPath, tableMeta.DbName);

			using (var unityWebRequest = UnityWebRequest.Get(streamingAssetsPath))
			{
				unityWebRequest.timeout = 1000;
				await Task.Run(() =>
				{
					// 이 방법이 거슬리면 나중에 Awaiter 구현하자..
					var request = unityWebRequest.SendWebRequest();

					while (request.isDone == false) { }
				});

				File.WriteAllBytes(loadablePath, unityWebRequest.downloadHandler.data);
			}

			if (File.Exists(loadablePath) == false)
				return null;

			//
			var container = await LoadSqliteDB<T>(loadablePath);
			//

			File.Delete(loadablePath);

			return container;
		}
	}

}
