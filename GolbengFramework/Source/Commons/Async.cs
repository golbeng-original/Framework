using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Golbeng.Framework.Commons
{
	public class AsyncResult
	{
		private class AsyncJob
		{
			public Action<ProgressDescirption> NormalAction { get; set; } = null;
			public Func<ProgressDescirption, Task> TaskAction { get; set; } = null;
		}

		public class ProgressDescirption
		{
			BackgroundWorker _worker;
			public ProgressDescirption(BackgroundWorker worker)
			{
				_worker = worker;
			}

			public string Description
			{
				set => _worker.ReportProgress(-1, value);
			}
		}


		private List<AsyncJob> _jobList = new List<AsyncJob>();

		private BackgroundWorker _worker;

		public bool IsDone { get; private set; } = false;

		public int Percent { get; private set; } = 0;

		public string Description { get; set; } = "";

		public Exception Exception { get; private set; } = null;

		public AsyncResult() { }

		public AsyncResult(Action<ProgressDescirption> action)
		{
			Add(action);
		}

		private void OnAction()
		{
			IsDone = false;
			Percent = 0;

			_worker = new BackgroundWorker();
			_worker.WorkerReportsProgress = true;

			_worker.DoWork += (sender, e) =>
			{
				foreach (var job in _jobList)
				{
					int percent = (int)((float)(_jobList.IndexOf(job) + 1) / (float)_jobList.Count * 100.0f);
					_worker.ReportProgress(percent, null);

					if (job.NormalAction != null)
					{
						job.NormalAction(new ProgressDescirption(_worker));
					}
					else if (job.TaskAction != null)
					{
						job.TaskAction(new ProgressDescirption(_worker)).Wait();
					}
				}
			};

			_worker.RunWorkerCompleted += (sender, e) =>
			{
				Percent = 100;
				IsDone = true;
				Exception = e.Error;
			};

			_worker.ProgressChanged += (sender, e) =>
			{
				if(e.ProgressPercentage >= 0)
					Percent = e.ProgressPercentage;

				if(e.UserState != null)
					Description = e.UserState as string;
			};

			_worker.RunWorkerAsync();
		}

		public void Add(Action<ProgressDescirption> job)
		{
			_jobList.Add(new AsyncJob()
			{
				NormalAction = job
			});
		}

		public void Add(Func<ProgressDescirption, Task> job)
		{
			_jobList.Add(new AsyncJob()
			{
				TaskAction = job
			});
		}

		public void Run()
		{
			OnAction();
		}

		public static AsyncResult Run(Action<ProgressDescirption> job)
		{
			AsyncResult actionResult = new AsyncResult();
			actionResult.Add(job);

			actionResult.OnAction();

			return actionResult;
		}

		public static AsyncResult Run(Func<ProgressDescirption, Task> job)
		{
			AsyncResult actionResult = new AsyncResult();
			actionResult.Add(job);

			actionResult.OnAction();

			return actionResult;
		}
	}
}
