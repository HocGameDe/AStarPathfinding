using System.Collections.Generic;
using System.Linq;
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
    public Sprite spritePlayer;
    public Sprite spriteTarget;
    public Sprite spriteOrigin;
    public Sprite spriteFrontier;
    public Sprite spriteExplored;
    public Sprite spriteObstacle;
    public Sprite spritePath;
    public Sprite spriteCurrent;

    [Header("Sprites Node")]
    public List<Sprite> sprites = new List<Sprite>();
    Sprite GetRandomSprite()
    {
        float increasePercent = 0.7f;
        float randomValue = Random.Range(0, 100000f);
        float percentSprite = 100000 / sprites.Count + 100000 / sprites.Count* increasePercent;
        float value;
        float percentNext;
        if (randomValue <= percentSprite) return sprites.First();
        for (int i = 1; i < sprites.Count - 1; i++)
        {
            value = (100000 - percentSprite) / (sprites.Count - i);
            percentNext = percentSprite + value+ value* increasePercent;
            
            if (randomValue >= percentSprite && randomValue <= percentNext) return sprites[i];
            percentSprite += value + value * increasePercent;
        }
        return sprites.Last();
    }
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
        transform.position = gridPos;
    }
    [ContextMenu("CreateGrid")]
    void CreateGrid()
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                var newNode = Instantiate(nodePrefab, transform);
                var spriteRenderer = newNode.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = GetRandomSprite();
                newNode.GetComponent<Node>().isObstacle = spriteRenderer.sprite == sprites[0] ? false : true;
                nodes.Add(newNode);
                if (i % 2 == 0) newNode.transform.localPosition = new Vector2(j * spaceNode * 1.2f, i * spaceNode * 0.35f);
                else newNode.transform.localPosition = new Vector2(j * spaceNode * 1.2f + 1.5f, i * spaceNode * 0.35f);
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
    public void AddHCostAllNode()
    {
        foreach (var node in nodes)
        {
            node.hCost = Vector2.Distance(PathfindingAStar.Instance.target.transform.position, node.transform.position);
        }
    }
}
