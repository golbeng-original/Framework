using UnityEngine;

namespace Golbeng.Framework._2D.Tile.Components
{
	public class CObjectTileComponent : MonoBehaviour
	{
		private CTileConfigure _tileConfigure = CTileAgent.TileConfigure;

		public Vector2Int TileSize;

		public int TileType;

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
