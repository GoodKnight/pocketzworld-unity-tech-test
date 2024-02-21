using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class NavGrid : MonoBehaviour
{
    /// <summary>
    /// Dimensions of the grid
    /// </summary>
    [SerializeField]
    private Vector2 _gridSize;

    /// <summary>
    /// Size for each node of the grid (square)
    /// </summary>
    [SerializeField]
    public float NodeSize;

    /// <summary>
    /// Layer that contains obstacles
    /// </summary>
    [SerializeField]
    private LayerMask _obstacleLayerMask;

    private NavGridPathNode[,] _nodes;
    private int _nodeRows;
    private int _nodeCols;

    public void Awake()
    {
        _nodeRows = (int)(_gridSize.x / NodeSize);
        _nodeCols = (int)(_gridSize.y / NodeSize);

        GenerateNodes();
    }

    /// <summary>
    /// Generate all nodes
    /// </summary>
    private void GenerateNodes()
    {
        _nodes = new NavGridPathNode[_nodeRows, _nodeCols];
        Vector3 worldPosBottomLeft = transform.position - new Vector3(1, 0, 0) * _gridSize.x / 2 - new Vector3(0, 0, 1) * _gridSize.y / 2; 

        for (int row = 0; row < _nodeRows; row++)
        {
            for (int col = 0; col < _nodeCols; col++)
            {
                Vector3 worldPos = worldPosBottomLeft + new Vector3(1, 0, 0) * (row * NodeSize + NodeSize / 2) +
                    new Vector3(0, 0, 1) * (col * NodeSize + NodeSize / 2);
                bool hasObstacle = Physics.CheckBox(worldPos, new Vector3(NodeSize / 2, NodeSize / 2, NodeSize / 2),
                    Quaternion.identity, _obstacleLayerMask);
                _nodes[row, col] = new NavGridPathNode(worldPos, row, col, hasObstacle);
            }
        }
    }

    public bool AreNodesGenerated()
    {
        return _nodes == null ? false : true;
    }

    /// <summary>
    /// Get node from world position
    /// </summary>
    public NavGridPathNode GetNodeFromWorldPos(Vector3 worldPos)
    {
        float percentRow = (worldPos.x + _gridSize.x / 2) / _gridSize.x;
        float percentCol = (worldPos.z + _gridSize.y / 2) / _gridSize.y;

        // To prevent going out of bounds
        percentRow = Mathf.Clamp01(percentRow);
        percentCol = Mathf.Clamp01(percentCol);

        // Round to the nearest node
        int row = Mathf.RoundToInt((_nodeRows - 1) * percentRow);
        int col = Mathf.RoundToInt((_nodeCols - 1) * percentCol);

        return _nodes[row, col];
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_gridSize.x, 1, _gridSize.y));

        if (_nodes != null)
        {
            foreach (NavGridPathNode node in _nodes)
            {
                if (node.HasObstacle)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(node.Position, new Vector3(1, 0, 1) * (NodeSize - 0.1f));
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(node.Position, new Vector3(1, 0, 1) * (NodeSize - 0.1f));
                }
            }
        }
    }

    /// <summary>
    /// Given the current and desired location, return a path to the destination (using A*)
    /// </summary>
    public List<NavGridPathNode> GetPath(Vector3 origin, Vector3 destination)
    {
        NavGridPathNode originNode = GetNodeFromWorldPos(origin);
        NavGridPathNode destinationNode = GetNodeFromWorldPos(destination);

        List<NavGridPathNode> availableNodes = new List<NavGridPathNode>();
        availableNodes.Add(originNode);
        HashSet<NavGridPathNode> visitedNodesSet = new HashSet<NavGridPathNode>();

        while (availableNodes.Count > 0)
        {
            // Find node with the lowest f_cost           
            NavGridPathNode currNode = availableNodes[0];
            for (int i = 1; i < availableNodes.Count; i++)
            {
                if (currNode.fCost > availableNodes[i].fCost ||
                    currNode.fCost == availableNodes[i].fCost && currNode.hCost > availableNodes[i].hCost)
                {
                    currNode = availableNodes[i];
                }
            }

            availableNodes.Remove(currNode);
            visitedNodesSet.Add(currNode);

            if (currNode == destinationNode)
            {
                // Path complete!
                List<NavGridPathNode> path = new List<NavGridPathNode>();

                currNode = destinationNode;
                while (currNode != originNode)
                {
                    path.Add(currNode);
                    currNode = currNode.parent;
                }

                path.Reverse();
                return path;
            }

            List<NavGridPathNode> neighbors = GetNeighborsForNode(currNode);
            foreach (NavGridPathNode node in neighbors)
            {
                if (node.HasObstacle || visitedNodesSet.Contains(node))
                {
                    continue;
                }

                int costToNode = currNode.gCost + GetDistance(currNode, node);
                if (costToNode < node.gCost || !availableNodes.Contains(node))
                {
                    // Update node cost
                    node.gCost = costToNode;
                    node.hCost = GetDistance(node, destinationNode);
                    node.parent = currNode;

                    if (!availableNodes.Contains(node))
                    {
                        availableNodes.Add(node);
                    }
                }
            }
        }

        return new List<NavGridPathNode>();
    }

    /// <summary>
    /// Get list of neighbors for node
    /// </summary>
    public List<NavGridPathNode> GetNeighborsForNode(NavGridPathNode node)
    {
        List<NavGridPathNode> neighbors = new List<NavGridPathNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    // Self
                    continue;
                }

                int row = node.Row + x;
                int col = node.Col + y;

                if (row < 0 || col < 0 || row >= _nodeRows || col >= _nodeCols)
                {
                    // Out of bounds
                    continue;
                }

                neighbors.Add(_nodes[row, col]);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Calculate distance between nodes
    /// </summary>
    public int GetDistance(NavGridPathNode nodeA, NavGridPathNode nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.Row - nodeB.Row);
        int distanceY = Mathf.Abs(nodeA.Col - nodeB.Col);

        if (distanceX > distanceY)
        {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }

        return 14 * distanceX + 10 * (distanceY - distanceX);
    }

    /// <summary>
    /// Get list of turn lines from path
    /// </summary>
    public List<NavGridPathNodeTurnLine> GetTurnLinesFromPath(List<NavGridPathNode> path, Vector3 originPos, float turnDistance)
    {
        List<NavGridPathNodeTurnLine> turnLines = new List<NavGridPathNodeTurnLine>();

        Vector2 prevPoint = new Vector2(originPos.x, originPos.z);
        for (int i = 0; i < path.Count; i++)
        {
            Vector2 currPoint = new Vector2(path[i].Position.x, path[i].Position.z);
            Vector2 directionToCurrPoint = (currPoint - prevPoint).normalized;
            Vector2 turnLinePoint = (i == path.Count - 1) ? currPoint : currPoint - directionToCurrPoint * turnDistance;
            turnLines.Add(new NavGridPathNodeTurnLine(turnLinePoint, prevPoint - directionToCurrPoint * turnDistance));
            prevPoint = turnLinePoint;
        }

        return turnLines;
    }
}
