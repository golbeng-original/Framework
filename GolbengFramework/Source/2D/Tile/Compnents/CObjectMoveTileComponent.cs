using Golbeng.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework._2D.Tile.Components
{
	public delegate void MouseDownEventHandler(CObjectMoveTileComponent moveTIleComponent);
	public delegate bool MouseUpEventHandler(CObjectMoveTileComponent moveTIleComponent);
	public delegate void MouseEventFinishHandler(CObjectMoveTileComponent moveTIleComponent);

	public class CObjectMoveTileComponent : CObjectTileComponent
	{
		private Vector3 _touchOffset;
		private Vector3 _prevTilePosition;

		public event MouseDownEventHandler MouseDownEventHandler;
		public event MouseUpEventHandler MouseUpEventHandler;
		public event MouseEventFinishHandler MouseEventFinishHandler;

		private Vector3 GetTouchPosition()
		{
			var screenPos = Camera.main.WorldToScreenPoint(transform.position);
			return CInputHelper.GetTouchPoint(screenPos.z);
		}

		private void OnMouseDown()
		{
			_prevTilePosition = transform.position;

			var touchPos = GetTouchPosition();
			_touchOffset = transform.position - touchPos;

			if (MouseDownEventHandler != null)
				MouseDownEventHandler(this);

		}
		private void OnMouseDrag()
		{
			var touchPos = GetTouchPosition();
			transform.position = touchPos + _touchOffset;
		}
		private void OnMouseUp()
		{
			AdjustPosition(transform.position);

			if(MouseUpEventHandler != null)
			{
				bool complete = MouseUpEventHandler(this);
				if(complete == false)
				{
					AdjustPosition(_prevTilePosition);
				}
			}

			if(MouseEventFinishHandler != null)
			{
				MouseEventFinishHandler(this);
			}
		}
	}
}
