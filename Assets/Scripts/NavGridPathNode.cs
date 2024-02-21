using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavGridPathNode
{
    /// <summary>
    /// World position of the node
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Row and Column position within grid
    /// </summary>
    public int Row;
    public int Col;

    /// <summary>
    /// If the node contains an obstacle
    /// </summary>
    public bool HasObstacle;

    /// <summary>
    /// Distance from origin
    /// </summary>
    public int gCost;

    /// <summary>
    /// Distance from target (heuristic)
    /// </summary>
    public int hCost;

    /// <summary>
    /// Parent node to this node when forming a path
    /// </summary>
    public NavGridPathNode parent;

    public NavGridPathNode(Vector3 position, int row, int col, bool hasObstacle)
    {
        this.Position = position;
        this.Row = row;
        this.Col = col;
        this.HasObstacle = hasObstacle;

        this.gCost = 0;
        this.hCost = 0;

        this.parent = null;
    }

    /// <summary>
    /// Heuristic estimate
    /// </summary>
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
}

/// <summary>
/// The turn line for a node, when passed the player should start turning towards the next node
/// </summary>
public class NavGridPathNodeTurnLine
{
    private const float _verticalLineGradient = float.MaxValue;


    private float _gradient;
    private float _yIntercept;
    private Vector2 _pointOnLine1;
    private Vector2 _pointOnLine2;

    private float _gradientPerpendicular;

    private bool _approachSide;

    public NavGridPathNodeTurnLine(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {
        float deltaX = pointOnLine.x - pointPerpendicularToLine.x;
        float deltaY = pointOnLine.y - pointPerpendicularToLine.y;

        if (deltaX == 0)
        {
            _gradientPerpendicular = _verticalLineGradient;
        }
        else
        {
            _gradientPerpendicular = deltaY / deltaX;
        }

        if (_gradientPerpendicular == 0)
        {
            _gradient = _verticalLineGradient;
        }
        else
        {
            _gradient = -1 / _gradientPerpendicular;
        }

        _yIntercept = pointOnLine.y - _gradient * pointOnLine.x;
        _pointOnLine1 = pointOnLine;
        _pointOnLine2 = pointOnLine + new Vector2(1, _gradient);

        _approachSide = GetSide(pointPerpendicularToLine);
    }

    bool GetSide(Vector2 point)
    {
        return (point.x - _pointOnLine1.x) * (_pointOnLine2.y - _pointOnLine1.y) > (point.y - _pointOnLine1.y) * (_pointOnLine2.x - _pointOnLine1.x);
    }

    public bool CrossedLine(Vector2 p)
    {
        return GetSide(p) != _approachSide;
    }

    public float DistanceFromPoint(Vector2 point)
    {
        float yInterceptPerpendicular = point.y - _gradientPerpendicular * point.x;
        float intersectX = (yInterceptPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
        float intersectY = _gradient * intersectX + _yIntercept;
        return Vector2.Distance(point, new Vector2(intersectX, intersectY));
    }
}