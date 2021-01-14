using System;
using UnityEngine;

namespace Golbeng.Framework._2D.Tile.Components
{
	public class CObjectTileComponent<TTileState> : MonoBehaviour
	{
		private CTileConfigure _tileConfigure = CTileAgent<TTileState>.TileConfigure;

		public Vector2Int TileSize;

		public virtual Vector2Int CellIndex
		{
			set
			{
				transform.position = _tileConfigure.ConvertToTilePosition(value, TileSize);
			}

			get
			{
				return _tileConfigure.ConvertToTileIndex(transform.position, TileSize);
			}
		}

		public virtual TTileState TileType { get; } = default(TTileState);

		protected virtual void Awake() {}

		protected virtual void Start()
		{
			AdjustPosition(transform.position);
		}
		protected virtual void Update()
		{

		}
		protected void AdjustPosition(Vector3 position)
		{
			transform.position = _tileConfigure.NormalizeTilePosition(position, TileSize);
		}

	}
}
