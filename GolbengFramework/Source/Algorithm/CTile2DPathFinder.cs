using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GolbengFramework.Algorithm
{
	public partial class CTile2DPathFinder
	{
		public enum HuristicType
		{
			Manhttan,
			Euclidean,
			Octile,
			Chebyshev
		}

		public enum DiagonalMoveType
		{
			None,               // 대각선 계산 안함
			OneObstacle,        // 한 방향이라고 Open이면.
			NoObstacle,         // 두 방향 모두 장애물이 없어야 함.
		}

		private enum NeighborDirection : int
		{
			Up = 0,
			Right,
			Down,
			Left
		}

		public class PathMap
		{
			private Dictionary<(int x, int y), int> _pathMapInfo = new Dictionary<(int x, int y), int>();

			public int this[int x, int y]
			{
				get
				{
					var key = (x, y);

					if (_pathMapInfo.ContainsKey(key) == false)
						return -1;

					return _pathMapInfo[key];
				}

				set
				{
					var key = (x, y);
					_pathMapInfo[key] = value;
				}
			}

			public void Remove(int x, int y)
			{
				_pathMapInfo.Remove((x, y));
			}

			public void Clear()
			{
				_pathMapInfo.Clear();
			}
		}

		public class AStarNode
		{
			public int X { get; set; } = 0;
			public int Y { get; set; } = 0;

			public double G { get; set; } = 0;

			public double H { get; set; } = 0;

			public double F { get => (G + H); }

			public bool IsOpend { get; set; } = false;
			public bool IsClosed { get; set; } = false;

			public AStarNode Parent { get; set; } = null;
		}

	}


	public partial class CTile2DPathFinder
	{
		private List<AStarNode> _openList = new List<AStarNode>();
		private Dictionary<(int X, int Y), AStarNode> _visitedNodes = new Dictionary<(int X, int Y), AStarNode>();

		public PathMap ActivePathMap { get; set; }
		public (int X, int Y) StartPosition { get; set; } = (0, 0);
		public (int X, int Y) EndPosition { get; set; } = (0, 0);

		private AStarNode StartNode { get => GetNode(StartPosition.X, StartPosition.Y); }
		private AStarNode EndNode { get => GetNode(EndPosition.X, EndPosition.Y); }
		//
		public HuristicType Huristic { get; set; } = HuristicType.Manhttan;

		public DiagonalMoveType DiagonalType { get; set; } = DiagonalMoveType.None;

		public double HuristicWeight { get; set; } = 1;

		public HashSet<int> BlockValues { get; private set; } = new HashSet<int>();

		public List<(int X, int Y)> FindPathList { get; private set; } = new List<(int X, int Y)>();

		public CTile2DPathFinder(PathMap pathMap)
		{
			ActivePathMap = pathMap;
		}

		public bool Prepare()
		{
			if (StartNode == null)
				return false;

			if (EndNode == null)
				return false;

			_visitedNodes.Clear();
			_openList.Clear();

			StartNode.IsOpend = true;
			_openList.Add(StartNode);

			FindPathList.Clear();

			return true;
		}

		public bool FindPath()
		{
			while (FindPathStep() == false) { }

			return FindPathList.Count == 0 ? false : true;
		}

		public async Task<bool> FindPathAsync()
		{
			bool result = false;

			await Task.Run(() =>
			{
				result = FindPath();
			});

			return result;
		}

		public bool FindPathStep()
		{
			if (_openList.Count == 0)
				return true;

			var currNode = _openList[0];
			_openList.RemoveAt(0);

			currNode.IsClosed = true;
			if (currNode == EndNode)
			{
				FindPathList = MakePathFromNode(currNode);
				return true;
			}

			foreach (var neighbor in FindNeighbors(currNode))
			{
				if (neighbor.IsClosed == true)
					continue;

				double weightG = 1;

				// 대각선 방향이므로 √2
				if (currNode.X - neighbor.X != 0 || currNode.Y - neighbor.Y != 0)
					weightG = Math.Sqrt(2);

				double newG = currNode.G + weightG;

				if (neighbor.IsOpend == false || newG < neighbor.G)
				{
					// 일반 가중치(시작점에서 거리)
					neighbor.G = newG;

					// 휴리스틱 가중치
					neighbor.H = HuristicWeight * GetHuristicValue(Math.Abs(neighbor.X - EndNode.X), Math.Abs(neighbor.Y - EndNode.Y));

					neighbor.Parent = currNode;

					if (neighbor.IsOpend == false)
					{
						neighbor.IsOpend = true;
						_openList.Add(neighbor);
					}

					_openList.Sort((lhs, rhs) =>
					{
						return lhs.F < rhs.F ? -1 : 1;
					});
				}
			}

			return false;
		}

		public IEnumerable<AStarNode> GetVisitedNodes()
		{
			return _visitedNodes.Values;
		}

		private List<(int X, int Y)> MakePathFromNode(AStarNode node)
		{
			List<(int X, int Y)> positionList = new List<(int X, int Y)>();

			var currNode = node;
			while (currNode != null)
			{
				positionList.Add((currNode.X, currNode.Y));
				currNode = currNode.Parent;
			}

			positionList.Reverse();
			return positionList;
		}

		private AStarNode GetNode(int x, int y)
		{
			(int X, int Y) key = (x, y);
			if (_visitedNodes.ContainsKey(key) == false)
				_visitedNodes.Add(key, new AStarNode() { X = x, Y = y });

			return _visitedNodes[key];
		}

		private IEnumerable<AStarNode> FindNeighbors(AStarNode node)
		{
			List<(int X, int Y)> neighborPositions = new List<(int X, int Y)>()
			{
				(node.X, node.Y - 1), // ↑
				(node.X + 1, node.Y), // →
				(node.X, node.Y + 1), // ↓
				(node.X - 1, node.Y), // ←
			};

			bool[] findNeigborDirection = new bool[4] { false, false, false, false };

			foreach (var position in neighborPositions)
			{
				if (IsBlock(position.X, position.Y) == true)
					continue;

				var neighborNode = GetNode(position.X, position.Y);
				if (neighborNode == null)
					continue;

				findNeigborDirection[neighborPositions.IndexOf(position)] = true;

				yield return neighborNode;
			}

			neighborPositions.Clear();

			if (DiagonalType == DiagonalMoveType.NoObstacle)
			{
				if (findNeigborDirection[(int)NeighborDirection.Up] &&
					findNeigborDirection[(int)NeighborDirection.Right])
					neighborPositions.Add((node.X + 1, node.Y - 1));

				if (findNeigborDirection[(int)NeighborDirection.Right] &&
					findNeigborDirection[(int)NeighborDirection.Down])
					neighborPositions.Add((node.X + 1, node.Y + 1));

				if (findNeigborDirection[(int)NeighborDirection.Down] &&
					findNeigborDirection[(int)NeighborDirection.Left])
					neighborPositions.Add((node.X - 1, node.Y + 1));

				if (findNeigborDirection[(int)NeighborDirection.Left] &&
					findNeigborDirection[(int)NeighborDirection.Up])
					neighborPositions.Add((node.X - 1, node.Y - 1));
			}
			else if (DiagonalType == DiagonalMoveType.OneObstacle)
			{
				if (findNeigborDirection[(int)NeighborDirection.Up] ||
					findNeigborDirection[(int)NeighborDirection.Right])
					neighborPositions.Add((node.X + 1, node.Y - 1));

				if (findNeigborDirection[(int)NeighborDirection.Right] ||
					findNeigborDirection[(int)NeighborDirection.Down])
					neighborPositions.Add((node.X + 1, node.Y + 1));

				if (findNeigborDirection[(int)NeighborDirection.Down] ||
					findNeigborDirection[(int)NeighborDirection.Left])
					neighborPositions.Add((node.X - 1, node.Y + 1));

				if (findNeigborDirection[(int)NeighborDirection.Left] ||
					findNeigborDirection[(int)NeighborDirection.Up])
					neighborPositions.Add((node.X - 1, node.Y - 1));
			}

			foreach (var position in neighborPositions)
			{
				if (IsBlock(position.X, position.Y) == true)
					continue;

				var neighborNode = GetNode(position.X, position.Y);
				if (neighborNode == null)
					continue;

				findNeigborDirection[neighborPositions.IndexOf(position)] = true;

				yield return neighborNode;
			}
		}

		private double GetHuristicValue(double dx, double dy)
		{
			switch (Huristic)
			{
				case HuristicType.Manhttan:
					return dx + dy;
				case HuristicType.Euclidean:
					return Math.Sqrt(dx * dx + dy * dy);
				case HuristicType.Octile:
					double f = Math.Sqrt(2) - 1;
					return (dx < dy) ? f * dx + dy : f * dy + dx;
				case HuristicType.Chebyshev:
					return Math.Max(dx, dy);
			}

			return 0;
		}

		private bool IsBlock(int x, int y)
		{
			int nodeType = ActivePathMap[x, y];
			if (nodeType == -1)
				return true;

			return BlockValues.Contains(nodeType) == true ? true : false;
		}
	}
}
