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
	}
}
