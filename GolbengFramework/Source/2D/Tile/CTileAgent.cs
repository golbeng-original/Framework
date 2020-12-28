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
	public partial class CTileAgent
	{
		public static CTileConfigure TileConfigure { get; set; } = new CTileConfigure(new Vector2Int(1, 1));
	}

	public partial class CTileAgent
	{
		private Dictionary<Vector2Int, int> _backgroundTileState = new Dictionary<Vector2Int, int>();

		private HashSet<CObjectTileComponent> _registedObjectTiles = new HashSet<CObjectTileComponent>();
		private Dictionary<Vector2Int, CObjectTileComponent> _registedTileState = new Dictionary<Vector2Int, CObjectTileComponent>();

		public bool UsePathMap { get; set; } = true;
		public CTile2DPathFinder.PathMap PathMap { get; set; } = new CTile2DPathFinder.PathMap();

		public void UpdateBackgroundTileType(Tilemap tileMap, Func<TileBase, int> tileStateFunc)
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

		public void RegisterObjectTile(CObjectTileComponent objectTile)
		{
			if (_registedObjectTiles.Contains(objectTile) == true)
				return;

			_registedObjectTiles.Add(objectTile);

			foreach(var position in objectTile.GetTileIndices(TileConfigure))
			{
				var registedPosition = new Vector2Int(position.x, position.y);

				_registedTileState.Add(registedPosition, objectTile);

				if (UsePathMap == true)
					PathMap[position.x, position.y] = GetTileState(registedPosition);

			}
		}

		public void UnregisterObjectTile(CObjectTileComponent objectTile)
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

		public int GetTileState(Vector2Int cellPosition, CObjectTileComponent focusTile = null)
		{
			var registedTile = GetRegistedObjectTile(cellPosition);
			if(registedTile != null)
			{
				if(focusTile != null && registedTile == focusTile)
					return GetBackgroundTileState(cellPosition);

				return registedTile.TileType;
			}

			return GetBackgroundTileState(cellPosition);
		}

		public int GetBackgroundTileState(Vector2Int cellPosition)
		{
			if (_backgroundTileState.ContainsKey(cellPosition) == false)
				return -1;

			return _backgroundTileState[cellPosition];
		}

		public CObjectTileComponent GetRegistedObjectTile(Vector2Int cellPosition)
		{
			if (_registedTileState.ContainsKey(cellPosition) == false)
				return null;

			return _registedTileState[cellPosition];
		}
	
		public IEnumerable<(Vector2Int Position, int Type)> GetBackgroundTiles()
		{
			foreach(var kv in _backgroundTileState)
			{
				yield return (kv.Key, kv.Value);
			}
		}
	}
}
