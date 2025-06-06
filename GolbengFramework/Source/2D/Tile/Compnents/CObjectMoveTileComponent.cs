﻿using Golbeng.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework._2D.Tile.Components
{
	public delegate void MouseDownEventHandler<TTileState>(CObjectMoveTileComponent<TTileState> moveTIleComponent);
	public delegate void MouseDragEventHandler<TTileState>(CObjectMoveTileComponent<TTileState> moveTIleComponent);
	public delegate bool MouseUpEventHandler<TTileState>(CObjectMoveTileComponent<TTileState> moveTIleComponent);
	public delegate void MouseEventFinishHandler<TTileState>(CObjectMoveTileComponent<TTileState> moveTIleComponent);

	public abstract class CObjectMoveTileComponent<TTileState> : CObjectTileComponent<TTileState>
	{
		private Vector3 _touchOffset;
		private Vector3 _prevTilePosition;
		private bool _isEnableMouseEvent = false;

		public event MouseDownEventHandler<TTileState> MouseDownEventHandler;
		public event MouseDragEventHandler<TTileState> MouseDragEventHandler;
		public event MouseUpEventHandler<TTileState> MouseUpEventHandler;
		public event MouseEventFinishHandler<TTileState> MouseEventFinishHandler;

		public Action<bool> EnableMouseEventHandler { get; set; }

		public bool IsMoveable { get; }

		public bool IsEnableMouseEvent 
		{
			get => _isEnableMouseEvent;
			set
			{
				EnableMouseEventHandler?.Invoke(value);
				_isEnableMouseEvent = value;
			}
		}

		public CObjectMoveTileComponent(bool isMoveable)
		{
			IsMoveable = isMoveable;
		}

		private Vector3 GetTouchPosition()
		{
			var screenPos = Camera.main.WorldToScreenPoint(transform.position);
			return CInputHelper.GetTouchPoint(screenPos.z);
		}

		private void OnMouseDown()
		{
			if (IsMoveable == false)
				return;

			if (IsEnableMouseEvent == false)
				return;

			_prevTilePosition = transform.position;

			var touchPos = GetTouchPosition();
			_touchOffset = transform.position - touchPos;

			MouseDownEventHandler?.Invoke(this);
		}
		private void OnMouseDrag()
		{
			if (IsMoveable == false)
				return;

			if (IsEnableMouseEvent == false)
				return;

			var touchPos = GetTouchPosition();
			transform.position = touchPos + _touchOffset;

			MouseDragEventHandler?.Invoke(this);
		}
		private void OnMouseUp()
		{
			if (IsMoveable == false)
				return;

			if (IsEnableMouseEvent == false)
				return;

			AdjustPosition(transform.position);

			if(MouseUpEventHandler != null)
			{
				bool complete = MouseUpEventHandler(this);
				if(complete == false)
				{
					AdjustPosition(_prevTilePosition);
				}
			}

			MouseEventFinishHandler?.Invoke(this);
		}
	}
}
