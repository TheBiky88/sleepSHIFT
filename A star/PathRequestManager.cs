using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    private static PathRequestManager instance;

    public static PathRequestManager Instance { get { return instance; } }
    
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    PathFinding pathfinding;

    bool isProcessingPath;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
        pathfinding = GetComponent<PathFinding>();
    }
    public void RequestPath(NodeManager nodeStart, NodeManager nodeEnd, Action<NodeManager[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(nodeStart, nodeEnd, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.nodeStart, currentPathRequest.nodeEnd);
        }
    }

    public void FinishedProcessingPath(NodeManager[] path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public NodeManager nodeStart;
        public NodeManager nodeEnd;
        public Action<NodeManager[], bool> callback;

        public PathRequest(NodeManager _start, NodeManager _end, Action<NodeManager[], bool> _callback)
        {
            nodeStart = _start;
            nodeEnd = _end;
            callback = _callback;
        }
    }
}
