using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golbeng.Framework._2D.Tile.Components
{
	public abstract class CObjectTileComponent<TTileState> : MonoBehaviour
	{
		private CTileConfigure _tileConfigure = CTileAgent<TTileState>.TileConfigure;

		public Vector2Int TileSize;

		public List<Vector2Int> ExcludeTileIndices;

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

		public abstract TTileState TileType { get; }

		public abstract TTileState ExcludeTileType { get; }

		protected virtual void Awake() 
		{
			/*
			float adjustX = TileSize.x % 2 > 0 ? 0.0f : 0.5f;
			float adjustY = TileSize.y % 2 > 0 ? 0.0f : 0.5f;
			int middleX = TileSize.x / 2;
			int middleY = TileSize.y / 2;

			for (int x = 0; x < TileSize.x; x++)
			{
				for(int y = 0; y < TileSize.y; y++)
				{
					if (ExcludeTileIndices.Contains(new Vector2Int(x, y)) == true)
						continue;

					var boxCollider = gameObject.AddComponent<BoxCollider2D>();

					float offsetX = x - middleX + adjustX;
					float offsetY = y - middleY + adjustY;

					boxCollider.offset = new Vector2(offsetX, offsetY);
					boxCollider.size = new Vector2(1, 1);
				}
			}
			*/

			var boxCollider = gameObject.AddComponent<BoxCollider2D>();
			boxCollider.offset = new Vector2(0, 0);
			boxCollider.size = new Vector2(TileSize.x, TileSize.y);
		}

		protected virtual void OnApplicationQuit()
		{
			var boxColliders = gameObject.GetComponents<BoxCollider2D>();
			foreach (var boxCollider in boxColliders)
			{
				Destroy(boxCollider);
			}
		}

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
