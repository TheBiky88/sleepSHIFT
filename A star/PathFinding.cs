using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    private static PathFinding instance;

    public static PathFinding Instance { get { return instance; } }

    PathRequestManager requestManager;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
        requestManager = GetComponent<PathRequestManager>();
    }

    private List<NodeManager> path = new List<NodeManager>();

    public void StartFindPath(NodeManager startNode, NodeManager endNode)
    {
        StartCoroutine(FindPath(startNode, endNode));
    }

    private int GetDistance(NodeManager nodeA, NodeManager nodeB)
    {
        int dstX = Mathf.RoundToInt(Mathf.Abs(nodeA.transform.position.x - nodeB.transform.position.x));
        int dstY = Mathf.RoundToInt(Mathf.Abs(nodeA.transform.position.z - nodeB.transform.position.z));

        return 14 * Mathf.Min(dstX, dstY) + 10 * Mathf.Abs(dstY - dstX);
    }

    IEnumerator FindPath(NodeManager startNode, NodeManager targetNode)
    {
        Grid grid = Grid.Instance;

        NodeManager[] nodesPath = new NodeManager[0];
        bool pathSucces = false;

        Heap<NodeManager> openSet = new Heap<NodeManager>(grid.gridSize);
        HashSet<NodeManager> closedSet = new HashSet<NodeManager>();



        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);



        openSet.Add(startNode);

        if (openSet.Count == 0)
        {
            Debug.Log("No path could be made");
        }

        while (openSet.Count > 0)
        {
            NodeManager currentNode = openSet.RemoveFirst();

            closedSet.Add(currentNode);


            if (currentNode.transform.parent == targetNode.transform.parent)
            {
                pathSucces = true;
                break;
            }

            foreach (NodeManager surroundingNode in currentNode.surroundingNodes)
            {
                if (closedSet.Contains(surroundingNode))
                {
                    continue;
                }

                if (!surroundingNode.isPassable)
                {
                    closedSet.Add(surroundingNode);
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, surroundingNode);

                if (newMovementCostToNeighbour < surroundingNode.gCost || !openSet.Contains(surroundingNode))
                {
                    surroundingNode.gCost = newMovementCostToNeighbour;
                    surroundingNode.hCost = GetDistance(surroundingNode, targetNode);
                    surroundingNode.parent = currentNode;

                    if (!openSet.Contains(surroundingNode))
                    {
                        openSet.Add(surroundingNode);
                    }
                    else
                    {
                        openSet.UpdateItem(surroundingNode);
                    }
                }
            }
        }

        yield return null;

        if (pathSucces)
        {
            nodesPath = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(nodesPath, pathSucces);
    }

    NodeManager[] RetracePath(NodeManager startNode, NodeManager endNode)
    {
        path.Clear();

        NodeManager currentNode = endNode;

        while (currentNode.transform.parent != startNode.transform.parent)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        if (currentNode == startNode)
        {
            path.Add(currentNode);
        }
        path.Reverse();

        return path.ToArray();
    }
}
