using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;


public class NodeManager : MonoBehaviour, IHeapItem<NodeManager>
{
    public List<NodeManager> surroundingNodes = new List<NodeManager>();

    public bool isPassable = true;

    public NodeManager parent;

    public int gCost;
    public int hCost;
    int heapIndex;

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }

        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(NodeManager nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);

        }
        return -compare;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    void Start()
    {
        float radius = 1.1f;
        List<NodeManager> nodes = new List<NodeManager>();
        nodes = FindObjectsOfType<NodeManager>().ToList();
        foreach (NodeManager node in nodes)
        {
            if (node != null)
            {
                //filter adjacent nodes
                if (Vector3.Distance(transform.position, node.transform.position) < radius)
                {
                    if (node.transform != transform)
                    {
                        surroundingNodes.Add(node);
                    }
                }
            }
        }
        nodes.Clear();
    }   
}
