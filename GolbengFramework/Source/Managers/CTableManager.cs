using CommonPackage.Tables;

using Golbeng.Framework.Commons;
using Golbeng.Framework.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Golbeng.Framework.Manager
{
	public class CTableManager : Singleton<CTableManager>
	{
		private HashSet<Type> _registerTableType = new HashSet<Type>();

		private Dictionary<Type, HashSet<TblBase>> _conatiner = new Dictionary<Type, HashSet<TblBase>>();

		private ITableLoder _loader = null;

		public override void OnInitSingleton()
		{
			// PC, Editor
			if (ManagerProvider.IsEditMode == true || ManagerProvider.CurrentPlatform == ManagerProvider.Platform.Windows)
			{
				_loader = new CPcTableLoader(ManagerProvider.RawTableAssestsPath);
				return;
			}

			// android
			if (ManagerProvider.CurrentPlatform == ManagerProvider.Platform.Android)
			{
				_loader = new CAndroidTableLoader(ManagerProvider.RawTableAssestsPath, ManagerProvider.LoadTableAssestsPath);
			}
			// Iphone
			else if (ManagerProvider.CurrentPlatform == ManagerProvider.Platform.IPhone)
			{
				//
			}
		}
		
		public void RegisterTable<T>()
		{
			_registerTableType.Add(typeof(T));
		}

		// async await Version
		public WaitForTask LoadAllTableAsync()
		{
			var methodInfo = typeof(CTableManager).GetMethod("LoadTable", BindingFlags.NonPublic | BindingFlags.Instance);
			if(methodInfo == null)
			{
				return new WaitForTask(() => { });
			}

			var waitForTask = new WaitForTask(async () =>
			{
				List<Task> taskList = new List<Task>();
				foreach (var table in _registerTableType)
				{
					var task = Task.Run(() =>
					{
						var loadTableMethod = methodInfo.MakeGenericMethod(table);
						loadTableMethod.Invoke(this, null);
					});
					taskList.Add(task);
				}

				await Task.WhenAll(taskList);
			});

			return waitForTask;
		}

		private void LoadTable<T>() where T : class, new()
		{
			try
			{
				var container = _loader.LoadTable<T>();
				if(container == null)
				{
					Debug.Log($"{nameof(T)} LoadTable is null");
					return;
				}

				lock(_conatiner)
				{
					Type type = typeof(T);
					if (_conatiner.ContainsKey(type) == false)
					{
						_conatiner.Add(type, container);
					}
				}
			}
			catch(Exception e)
			{
				Debug.Log($"{e.Message} [{e.InnerException?.Message}]");
			}

		}

		public IEnumerable<T> GetTableDatas<T>() where T : class
		{
			Type type = typeof(T);

			if (_conatiner.ContainsKey(type) == false)
				yield break;

			var items = _conatiner[type];

			foreach (var item in items)
			{
				yield return item as T;
			}
		}
		public T GetTableData<T>(uint primaryKey) where T : TblBase
		{
			Type type = typeof(T);

			if (_conatiner.ContainsKey(type) == false)
				return default(T);

			var items = _conatiner[type];

			return items.SingleOrDefault(tblBase => tblBase.primarykey == primaryKey) as T;
		}
		public T GetTableData<T>(uint primaryKey, uint secondaryKey) where T : TblBase
		{
			Type type = typeof(T);

			if (_conatiner.ContainsKey(type) == false)
				return default(T);

			var items = _conatiner[type];

			return items.SingleOrDefault(tblBase => tblBase.primarykey == primaryKey && tblBase.secondarykey == secondaryKey) as T;
		}
	}
}



