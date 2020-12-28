using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Input
{
	public class CInputHelper
	{
		public static Collider2D GetCurrentHitCollider2D()
		{
			var hit = Physics2D.Raycast(GetTouchPoint(), Vector2.zero);
			if (hit.collider == null)
				return null;

			return hit.collider;
		}

		public static string GetCurrentHitTag()
		{
			var collider = GetCurrentHitCollider2D();
			if (collider == null)
				return "";

			return collider.gameObject.tag;
		}

		public static Vector3 GetTouchPoint(float zCoord = 0.0f)
		{
			return Camera.main.ScreenToWorldPoint(new Vector3(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y, zCoord));
		}
	}
}
