using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.State
{
	public class CStateMachineBehavior : StateMachineBehaviour
	{
		private CStateActionAgentController GetCustomerStateDefine(Animator animator)
		{
			return animator.gameObject.GetComponent<CStateActionAgentController>();
		}

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			UpdateStateDefine(animator);

			base.OnStateEnter(animator, stateInfo, layerIndex);
		}

		private void UpdateStateDefine(Animator animator)
		{
			var stateDefine = GetCustomerStateDefine(animator);
			if (stateDefine != null)
			{
				var methodInfo = stateDefine.GetType().GetMethod("UpdateStateFromMecanim", BindingFlags.Public | BindingFlags.Instance);
				if (methodInfo != null)
				{
					var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
					foreach (var field in fields)
					{
						var genericMethod = methodInfo.MakeGenericMethod(field.FieldType);
						genericMethod.Invoke(stateDefine, new[] { field.GetValue(this) });
					}
				}
			}
		}
	}
}