using Golbeng.Framework.Commons;
using Golbeng.Framework.Manager;
using CommonPackage.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ILogger = Golbeng.Framework.Commons.ILogger;
using System.Collections;
using Golbeng.Framework.Managers;

namespace Golbeng.Framework
{
	public class FrameworkStarter
	{
		public void Initalize(string configureFile = null)
		{
			switch (Application.platform)
			{
				case RuntimePlatform.Android:
					ManagerProvider.CurrentPlatform = ManagerProvider.Platform.Android;
					break;
				case RuntimePlatform.IPhonePlayer:
					ManagerProvider.CurrentPlatform = ManagerProvider.Platform.IPhone;
					break;
				case RuntimePlatform.WindowsPlayer:
					ManagerProvider.CurrentPlatform = ManagerProvider.Platform.Windows;
					break;
			}

			ManagerProvider.IsEditMode = Application.installMode == ApplicationInstallMode.Editor ? true : false;

			ManagerProvider.StreamingAssestsPath = Application.streamingAssetsPath;
			ManagerProvider.PersistentDataPath = Application.persistentDataPath;

			DirectoryInfo directoryInfo = new DirectoryInfo(ManagerProvider.LoadTableAssestsPath);
			if (directoryInfo.Exists == false)
				directoryInfo.Create();

			// 임시
			string url = "https://localhost:44302";
			if (ManagerProvider.CurrentPlatform == ManagerProvider.Platform.Android)
				url = "https://10.0.2.2:44302";

			ManagerProvider.HttpConfig.ConnectUrl = $"{url}/WeatherForecast";
			ManagerProvider.HttpConfig.SendMethod = "proto2";

			ManagerProvider.SignalRConfig.ConnectUrl = $"{url}/hub";
			ManagerProvider.SignalRConfig.SendMethod = "SendHub";
			ManagerProvider.SignalRConfig.ReceiveMethod = "ReceiveHub";
			//
		}

		public AsyncWorker LaodTableManager(MonoBehaviour dispatcher)
		{
			AsyncWorker worker = new AsyncWorker();
			worker.JobList.Add(LaodTableManager);
			worker.JobList.Add(LoadIconManager);

			worker.Run(dispatcher);

			return worker;
		}

		private IEnumerator LaodTableManager(AsyncWorker.WorkArgs args)
		{
			args.Description = "테이블 로드 시작";

			var tableManager = CTableManager.Instance;
			tableManager.RegisterTable<TblExample_table>();

			yield return tableManager.LoadAllTableAsync();
			
			args.Description = "테이블 로드 완료";
		}

		private IEnumerator LoadIconManager(AsyncWorker.WorkArgs args)
		{
			args.Description = "리소스 로드 시작";

			CSpriteResourceManager.Instance.RegisterIconRootPath("Sprites/Icons/Test");

			yield return CSpriteResourceManager.Instance.Load(args.Dispatcher);

			args.Description = "리소스 로드 완료";

			yield return null;
		}

		public AsyncResult LoadAsyncAwaitTest()
		{
			var asyncResult = new AsyncResult();
			asyncResult.Add(async (args) =>
			{
				await Task.Delay(1000);
				args.Description = "테이블111  완료!!";
			});

			asyncResult.Add(async (args) =>
			{
				await Task.Delay(1000);
				args.Description = "테이블222  완료!!";
			});

			asyncResult.Add((args) =>
			{
				Thread.Sleep(1000);
				args.Description = "테이블333  완료!!";
			});

			asyncResult.Run();

			return asyncResult;
		}
	}
}
