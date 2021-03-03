using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Commons
{
	public delegate IEnumerator JobActionHandler(AsyncWorker.WorkArgs args);
	public class WaitForTasks : CustomYieldInstruction
	{
		private bool _isComplete = false;
		private Action _action;
		private Func<Task> _taskAction;

		public bool IsDone { get => _isComplete == true ? true : false; }
		public override bool keepWaiting { get => _isComplete == false ? true : false; }

		public WaitForTasks(Action action)
		{
			_action = action;
			_isComplete = false;

			Task.Run(() =>
			{
				_action();
				_isComplete = true;
			});
		}

		public WaitForTasks(Func<Task> action)
		{
			_taskAction = action;
			_isComplete = false;

			Task.Run(async () =>
			{
				await _taskAction();
				_isComplete = true;
			});
		}
	}

	public class WaitForCoroutineFunc<T> : CustomYieldInstruction
	{
		private bool _isComplete = false;
		
		public T Result { get; private set; }
		public bool IsDone { get => _isComplete == true ? true : false; }
		public override bool keepWaiting { get => _isComplete == false ? true : false; }

		public WaitForCoroutineFunc(MonoBehaviour dispatcher, Func<T> func)
		{
			_isComplete = false;
			dispatcher.StartCoroutine(Run(func));
		}
		private IEnumerator Run(Func<T> func)
		{
			Result = func();

			yield return null;
			_isComplete = true;
		}
	}

	public class WaitFroCoroutineAction : CustomYieldInstruction
	{
		private bool _isComplete = false;

		public bool IsDone { get => _isComplete == true ? true : false; }
		public override bool keepWaiting { get => _isComplete == false ? true : false; }

		public WaitFroCoroutineAction(MonoBehaviour dispatcher, Action action)
		{
			_isComplete = false;
			dispatcher.StartCoroutine(Run(action));
		}
		private IEnumerator Run(Action action)
		{
			action();
			yield return null;
			_isComplete = true;
		}
	}


	public class WaitForCoroutineTasks : CustomYieldInstruction
	{
		private class WaitForUnit : CustomYieldInstruction
		{
			private bool _isComplete = false;
			private Action _action = null;

			public bool IsDone { get => _isComplete == true ? true : false; }
			public override bool keepWaiting { get => _isComplete == false ? true : false; }

			public WaitForUnit(MonoBehaviour diaptcher, Action action)
			{
				_action = action;
				_isComplete = false;

				diaptcher.StartCoroutine(Run());
			}

			private IEnumerator Run()
			{
				_action?.Invoke();
				yield return null;

				_isComplete = true;
			}
		}

		//
		private MonoBehaviour _dispatcher = null;

		private bool _isComplete = false;
		private List<Action> _actions = new List<Action>();
		private List<WaitForUnit> _actionProvider = new List<WaitForUnit>();

		public bool IsDone { get => _isComplete == true ? true : false; }
		public override bool keepWaiting { get => _isComplete == false ? true : false; }

		public WaitForCoroutineTasks(MonoBehaviour dispatcher)
		{
			_dispatcher = dispatcher;
		}

		public void RegisterTask(Action action)
		{
			_actions.Add(action);
		}

		public void Run()
		{
			foreach (var action in _actions)
			{
				var unit = new WaitForUnit(_dispatcher, action);
				_actionProvider.Add(unit);
			}

			_dispatcher.StartCoroutine(_IsComplete());
		}

		private IEnumerator _IsComplete()
		{
			foreach (var provider in _actionProvider)
			{
				yield return provider;
			}

			_isComplete = true;
		}
	}
	public class AsyncWorker : CustomYieldInstruction
	{
		public class WorkArgs
		{
			private AsyncWorker _targetWorker;

			public WorkArgs(AsyncWorker worker, MonoBehaviour dispatcher)
			{
				_targetWorker = worker;
				Dispatcher = dispatcher;
			}

			public MonoBehaviour Dispatcher { get; private set; }
			public string Description
			{
				set => _targetWorker.ProgressDescription = value;
			}
			public bool Cancel { get; set; } = false;
		}

		public List<JobActionHandler> JobList = new List<JobActionHandler>();

		public override bool keepWaiting { get => IsDone == false ? true : false; }

		public bool IsDone { get; private set; } = false;
		public bool IsCancel { get; private set; } = false;

		public float Progress { get; private set; } = 0.0f;

		public string ProgressDescription { get; private set; } = "";

		public void Run(MonoBehaviour dispatcher)
		{
			dispatcher.StartCoroutine(_Run(dispatcher));
		}

		private IEnumerator _Run(MonoBehaviour dispatcher)
		{
			WorkArgs args = new WorkArgs(this, dispatcher);
			foreach (var job in JobList)
			{
				Progress = (int)((float)(JobList.IndexOf(job) + 1) / (float)JobList.Count * 100.0f);

				yield return job(args);

				if (args.Cancel == true)
				{
					IsDone = true;
					IsCancel = true;
					yield break;
				}
			}

			IsDone = true;
		}
	}

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
