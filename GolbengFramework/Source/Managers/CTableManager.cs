using CommonPackage.Tables;

using Golbeng.Framework.Commons;
using Golbeng.Framework.Loader;
using Golbeng.Framework.Managers;
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

		private ITableLoder GetLoader()
		{
			// Editor 상태에서 실행
			if (ManagerProvider.IsEditMode == true)
			{
				return new CEditModeTableLoader(CResourceManager.Instance, ManagerProvider.TableAssetPath);
			}

			// 실행 모드
			return new CPlayModeTableLoader(CResourceManager.Instance, ManagerProvider.TableAssetPath, ManagerProvider.PersistentDataPath);
		}

		public void RegisterTable<T>()
		{
			_registerTableType.Add(typeof(T));
		}

		public WaitFroCoroutineAction LoadTableAsync<T>(MonoBehaviour dispatcher) where T : class, new()
		{
			var action = new WaitFroCoroutineAction(dispatcher, () =>
			{
				LoadTable<T>();
			});

			return action;
		}

		// Bundle Load
		/*
		public WaitForCoroutineTasks LoadAllTableAsyncCorutine(MonoBehaviour dispatcher)
		{
			var worker = new WaitForCoroutineTasks(dispatcher);

			var loadTableMethodInfo = typeof(CTableManager).GetMethod("LoadTable", BindingFlags.NonPublic | BindingFlags.Instance);
			if (loadTableMethodInfo != null)
			{
				foreach (var table in _registerTableType)
				{
					worker.RegisterTask(() =>
					{
						var loadTableMethod = loadTableMethodInfo.MakeGenericMethod(table);
						loadTableMethod.Invoke(this, null);
					});
				}
			}

			worker.Run();

			return worker;
		}
		*/

		// async await Version
		/*
		public WaitForTask LoadAllTableAsync()
		{
			var loadTableMethodInfo = typeof(CTableManager).GetMethod("LoadTable", BindingFlags.NonPublic | BindingFlags.Instance);
			if(loadTableMethodInfo == null)
			{
				return new WaitForTask(() => { });
			}

			// prepare
			foreach (var table in _registerTableType)
			{
				var loader = GetLoader();

				var prepareMethodInfo = loader.GetType().GetMethod("Prepare", BindingFlags.Public | BindingFlags.Instance);
				if (prepareMethodInfo == null)
					continue;

				var prepareMethod = prepareMethodInfo.MakeGenericMethod(table);
				if (prepareMethod == null)
					continue;

				prepareMethod.Invoke(loader, null);
				_matchTableLoader.Add(table, loader);
			}

			// Load Tasks...
			var waitForTask = new WaitForTask(async () =>
			{
				List<Task> taskList = new List<Task>();
				foreach (var table in _registerTableType)
				{
					var task = Task.Run(() =>
					{
						var loadTableMethod = loadTableMethodInfo.MakeGenericMethod(table);
						loadTableMethod.Invoke(this, null);
					});
					taskList.Add(task);
				}

				await Task.WhenAll(taskList);
			});

			return waitForTask;
		}
		*/

		private void LoadTable<T>() where T : class, new()
		{
			var loader = GetLoader();

			try
			{
				var container = loader.LoadTable<T>();
				if (container == null)
				{
					ManagerProvider.Logger.Error("CTableManager", $"{nameof(T)} LoadTable is null");
					return;
				}

				lock (_conatiner)
				{
					var type = typeof(T);

					if (_conatiner.ContainsKey(type) == false)
					{
						_conatiner.Add(type, container);
					}
				}
			}
			catch (Exception e)
			{
				ManagerProvider.Logger.Exception("CTableMaanger", "LoadTable Exception", e);
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
		
		public T GetTableData<T, U>(U primaryKey) where T : TblBase where U : struct
		{
			Type type = typeof(T);
			if (_conatiner.ContainsKey(type) == false)
				return default(T);

			long queryPrimaryKey = TblBase.ConvertKey(primaryKey);

			var items = _conatiner[type];
			return items.SingleOrDefault(TblBase => TblBase.queryPrimaryKey == queryPrimaryKey) as T;
		}

		public T GetTableData<T, U, V>(U primaryKey, V secondaryKey) where T : TblBase where U : struct where V : struct
		{
			Type type = typeof(T);

			if (_conatiner.ContainsKey(type) == false)
				return default(T);

			long queryPrimaryKey = TblBase.ConvertKey(primaryKey);
			long querySecondaryKey = TblBase.ConvertKey(secondaryKey);

			var items = _conatiner[type];

			return items.SingleOrDefault(tblBase => tblBase.queryPrimaryKey == queryPrimaryKey && tblBase.querySecondaryKey == querySecondaryKey) as T;
		}
	}
}



