using UnityEngine;

public enum Occupation
{
    EMPTY,
    GATE,
    BASICTOWER,
    BOMBTOWER,
    STUNTOWER,
}

public class GameTile : MonoBehaviour
{
    public bool occupied = false;
    public bool buildable = true;
    public bool upgradeable = true;
    public bool Highlighted = false;
    [SerializeField] private bool canHighlight = false;
    public Occupation occupation = Occupation.EMPTY;
    private NodeManager node;

    public GameObject Tower;

    private Material oldMat;
    [SerializeField] private Material BuildableMat;
    [SerializeField] private Material NotBuildableMat;
    [SerializeField] private Material UpgradeableMat;
    private Material mat;
    private MeshRenderer mr;
    private void Start()
    {
        node = GetComponentInChildren<NodeManager>();
        mr = GetComponent<MeshRenderer>();
        oldMat = mr.material;
    }

    private void Update()
    {
        if (Highlighted && canHighlight)
        {
            if (buildable) mat = BuildableMat;
            else if (occupied && upgradeable) mat = UpgradeableMat;
            else if (occupied && !upgradeable) mat = NotBuildableMat;
        }
        
        else
        {
            mat = oldMat;
        }
        
        mr.material = mat;
    }
}
