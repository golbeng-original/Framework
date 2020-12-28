using UnityEngine;
using UnityEngine.Tilemaps;

namespace Golbeng.Framework._2D.Tile.ScriptableObject
{
	[CreateAssetMenu(fileName = "BlockTile", menuName = "2D/GolbengCustomTiles/BlockTile", order = 2)]
	public class CBlockAssestTile : TileBase
	{
		public Sprite Sprite;
		public Color Color = Color.white;
		public UnityEngine.Tilemaps.Tile.ColliderType TileColliderType;

		public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
		{
			tileData.transform = Matrix4x4.identity;
			tileData.color = Color;
			if (Sprite != null)
			{
				tileData.sprite = Sprite;
				tileData.colliderType = TileColliderType;
			}
		}
	}
}
