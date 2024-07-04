using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid information")]
    List<Node> nodes = new List<Node>();
    public Node nodePrefab;
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] float spaceNode;
    Vector2 gridPos;

    [Header("Color Node modes")]
    public Color colorPlayer;
    public Color colorTarget;
    public Color colorOrigin;
    public Color colorFrontier;
    public Color colorExplored;
    public Color colorObstacle;
    public Color colorPath;

    private void Awake()
    {
        nodePrefab = Resources.Load<Node>("Prefabs/Node");
        Instance = this;
    }
    private void Start()
    {
        SetPosGrid();
        CreateGrid();
    }
    void SetPosGrid()
    {
        gridPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
        transform.position = gridPos + new Vector2(0.3f,0.35f);
    }
    [ContextMenu("CreateGrid")]
    void CreateGrid()
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                var newNode = Instantiate(nodePrefab, transform);
                newNode.GetComponent<SpriteRenderer>().color = colorOrigin;
                nodes.Add(newNode);
                if (i % 2 == 0) newNode.transform.localPosition = new Vector2(j * spaceNode, i * spaceNode*0.86f);
                else newNode.transform.localPosition = new Vector2(j * spaceNode + spaceNode / 2, i * spaceNode*0.86f);
            }
    }
    [ContextMenu("DeleteGrid")]
    void DeleteGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void AddCostAllNode()
    {
        foreach(var node in nodes)
        {
            node.hCost = Vector2.Distance(PathfindingAStar.Instance.target.transform.position, node.transform.position);
        }
    }
}
