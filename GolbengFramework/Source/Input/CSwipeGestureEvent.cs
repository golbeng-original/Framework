using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Input
{
	public enum GestureDirection
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	public delegate void SwipeHandler(CSwipeGestureEvent sender, GestureDirection direction);

	public class CSwipeGestureEvent
	{
		private bool _isBeginSwipeEvent = false;
		private float _beginTouchTime = 0.0f;

		public float RadiusRange { get; set; } = 0.15f;
		public float SwipeSensitivity { get; set; } = 0.15f;
		public float TimeSensitivity { get; set; } = 0.3f;

		public Vector3 BeginTouchPos { get; private set; }
		public Vector3 EndTouchPos { get; private set; }
		public GestureDirection Direction { get; private set; } = GestureDirection.None;

		public event SwipeHandler SwipeHandler;

		private void Clear()
		{
			_isBeginSwipeEvent = false;
			BeginTouchPos = Vector3.zero;
			EndTouchPos = Vector3.zero;

			_beginTouchTime = 0.0f;
			Direction = GestureDirection.None;
		}

		public bool UpdateSwipeState()
		{
			if (UnityEngine.Input.GetMouseButtonDown(0))
			{
				//var hitTag = UInputManager.GetCurrentHitTag();
				//if (hitTag.Equals("MoveableTile") == true)
				//	return false;

				Clear();

				BeginTouchPos = UnityEngine.Input.mousePosition;
				_beginTouchTime = Time.time;
				_isBeginSwipeEvent = true;
			}
			else if (UnityEngine.Input.GetMouseButtonUp(0))
			{
				if (_isBeginSwipeEvent == false)
					return false;

				_isBeginSwipeEvent = false;

				var gapTime = Time.time - _beginTouchTime;
				if (gapTime >= TimeSensitivity)
					return false;

				EndTouchPos = UnityEngine.Input.mousePosition;

				var distance = Vector3.Distance(EndTouchPos, BeginTouchPos);
				var distanceRatio = distance / Screen.height;

				if (distanceRatio <= SwipeSensitivity)
					return false;

				var vec = EndTouchPos - BeginTouchPos;
				var radius = Mathf.Atan2(vec.x, vec.y);
				radius = radius < 0.0f ? (Mathf.PI * 2) + radius : radius;

				Direction = GestureDirection.None;

				if (Mathf.Abs(radius) < 0.15f || Mathf.Abs(Mathf.PI * 2.0f - radius) < 0.15f)
				{
					Direction = GestureDirection.Up;
				}
				else if (Mathf.Abs(Mathf.PI * 0.5f - radius) < 0.15f)
				{
					Direction = GestureDirection.Right;
				}
				else if (Mathf.Abs(Mathf.PI - radius) < 0.15f)
				{
					Direction = GestureDirection.Down;
				}
				else if (Mathf.Abs(Mathf.PI * 1.5f - radius) < 0.15f)
				{
					Direction = GestureDirection.Left;
				}

				if(Direction != GestureDirection.None)
				{
					SwipeHandler?.Invoke(this, Direction);
					return true;
				}

				return false;
			}

			return false;
		}
	}
}
