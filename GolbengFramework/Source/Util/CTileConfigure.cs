using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Util
{
	public class CTileConfigure
	{
		public Vector2 TileCenterBias { get; private set; }

		public CTileConfigure(Vector2 cetnerBias)
		{
			TileCenterBias = cetnerBias;
		}

		public Vector3 ConvertToTilePosition(int x, int y)
		{
			return ConvertToTilePosition(new Vector2Int(x, y));
		}

		public Vector3 ConvertToTilePosition(Vector2Int cellIndex)
		{
			return new Vector3(TileCenterBias.x + cellIndex.x, TileCenterBias.y + cellIndex.y, 0.0f);
		}

		public Vector2Int ConvertToTileIndex(float x, float y, float z)
		{
			return ConvertToTileIndex(new Vector3(x, y, z));
		}

		public Vector2Int ConvertToTileIndex(Vector3 position)
		{
			var x = position.x - TileCenterBias.x;
			x = Mathf.Round(x) + TileCenterBias.x;

			var y = position.y - TileCenterBias.y;
			y = Mathf.Round(y) + TileCenterBias.y;

			return new Vector2Int(Mathf.CeilToInt(x - TileCenterBias.x), Mathf.CeilToInt(y - TileCenterBias.y));
		}

		public Vector3 NormalizeTilePosition(Vector3 position)
		{
			var cellIndex = ConvertToTileIndex(position);
			return ConvertToTilePosition(cellIndex);
		}

	}
}
