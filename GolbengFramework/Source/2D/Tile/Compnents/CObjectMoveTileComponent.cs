using Golbeng.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework._2D.Tile.Components
{
	public delegate void MouseDownEventHandler<TTileState>(CObjectMoveTileComponent<TTileState> moveTIleComponent);
	public delegate bool MouseUpEventHandler<TTileState>(CObjectMoveTileComponent<TTileState> moveTIleComponent);
	public delegate void MouseEventFinishHandler<TTileState>(CObjectMoveTileComponent<TTileState> moveTIleComponent);

	public class CObjectMoveTileComponent<TTileState> : CObjectTileComponent<TTileState>
	{
		private Vector3 _touchOffset;
		private Vector3 _prevTilePosition;

		public event MouseDownEventHandler<TTileState> MouseDownEventHandler;
		public event MouseUpEventHandler<TTileState> MouseUpEventHandler;
		public event MouseEventFinishHandler<TTileState> MouseEventFinishHandler;

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
