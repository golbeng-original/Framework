using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework._2D.Tile
{
	public class CTileConfigure
	{
		public enum TileCellAnchor
		{
			BottomLeft	// 현재 BoottomLeft Anchor 기준으로 CeillIndex 계산
		}

		public Vector2Int BasisTileSize { get; private set; }

		public CTileConfigure(Vector2Int basisTileSize)
		{
			BasisTileSize = basisTileSize;
		}

		public Vector3 ConvertToTilePosition(int x, int y)
		{
			return ConvertToTilePosition(new Vector2Int(x, y));
		}

		public Vector3 ConvertToTilePosition(Vector2Int cellIndex)
		{
			return ConvertToTilePosition(cellIndex, BasisTileSize);
		}

		public Vector3 ConvertToTilePosition(Vector2Int cellIndex, Vector2Int tileSize)
		{
			float resultX = cellIndex.x + (tileSize.x * 0.5f);
			float resultY = cellIndex.y + (tileSize.y * 0.5f);

			return new Vector3(resultX, resultY, 0.0f);
		}

		public Vector2Int ConvertToTileIndex(float x, float y, float z)
		{
			return ConvertToTileIndex(new Vector3(x, y, z));
		}

		public Vector2Int ConvertToTileIndex(Vector3 position)
		{
			return ConvertToTileIndex(position, BasisTileSize);
		}

		public Vector2Int ConvertToTileIndex(Vector3 position, Vector2Int tileSize)
		{
			float offsetX = tileSize.x * 0.5f;
			float offsetY = tileSize.y * 0.5f;

			var x = position.x - offsetX;
			x = Mathf.Round(x) + offsetX;

			var y = position.y - offsetY;
			y = Mathf.Round(y) + offsetY;

			return new Vector2Int(Mathf.CeilToInt(x - offsetX), Mathf.CeilToInt(y - offsetY));
		}

		public Vector3 NormalizeTilePosition(Vector3 position)
		{
			return NormalizeTilePosition(position, BasisTileSize);
		}

		public Vector3 NormalizeTilePosition(Vector3 position, Vector2Int tileSize)
		{
			var cellIndex = ConvertToTileIndex(position, tileSize);
			var normalizePosition = ConvertToTilePosition(cellIndex, tileSize);

			return new Vector3(normalizePosition.x, normalizePosition.y, position.z);
		}
	
		public IEnumerable<Vector2Int> GetTileIndices(Vector3 position, Vector2Int tileSize)
		{
			var normalizeTilePosition = NormalizeTilePosition(position, tileSize);

			var basisCellIndex = ConvertToTileIndex(normalizeTilePosition, tileSize);

			return GetTileIndices(basisCellIndex, tileSize);
		}

		public IEnumerable<Vector2Int> GetTileIndices(Vector2Int cellIndex, Vector2Int tileSize)
		{
			for (int x = 0; x < tileSize.x; x++)
			{
				for (int y = 0; y < tileSize.y; y++)
				{
					yield return new Vector2Int(cellIndex.x + x, cellIndex.y + y);
				}
			}
		}
	}
}
