using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Input
{
	public partial class CLongTouchGestureEvent
	{
		private HashSet<string> _touchableLayers = new HashSet<string>();

		public void RegisterTouchableLayer(params string[] layers)
		{
			foreach(var layer in layers)
			{
				_touchableLayers.Add(layer);
			}
		}
	}

	public partial class CLongTouchGestureEvent
	{
		private bool _beginTouch = false;
		private float _beginTouchTime = 0.0f;
		private string _beginTouchLayer = "";

		public float TimeSenstivity { get; set; } = 0.8f;
		public string TouchLayer { get; private set; } = "";

		public GameObject TouchGameObject { get; private set; } = null;

		private void Clear()
		{
			_beginTouch = false;
			_beginTouchTime = 0.0f;

			TouchLayer = "";
			TouchGameObject = null;
		}

		public bool UpdateLongTouchState()
		{
			if(UnityEngine.Input.GetMouseButtonUp(0) == true)
			{
				Clear();
				return false;
			}

			if(_beginTouch == false)
			{
				if(UnityEngine.Input.GetMouseButtonDown(0) == true)
				{
					_beginTouchLayer = CInputHelper.GetCurrentHitLayer();

					if (_touchableLayers.Contains(_beginTouchLayer) == true)
					{
						_beginTouch = true;
						_beginTouchTime = Time.time;
					}
				}
			}
			else
			{
				var gapTime = Time.time - _beginTouchTime;
				if (gapTime >= TimeSenstivity)
				{
					var lastTouchLayer = CInputHelper.GetCurrentHitLayer();
					if(_beginTouchLayer.Equals(lastTouchLayer) == true)
					{
						TouchGameObject = CInputHelper.GetCurrentHitGameObject();
						TouchLayer = _beginTouchLayer;

						_beginTouch = false;

						return true;
					}
				}
			}

			return false;
		}
	}
}
