using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework._2D.Tile.Components
{
	public static class CObjectTileExtenstion
	{
		public static IEnumerable<Vector2Int> GetTileIndices<TTileState>(this CObjectTileComponent<TTileState> objectTile, CTileConfigure tileConfigure)
		{
			return tileConfigure.GetTileIndices(objectTile.transform.position, objectTile.TileSize);
		}

		public static IEnumerable<Vector2Int> GetExcludeTileIndices<TTileState>(this CObjectTileComponent<TTileState> objectTile, CTileConfigure tileConfigure)
		{
			var normalizeTilePosition = tileConfigure.NormalizeTilePosition(objectTile.transform.position, objectTile.TileSize);
			var basisCellIndex = tileConfigure.ConvertToTileIndex(normalizeTilePosition, objectTile.TileSize);

			var excludeTileIndicies = objectTile.ExcludeTileIndices.Select(t => {
				return new Vector2Int(t.x + basisCellIndex.x, t.y + basisCellIndex.y);
			});

			return excludeTileIndicies;
		}
	}
}
