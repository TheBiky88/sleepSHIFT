using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Grid : MonoBehaviour
{
    private static Grid instance;
    
    public static Grid Instance { get { return instance; } }

    [SerializeField] private int gridWidth = 11;
    [SerializeField] private int gridHeight = 11;

    [SerializeField] private Transform tilePrefab;
    [SerializeField] private Transform endTilePrefab;
    [SerializeField] private Transform startTilePrefab;

    public Transform _startTile;
    public Transform _endTile;

    public bool pathPossible;

    public bool shouldChangeScale = false;

    [SerializeField] private List<Transform> blocks = new List<Transform>();

    Vector3 startPos;

    private float blockWidth = 1;
    NodeManager startNode, targetNode;

    public int gridSize
    {
        get
        {
            return gridWidth * gridHeight;
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void Start()
    {
        StartCoroutine(Creation());
    }

    IEnumerator Creation()
    {
        CalcStartPos();
        yield return new WaitForSeconds(0.01f);
        
        CreateGrid();
        yield return new WaitForSeconds(0.01f);


        PlaceStartEndTiles();
        yield return new WaitForSeconds(0.1f);

        if (shouldChangeScale) SizeUpMap();

    }

    private void SizeUpMap()
    {
        Vector3 scale = transform.localScale;
        scale *= 7;
        transform.localScale = scale;

        transform.position = new Vector3(34, -1, 2);
    }

    private void PlaceStartEndTiles()
    {
        Transform[] removeTiles = new Transform[2];


        List<Transform> bottomRowTiles = new List<Transform>();
        foreach (Transform block in blocks)
        {
            if (block.transform.position.z == 13)
            {
                bottomRowTiles.Add(block);
            }
        }

        // place start tile on bottom row (y = 0)
        Transform tile = bottomRowTiles[UnityEngine.Random.Range(0, bottomRowTiles.Count)];

        //place a startTile and remove the old one
        Transform startTile = Instantiate(startTilePrefab, tile.position, tile.rotation);
        removeTiles[0] = tile;


        tile = blocks[76];

        Transform endTile = Instantiate(endTilePrefab, tile.position, tile.rotation);
        removeTiles[1] = tile;

        removeTiles[0].GetChild(0).transform.parent = startTile.transform;
        removeTiles[1].GetChild(0).transform.parent = endTile.transform;

        Destroy(removeTiles[0].gameObject);
        Destroy(removeTiles[1].gameObject);
        blocks.Remove(removeTiles[0]);
        blocks.Remove(removeTiles[1]);

        _endTile = endTile;
        _startTile = startTile;
        _endTile.parent = transform;
        _startTile.parent = transform;
        GameManager.Instance.startTile = _startTile.GetComponentInChildren<NodeManager>().transform;
        GameManager.Instance.endTile = _endTile.GetComponentInChildren<NodeManager>().transform;

        blocks.Add(_endTile);
        blocks.Add(_startTile);


        startNode = _startTile.GetComponentInChildren<NodeManager>();
        targetNode = _endTile.GetComponentInChildren<NodeManager>();
    }

    private void CalcStartPos()
    {
        float x = -blockWidth * (gridWidth);
        float z = blockWidth * (gridHeight);

        startPos = new Vector3(x, 0, z);
    }

    Vector3 CalcWorldPos(Vector3 gridPos)
    {
        float x = startPos.x + gridPos.x * blockWidth;
        float y = gridPos.z;
        float z = startPos.z - gridPos.y * blockWidth;

        return new Vector3(x, y, z);
    }

    private void CreateGrid()
    {
        for (int x = 0; x < gridHeight; x++)
        {
            for (int y = 0; y < gridWidth; y++)
            {
                Transform block;

                block = Instantiate(tilePrefab);
                blocks.Add(block);

                Vector3 gridPos = new Vector3(x, y, 0);
                block.position = CalcWorldPos(gridPos);
                block.parent = transform;
                block.name = "Tile - x|y: " + x + "|" + y;

            }
        }
    }


}
