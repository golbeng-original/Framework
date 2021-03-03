using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.State
{
	public abstract class CStateMachineBehavior : StateMachineBehaviour
	{
		private CStateActionAgentStateUpdater _stateUpdater = null;

		private CStateActionAgentController GetStateActionAgentController(Animator animator)
		{
			return animator.gameObject.GetComponent<CStateActionAgentController>();
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			UpdateStateDefine(animator);

			base.OnStateEnter(animator, stateInfo, layerIndex);
		}

		protected abstract void UpdateState(CStateActionAgentStateUpdater stateUpdater);

		private void UpdateStateDefine(Animator animator)
		{
			if(_stateUpdater == null)
			{
				var controller = GetStateActionAgentController(animator);
				if (controller == null)
					return;

				_stateUpdater = new CStateActionAgentStateUpdater(controller);
			}

			if (_stateUpdater == null)
				return;

			UpdateState(_stateUpdater);
		}
	}
}