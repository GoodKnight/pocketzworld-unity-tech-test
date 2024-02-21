using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<NavGridPathNode> _currentPath = null;
    private List<NavGridPathNodeTurnLine> _currentTurnLines = null;
    private int _currentPathIndex = 0;
    private bool _traversing = false;

    /// <summary>
    /// Navigation grid
    /// </summary>
    [SerializeField]
    private NavGrid _grid;

    /// <summary>
    /// Speed of the player
    /// </summary>
    [SerializeField]
    private float _speed = 10.0f;

    /// <summary>
    /// Turn speed of the player (upon reaching a node turn line)
    /// </summary>
    [SerializeField]
    private float _turnSpeed = 20.0f;

    /// <summary>
    /// How far the turn line should be from node
    /// </summary>
    [SerializeField]
    private float _turnDistance = 1.0f;

    void Update()
    {
        // Check Input
        if (Input.GetMouseButtonUp(0))
        {
            if (!_traversing)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hitInfo))
                {
                    _currentPath = _grid.GetPath(transform.position, hitInfo.point);
                    _currentTurnLines = _grid.GetTurnLinesFromPath(_currentPath, transform.position, _turnDistance);
                    _currentPathIndex = 0;

                    StopCoroutine(Traverse());
                    StartCoroutine(Traverse());
                }
            }
        }
    }

    IEnumerator Traverse()
    {
        _traversing = true;

        while (_currentPathIndex < _currentPath.Count)
        {
            var currentNode = _currentPath[_currentPathIndex];

            var vectorToDestination = currentNode.Position - transform.position;
            vectorToDestination.y = 0f; // Ignore Y

            Quaternion targetRotation = Quaternion.LookRotation(vectorToDestination);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * _speed, Space.Self);

            Vector2 player2DPos = new Vector2(transform.position.x, transform.position.z);
            while (_currentPathIndex < _currentPath.Count && _currentTurnLines[_currentPathIndex].CrossedLine(player2DPos))
            {
                _currentPathIndex++;
            }

            yield return null;
        }

        _traversing = false;
    }

    public void OnDrawGizmos()
    {
        if (_grid != null && _grid.AreNodesGenerated())
        {
            NavGridPathNode playerNode = _grid.GetNodeFromWorldPos(transform.position);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(playerNode.Position, new Vector3(1, 0, 1) * (_grid.NodeSize - 0.2f));
        }
    }

}
