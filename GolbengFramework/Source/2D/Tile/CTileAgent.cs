using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Golbeng.Framework.Algorithm;
using Golbeng.Framework._2D.Tile.Components;

namespace Golbeng.Framework._2D.Tile
{
	public partial class CTileAgent<TTileState>
	{
		private static readonly Vector2Int[] ReleativeNeighborPos = {
			new Vector2Int(0, 1),	//↑
			new Vector2Int(-1, 0),	// ←
			new Vector2Int(1, 0),	// →
			new Vector2Int(0, -1),	// ↓
		};

		private static readonly Vector2Int[] ReleativeNeighborObstaclePos = {
			new Vector2Int(1, 1),	// ↗
			new Vector2Int(1, -1),	// ↘
			new Vector2Int(-1, -1),	// ↙
			new Vector2Int(-1, 1),	// ↖
		};
	}

	public partial class CTileAgent<TTileState>
	{
		public static CTileConfigure TileConfigure { get; set; } = new CTileConfigure(new Vector2Int(1, 1));
	
		public CTile2DPathFinder<TTileState> MakePathFinder(params TTileState[] addtionBlockValues)
		{
			var newPathFinder = new CTile2DPathFinder<TTileState>(PathMap);

			foreach(var block in BlockTiles)
			{
				newPathFinder.BlockValues.Add(block);
			}

			foreach(var block in addtionBlockValues)
			{
				newPathFinder.BlockValues.Add(block);
			}

			return newPathFinder;
		}
	}

	public partial class CTileAgent<TTileState>
	{
		private Dictionary<Vector2Int, TTileState> _backgroundTileState = new Dictionary<Vector2Int, TTileState>();

		private HashSet<CObjectTileComponent<TTileState>> _registedObjectTiles = new HashSet<CObjectTileComponent<TTileState>>();
		private Dictionary<Vector2Int, CObjectTileComponent<TTileState>> _registedTileState = new Dictionary<Vector2Int, CObjectTileComponent<TTileState>>();

		public bool UsePathMap { get; set; } = true;
		public CTile2DPathFinder<TTileState>.PathMap PathMap { get; set; } = new CTile2DPathFinder<TTileState>.PathMap();

		public HashSet<TTileState> BlockTiles { get; private set; } = new HashSet<TTileState>();

		public void UpdateBackgroundTileType(Tilemap tileMap, Func<TileBase, TTileState> tileStateFunc)
		{
			if (tileMap == null)
				return;

			_backgroundTileState.Clear();

			var bounds = tileMap.cellBounds;
			foreach(var position in bounds.allPositionsWithin)
			{
				var tile = tileMap.GetTile(position);
				if (tile == null)
					continue;

				var state = tileStateFunc(tile);
				_backgroundTileState[new Vector2Int(position.x, position.y)] = state;

				if (UsePathMap == true)
					PathMap[position.x, position.y] = state;
			}
		}

		public bool IsRegisterObjectTile(CObjectTileComponent<TTileState> objectTile)
		{
			foreach(var position in objectTile.GetTileIndices(TileConfigure))
			{
				if (IsRegisterCellIndex(position) == false)
					return false;
			}

			return true;
		}

		public bool IsRegisterCellIndex(Vector2Int cellIndex)
		{
			var tileState = GetTileState(cellIndex);
			return BlockTiles.Contains(tileState) == true ? false : true; 
		}

		public bool RegisterObjectTile(CObjectTileComponent<TTileState> objectTile)
		{
			if (_registedObjectTiles.Contains(objectTile) == true)
				return false;

			if (IsRegisterObjectTile(objectTile) == false)
				return false;

			_registedObjectTiles.Add(objectTile);

			foreach(var position in objectTile.GetTileIndices(TileConfigure))
			{
				_registedTileState.Add(position, objectTile);

				if (UsePathMap == true)
					PathMap[position.x, position.y] = GetTileState(position);
			}

			return true;
		}

		public void UnregisterObjectTile(CObjectTileComponent<TTileState> objectTile)
		{
			_registedObjectTiles.Remove(objectTile);

			List<Vector2Int> removeCellIndices = new List<Vector2Int>();
			foreach(var kv in _registedTileState)
			{
				if(kv.Value == objectTile)
					removeCellIndices.Add(kv.Key);
			}

			removeCellIndices.ForEach(key =>
			{
				_registedTileState.Remove(key);

				if (UsePathMap == true)
					PathMap[key.x, key.y] = GetTileState(key);
			});
		}

		public TTileState GetTileState(Vector2Int cellPosition, CObjectTileComponent<TTileState> focusTile = null)
		{
			var registedTile = GetRegistedObjectTile(cellPosition);
			if(registedTile != null)
			{
				if(focusTile != null && registedTile == focusTile)
					return GetBackgroundTileState(cellPosition);

				// 예외 타일 속성 (범위는 지정되어 있지만 지나갈수 있는 규칙?)
				if(registedTile.GetExcludeTileIndices(TileConfigure).Contains(cellPosition))
				{
					return registedTile.ExcludeTileType;
				}

				return registedTile.TileType;
			}

			return GetBackgroundTileState(cellPosition);
		}

		public TTileState GetBackgroundTileState(Vector2Int cellPosition)
		{
			if (_backgroundTileState.ContainsKey(cellPosition) == false)
				return default(TTileState);

			return _backgroundTileState[cellPosition];
		}

		public CObjectTileComponent<TTileState> GetRegistedObjectTile(Vector2Int cellPosition)
		{
			if (_registedTileState.ContainsKey(cellPosition) == false)
				return null;

			return _registedTileState[cellPosition];
		}
	
		public IEnumerable<(Vector2Int Position, TTileState Type)> GetBackgroundTiles()
		{
			foreach(var kv in _backgroundTileState)
			{
				yield return (kv.Key, kv.Value);
			}
		}
	
		public IEnumerable<(Vector2Int Position, TTileState state)> GetNeighborCellStates(Vector2Int cellPosition, bool withObstacle = false)
		{
			foreach(var releativePos in ReleativeNeighborPos)
			{
				var checkPos = cellPosition + releativePos;

				yield return (checkPos, GetTileState(checkPos));
			}

			if(withObstacle == true)
			{
				foreach (var releativePos in ReleativeNeighborObstaclePos)
				{
					var checkPos = cellPosition + releativePos;

					yield return (checkPos, GetTileState(checkPos));
				}
			}
		}
	}
}
