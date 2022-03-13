using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;

    public static CameraManager Instance { get { return instance; } }

    public GameTile gt; // reference for builder

    // garbage that needs to be repurposed
    public Vector3 cameraPosition;
    private Camera camera;
    private GameObject rangeVisual;
    public GameObject Tower;
    [SerializeField] private TextMeshProUGUI toolTipTMP;
    //--------

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void SetPositions(float x, float z)
    {
        transform.position = new Vector3(x, transform.position.y, z);
        cameraPosition = transform.position;
    } 

    void Start()
    {
        cameraPosition = transform.position;
        camera = GetComponent<Camera>();
    }

    private void RayCaster()
    {
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawRay(camera.transform.position, ray.direction * 150f);
        int layerMaskTile = LayerMask.GetMask("Tile");

        if (Physics.Raycast(ray, out hit, 150f, layerMaskTile))
        {
            Transform objectHit = hit.transform;
            gt = objectHit.GetComponent<GameTile>();
            gt.Highlighted = true;

            if (gt.occupied)
            {
                GameObject tower = gt.Tower.gameObject;
                if (tower != null)
                {
                    Turrets.BaseTurret turret = tower.GetComponent<Turrets.BaseTurret>();
                    rangeVisual = turret.rangeVisual;
                    rangeVisual.SetActive(true);
                    float range = turret.rangeCollider.radius * 2;
                    rangeVisual.transform.localScale = new Vector3(range, 0.01f, range);
                }
            }
        }
    }

    private void Update()
    {
        if (!GameManager.Instance.paused && !GameManager.Instance.gameOver)
        {
            if (rangeVisual != null) rangeVisual.SetActive(false);
            if (gt != null) gt.Highlighted = false;
            gt = null;
            RayCaster();
        }       
    }
}