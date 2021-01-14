using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.State
{
	[Serializable]
	public class CStateTransformInfo<TState>
	{
		public TState FromState;
		public TState ToState;
	}

	public class CStateEvnetAgentArgs
	{
		public object[] Arguments { get; set; } = new object[0];
		public bool Handled { get; set; } = false;
	}

	public partial class CStateEventAgent<TState>
	{
		public delegate bool StateEnterParamBehavoir(TState prevState, CStateEvnetAgentArgs args);
		public delegate bool StateEnterBehavior(TState prevState);
		public delegate void StateExitBehavior(TState nextState);

		protected class StateBehaviorInfo
		{
			public TState State { get; private set; }

			public event StateEnterParamBehavoir StateEnterParamBehavior;
			public event StateEnterBehavior StateEnterBehavior;

			public event StateExitBehavior StateExitBehavior;
		
			public StateBehaviorInfo(TState state)
			{
				State = state;
			}

			public bool OnEnterBehavior(TState prevState, CStateEvnetAgentArgs args)
			{
				if (StateEnterParamBehavior != null)
					return StateEnterParamBehavior(prevState, args);

				return StateEnterBehavior?.Invoke(prevState) ?? true;
			}

			public void OnExitBehavior(TState nextState)
			{
				StateExitBehavior?.Invoke(nextState);
			}

			public void ClearEnterStateBehavior()
			{
				StateEnterParamBehavior = null;
				StateEnterBehavior = null;
			}

			public void ClearExitStateBehavior()
			{
				StateExitBehavior = null;
			}
		}
	}

	public partial class CStateEventAgent<TState>
	{
		private HashSet<TState> _registerState = new HashSet<TState>();
		private Dictionary<TState, HashSet<TState>> _stateTransfroms = new Dictionary<TState, HashSet<TState>>();
		private Dictionary<TState, StateBehaviorInfo> _stateBehaviorInfos = new Dictionary<TState, StateBehaviorInfo>();

		public TState CurrentState { get; private set; } = default(TState);

		public bool IsCheckTransition { get; set; } = false;

		public void RegisterTransfromState(TState fromState, TState toState)
		{
			_registerState.Add(fromState);
			_registerState.Add(toState);

			if (_stateTransfroms.ContainsKey(fromState) == false)
			{
				_stateTransfroms.Add(fromState, new HashSet<TState>());
			}

			if (_stateTransfroms[fromState].Contains(toState) == true)
				return;

			_stateTransfroms[fromState].Add(toState);
		}

		private StateBehaviorInfo GetStateBehaviorInfo(TState state, bool needNew = true)
		{
			if (_stateBehaviorInfos.ContainsKey(state) == false)
			{
				if (needNew == false)
					return null;
				
				_stateBehaviorInfos.Add(state, new StateBehaviorInfo(state));
			}
			
			return _stateBehaviorInfos[state];
		}

		public void RegisterEnterBehavior(TState targetState, StateEnterParamBehavoir handler)
		{
			var beahaviorInfo = GetStateBehaviorInfo(targetState);
			beahaviorInfo.StateEnterParamBehavior += handler;
		}

		public void RegisterEnterBehavior(TState targetState, StateEnterBehavior handler)
		{
			var beahaviorInfo = GetStateBehaviorInfo(targetState);
			beahaviorInfo.StateEnterBehavior += handler;
		}

		public void UnregisterEnterBehavior(TState targetState)
		{
			var beahaviorInfo = GetStateBehaviorInfo(targetState);
			beahaviorInfo.ClearEnterStateBehavior();
		}

		public void RegisterExitBehavior(TState targetState, StateExitBehavior handler)
		{
			var beahaviorInfo = GetStateBehaviorInfo(targetState);
			beahaviorInfo.StateExitBehavior += handler;
		}

		public void UnregisterExitBehavior(TState targetState)
		{
			var beahaviorInfo = GetStateBehaviorInfo(targetState);
			beahaviorInfo.ClearExitStateBehavior();
		}

		private bool OnEnterBehavior(TState targetState, TState prevState, CStateEvnetAgentArgs args)
		{
			var behaviorInfo = GetStateBehaviorInfo(targetState, false);
			if (behaviorInfo == null)
				return true;

			return behaviorInfo.OnEnterBehavior(prevState, args);
		}

		private void OnExitBehavior(TState targetState, TState nextState)
		{
			var behaviorInfo = GetStateBehaviorInfo(targetState, false);
			if (behaviorInfo == null)
				return;

			behaviorInfo.OnExitBehavior(nextState);
		}

		public void InitializeState()
		{
			CurrentState = default(TState);
			UpdateForceState(CurrentState, null);
		}

		public bool UpdateForceState(TState state, CStateEvnetAgentArgs args)
		{
			var prevState = CurrentState;
			CurrentState = state;

			if (OnEnterBehavior(CurrentState, prevState, args) == false)
			{
				CurrentState = prevState;
				return false;
			}

			OnExitBehavior(prevState, CurrentState);

			return true;
		}

		public bool UpdateState(TState state, CStateEvnetAgentArgs args)
		{
			if(IsCheckTransition == true)
			{
				// From -> To State가 존재 하는가??
				if (_stateTransfroms.ContainsKey(CurrentState) == false)
					return false;

				if (_stateTransfroms[CurrentState].Contains(state) == false)
					return false;
			}

			if (CurrentState.Equals(state) == true)
				return false;

			return UpdateForceState(state, args);
		}
	}
}
