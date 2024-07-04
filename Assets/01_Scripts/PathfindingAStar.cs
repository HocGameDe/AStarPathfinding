using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathfindingAStar : MonoBehaviour
{
    public static PathfindingAStar Instance;
    [Header("Lists in Gameplay")]
    public List<Node> resultPath = new List<Node>();
    public List<Node> frontierNodes = new List<Node>();
    public List<Node> exploredNodes = new List<Node>();
    [Header("Nodes in Gameplay")]
    public Node player;
    public Node target;
    public Node currentNode;
    [Header("Time")]
    public float timeStepShowResult;
    float timeFind;

    private void Awake()
    {
        Instance = this;
    }
    void InitPathfinding()
    {
        currentNode = player;
        frontierNodes.AddRange(currentNode.neighbors.Where(node => !node.isObstacle));
        if (frontierNodes.Contains(target)) Debug.Log("TRUE");
        foreach (var node in frontierNodes)
        {
            node.gCost = Vector2.Distance(PathfindingAStar.Instance.player.transform.position, node.transform.position);
            node.previousNode = currentNode;
            node.UpdateDisplayFrontier();
        }
        exploredNodes.Add(currentNode);
    }
    public void StartFind()
    {
        InitPathfinding();
        var result = FindPath();
        if (result)
        {            
            StartCoroutine(nameof(ShowPathResult));
            Debug.Log("Showing Path ...");
        }
        LogResultFind(result);
    }
    bool FindPath()
    {
        timeFind = Time.time;
        if (frontierNodes.Count <= 0)
        {
            Debug.LogError("Cout frontier by zero");
            return false;
        }
        while (currentNode != target)
        {
            currentNode = null;
            currentNode = BestNodeCostFrontier();
            if (currentNode == null)
            {
                Debug.LogError("Don't Find Out Target Node");
                return false;
            }
            if (AddExplored(currentNode))
            {
                AddNeightborsFrontier(currentNode);
            }
            else
            {
                Debug.LogError("Bug Frontier Node convert to Explored Node");
                return false;
            }
            if (IsNodeTarget(currentNode)) return true;
        }
        timeFind = Time.time - timeFind;
        return true;
    }
    public void RestartFind()
    { 
        resultPath.Clear();
        foreach (var node in frontierNodes) if(!node.isObstacle) node.UpdateDisplayOrigin();
        frontierNodes.Clear();
        foreach (var node in exploredNodes) if (!node.isObstacle) node.UpdateDisplayOrigin();
        exploredNodes.Clear();
        StartFind();
    }
    public void SetOnChangePlayerNode()
    {
        Node.IsChangePlayer = true;
    }
    public void SetOnChangePlayerUpdate()
    {
        Node.IsChangePlayerUpdate = true;
        player.SetPlayerUnLockDrag();
    }
    public void LogResultFind(bool result)
    {
        Debug.Log("Result = "+ result);
        Debug.Log("Time = "+ timeFind);
    }
    Node BestNodeCostFrontier()
    {
        if(frontierNodes.Count <= 0) return null;
        return frontierNodes.OrderBy(node => node.FCost).First();
    }
    void AddNeightborsFrontier(Node node)
    {
        foreach (var neighbor in node.neighbors)
        {
            if (frontierNodes.Contains(neighbor))
            {
                CheckChangeNodePrevious(node, neighbor);
            }
            else
            {
                if(AddFrontier(neighbor)) neighbor.previousNode = node;
                neighbor.gCost = node.gCost + Vector2.Distance(node.transform.position, neighbor.transform.position);
            }
        }
    }
    void CheckChangeNodePrevious(Node current, Node neighbor)
    {
        var FCost = current.gCost+Vector2.Distance(current.transform.position, neighbor.transform.position) + neighbor.hCost;

        if (FCost < neighbor.FCost)
        {
            neighbor.previousNode = current;
            neighbor.gCost = current.gCost + Vector2.Distance(current.transform.position, neighbor.transform.position);
        }
        else if (FCost == neighbor.FCost && current.hCost < neighbor.previousNode.hCost)
        {
            neighbor.previousNode = current;
            neighbor.gCost = current.gCost + Vector2.Distance(current.transform.position, neighbor.transform.position);
        }
    }
    bool IsNodeTarget(Node node)
    {
        return node == target;
    }
    bool AddExplored(Node node)
    {        
        if (!exploredNodes.Contains(node) && !node.isObstacle)
        {
            frontierNodes.Remove(node);
            exploredNodes.Add(node);
            node.UpdateDisplayExplored();
            return true;
        }
        return false;
    }
    bool AddFrontier(Node node)
    {
        if (!node.isObstacle && !exploredNodes.Contains(node))
        {
            frontierNodes.Add(node);
            node.UpdateDisplayFrontier();
            return true;
        }
        return false;
    }
    IEnumerator ShowPathResult()
    {
        currentNode = target;
        resultPath.Add(currentNode);
        while (currentNode!=player)
        {
            currentNode = currentNode.previousNode;
            currentNode.UpdateDisplayPath();
            resultPath.Add(currentNode);
            yield return new WaitForSeconds(timeStepShowResult);
        }
    }
}