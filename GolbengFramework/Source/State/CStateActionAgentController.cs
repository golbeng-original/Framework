using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.State
{
	public partial class CStateActionAgentController : CActionAgentController
	{
		private bool _isEnable = false;

		private Dictionary<Type, object> _stateEventAgnetMapping = new Dictionary<Type, object>();
		private Dictionary<Type, Func<bool>> _stateChangePredicateMapping = new Dictionary<Type, Func<bool>>();
		private Dictionary<(Type type, int state), object[]> _stateChangeParameters = new Dictionary<(Type type, int state), object[]>();

		//
		private Dictionary<(Type Type, int Value), CAction> _stateEventActionMapping = new Dictionary<(Type Type, int Value), CAction>();
		public Animator Animator { get; private set; }
		public bool IsEnable
		{
			get => _isEnable;
			set
			{
				_isEnable = value;
				Animator.enabled = value;
			}
		}


		protected override void Awake()
		{
			Animator = GetComponent<Animator>();
			
			IsEnable = false;
		}

		public void InitializeStateEventAgent()
		{
			foreach (var stateAgent in _stateEventAgnetMapping.Values)
			{
				var methodInfo = stateAgent.GetType().GetMethod("InitializeState", BindingFlags.Public | BindingFlags.Instance);
				if (methodInfo == null)
					continue;

				methodInfo.Invoke(stateAgent, null);
			}
		}

		public void InistalizeEventParameter()
		{
			foreach (var parameter in Animator.parameters)
			{
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Bool:
						Animator.SetBool(parameter.name, parameter.defaultBool);
						break;
					case AnimatorControllerParameterType.Int:
						Animator.SetInteger(parameter.name, parameter.defaultInt);
						break;
					case AnimatorControllerParameterType.Float:
						Animator.SetFloat(parameter.name, parameter.defaultFloat);
						break;
				}
			}
		}

		public CStateEventAgent<T> GetStateEventAgent<T>() where T : Enum
		{
			Type type = typeof(T);
			if (_stateEventAgnetMapping.ContainsKey(type) == true)
				return _stateEventAgnetMapping[type] as CStateEventAgent<T>;

			var newStateEventAgent = new CStateEventAgent<T>();
			_stateEventAgnetMapping.Add(type, newStateEventAgent);

			return newStateEventAgent;
		}

		public void RegisterEnterBehavior<T>(T type, CStateEventAgent<T>.StateEnterParamBehavoir handler) where T : Enum
		{
			var stateEventAgent = GetStateEventAgent<T>();
			if (stateEventAgent == null)
				return;

			stateEventAgent.RegisterEnterBehavior(type, handler);
		}

		public void RegisterEnterBehavior<T>(T type, CStateEventAgent<T>.StateEnterBehavior handler) where T : Enum
		{
			var stateEventAgent = GetStateEventAgent<T>();
			if (stateEventAgent == null)
				return;

			stateEventAgent.RegisterEnterBehavior(type, handler);
		}

		public void RegisterExitBehavior<T>(T type, CStateEventAgent<T>.StateExitBehavior handler) where T : Enum
		{
			var stateEventAgent = GetStateEventAgent<T>();
			if (stateEventAgent == null)
				return;

			stateEventAgent.RegisterExitBehavior(type, handler);
		}

		public void RegisterUpdateEventPredicate<T>(Func<bool> func)
		{
			var type = typeof(T);
			if (_stateChangePredicateMapping.ContainsKey(type) == true)
				return;

			_stateChangePredicateMapping.Add(type, func);
		}

		public void RegisterStateDependencyAction<T>(T state, CAction action) where T : Enum
		{
			GetStateEventAgent<T>();

			var key = (typeof(T), Convert.ToInt32(state));
			if (_stateEventActionMapping.ContainsKey(key) == true)
				return;

			_stateEventActionMapping.Add(key, action);
			RegisterNormalAction(action);
		}

		public T GetCurrentState<T>() where T : Enum
		{
			var stateAgent = GetStateEventAgent<T>();
			if (stateAgent == null)
				return default(T);

			return stateAgent.CurrentState;
		}

		public void UpdateState<T>(T state)
		{
			Type type = typeof(T);
			if (_stateEventAgnetMapping.ContainsKey(type) == false)
				return;

			var stateAgent = _stateEventAgnetMapping[type] as CStateEventAgent<T>;
			if (stateAgent == null)
				return;

			CStateEvnetAgentArgs args = new CStateEvnetAgentArgs();
			bool isUpdateState = false;

			var key = (type, Convert.ToInt32(state));
			if (_stateChangeParameters.ContainsKey(key) == true)
			{
				var parameters = _stateChangeParameters[key];
				_stateChangeParameters.Remove(key);

				args.Arguments = parameters;

				isUpdateState = stateAgent.UpdateState(state, args);
			}
			else
			{
				isUpdateState = stateAgent.UpdateState(state, args);
			}

			if(isUpdateState == true && args.Handled == false)
			{
				if(_stateEventActionMapping.ContainsKey(key) == true)
					_stateEventActionMapping[key].Start(args.Arguments);
			}
		}

		// CStateMachineBehavior로 호출 되었다..
		public void UpdateStateFromMecanim<T>(T state) where T : Enum
		{
			UpdateState(state);
		}

		public void UpdateEvent<T>(string animteParameterName, T state) where T : Enum
		{
			var type = typeof(T);
			if (_stateEventAgnetMapping.ContainsKey(type) == false)
				return;

			if (_stateChangePredicateMapping.ContainsKey(type) == true)
			{
				// 조건에서 실패함..
				if (_stateChangePredicateMapping[type].Invoke() == false)
				{
					Debug.Log($"UpdateState Fail Predicate false paramName = {animteParameterName}, value = {state}");
					return;
				}
			}

			Animator.SetInteger(animteParameterName, Convert.ToInt32(state));
		}

		public void UpdateEvent<T>(string animteParameterName, T state, params object[] parameters) where T : Enum
		{
			var type = typeof(T);
			if (_stateEventAgnetMapping.ContainsKey(type) == false)
				return;

			if (_stateChangePredicateMapping.ContainsKey(type) == true)
			{
				// 조건에서 실패함..
				if (_stateChangePredicateMapping[type].Invoke() == false)
				{
					Debug.Log($"UpdateState Feil Predicate false paramName = {animteParameterName}, value = {state}");
					return;
				}
			}

			int intState = Convert.ToInt32(state);

			Animator.SetInteger(animteParameterName, intState);

			if (parameters.Length > 0)
			{
				var paramKey = (type, intState);
				_stateChangeParameters[paramKey] = parameters;
			}
		}
	}
}
