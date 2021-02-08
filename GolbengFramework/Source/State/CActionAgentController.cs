using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.State
{
	public partial class CActionAgentController : MonoBehaviour
	{
		public abstract class CAction
		{
			private CActionAgentController _actionAgent;
			private IEnumerator _corutine = null;
			private bool _isPause = false;

			public bool isRunning { get => _corutine != null ? true : false; }

			internal void Initialize(CActionAgentController actionAgent)
			{
				_actionAgent = actionAgent;

				OnInitialize();
			}

			protected virtual void OnInitialize() { }

			public void Start(params object[] param)
			{
				_isPause = false;
				_actionAgent.RegisterStartAction(this);

				_corutine = Running(param);
				_actionAgent.StartCoroutine(_corutine);
			}

			protected virtual void OnPause() { }

			internal void Pause()
			{
				OnPause();
				_isPause = true;
			}

			protected virtual void OnResume() { }

			internal void Resume()
			{
				OnResume();
				_isPause = false;
			}

			public void Stop()
			{
				OnStop();

				_actionAgent.UnregisterStartAction(this);

				if (_corutine != null)
				{
					_actionAgent.StopCoroutine(_corutine);
					_corutine = null;
				}

				_isPause = false;
			}

			protected virtual void OnStop() { }

			protected Coroutine Yield(YieldInstruction instruction = null)
			{
				return _actionAgent.StartCoroutine(_Yield(instruction));
			}

			protected Coroutine Yield(IEnumerator corutine)
			{
				return _actionAgent.StartCoroutine(_Yield(corutine));
			}

			private IEnumerator _Yield(YieldInstruction instruction = null)
			{
				while (_isPause == true)
				{
					yield return null;
				}

				yield return instruction;
			}

			private IEnumerator _Yield(IEnumerator corutine)
			{
				while (_isPause == true)
				{
					yield return null;
				}

				yield return corutine;
			}

			private IEnumerator Running(params object[] param)
			{
				// 정지 중일 수 있다..
				yield return Yield();

				OnBegin();

				yield return OnRun(param);

				OnEnd();

				_actionAgent.UnregisterStartAction(this);

				_corutine = null;
			}

			public abstract IEnumerator OnRun(params object[] param);
			protected virtual void OnBegin() { }
			protected virtual void OnEnd() { }
		}
	}

	public partial class CActionAgentController : MonoBehaviour
	{
		private List<CAction> _currentPlayActions = new List<CAction>();
		private List<CAction> _registerActions = new List<CAction>();

		public bool IsPause { get; private set; } = false;

		public bool IsExistRunning { get => _currentPlayActions.Where(a => a.isRunning == true).Any(); }

		protected virtual void Awake() {}

		protected virtual void Start() {}

		public T GetAction<T>() where T : CAction
		{
			foreach (var action in _registerActions)
			{
				if (action is T)
					return action as T;
			}

			return null;
		}

		public void RegisterNormalAction(CAction action)
		{
			action.Initialize(this);
			_registerActions.Add(action);
		}

		public void StopActionAll(CAction ignoreAction = null)
		{
			foreach (var action in _registerActions)
			{
				if (action.isRunning == true)
				{
					if (action == ignoreAction)
						continue;

					action.Stop();
				}
			}
		}

		public void PauseActions()
		{
			IsPause = true;

			foreach (var action in _currentPlayActions)
			{
				action.Pause();
			}
		}

		public void ResumeActions()
		{
			IsPause = false;

			foreach (var action in _currentPlayActions)
			{
				action.Resume();
			}
		}

		internal void RegisterStartAction(CAction action)
		{
			if (IsPause == true)
				action.Pause();

			_currentPlayActions.Add(action);
		}

		internal void UnregisterStartAction(CAction action)
		{
			_currentPlayActions.Remove(action);
		}
	}

}
