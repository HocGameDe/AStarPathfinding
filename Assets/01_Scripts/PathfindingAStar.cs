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
    public float timeStepFindSlow;
    float timeFind;
    private void Awake()
    {
        Instance = this;
    }
    void InitPathfinding()
    {
        ClearAllList();
        currentNode = player;
        frontierNodes.AddRange(currentNode.neighbors.Where(node => !node.isObstacle));
        foreach (var node in frontierNodes)
        {
            node.gCost = Vector2.Distance(PathfindingAStar.Instance.player.transform.position, node.transform.position);
            node.previousNode = currentNode;
            node.UpdateDisplayFrontier();
        }
        exploredNodes.Add(currentNode);
        PlayerController.Instance.SetStateRunAnimator(0);
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
            Debug.LogError("Count frontier by zero");
            return false;
        }
        if (frontierNodes.Contains(target))
        {
            Debug.LogWarning("One Block");
            return true;
        }
        if (target == player)
        {
            Debug.LogWarning("Zero Block");
            return true;
        }
        while (currentNode != target)
        {
            currentNode = null;
            currentNode = BestNodeCostFrontier();
            if (currentNode == null)
            {
                Debug.LogError("Target Node not found");
                return false;
            }
            if (IsNodeTarget(currentNode))
            {
                Debug.Log("Done");
                return true;
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
        }
        timeFind = Time.time - timeFind;
        return true;
    }
    public void RestartFind()
    {
        StartFind();
    }
    void ClearAllList()
    {
        resultPath.Clear();
        foreach (var node in frontierNodes) if (!node.isObstacle) node.UpdateDisplayOrigin();
        frontierNodes.Clear();
        foreach (var node in exploredNodes) if (!node.isObstacle) node.UpdateDisplayOrigin();
        exploredNodes.Clear();
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
    Node BestNodeCostFrontier(bool bestSpeed =false)
    {
        if(frontierNodes.Count <= 0) return null;
        if (bestSpeed) return frontierNodes.OrderBy(node => node.hCost).First();
        else
        {
            frontierNodes = frontierNodes.OrderBy(node => node.FCost).ToList();
            return frontierNodes.Where(node => node.FCost == frontierNodes.First().FCost)
                .OrderBy(node => node.hCost).First();
        }
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
                if (AddFrontier(neighbor))
                {
                    neighbor.previousNode = node;
                    neighbor.gCost = node.gCost + Vector2.Distance(node.transform.position, neighbor.transform.position);
                    neighbor.SetTextNode();
                }
            }
        }
    }
    void CheckChangeNodePrevious(Node current, Node neighbor)
    {
        var FCostNeighborWithCurrent = current.gCost+Vector2.Distance(current.transform.position, neighbor.transform.position) + neighbor.hCost;
        bool checkFCost = FCostNeighborWithCurrent < neighbor.FCost;
        bool checkHCost = (FCostNeighborWithCurrent == neighbor.FCost) && (current.hCost < neighbor.previousNode.hCost);
        if (checkFCost||checkHCost)
        {
            neighbor.previousNode = current;
            neighbor.gCost = current.gCost + Vector2.Distance(current.transform.position, neighbor.transform.position);
            neighbor.SetTextNode();
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
        if (!node.isObstacle && !exploredNodes.Contains(node)&&node!=player)
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
        MoveCharacter();
    }
    public void StartFindSlow(bool bestSpeed)
    {
        InitPathfinding();
        StartCoroutine(FindSlow(bestSpeed));
    }
    IEnumerator FindSlow(bool bestSpeed)
    {
        if (frontierNodes.Count <= 0)
        {
            Debug.LogError("Cout frontier by zero");
            yield break;
        }
        if (frontierNodes.Contains(target))
        {
            Debug.LogWarning("One Block");
            yield break;
        }
        if (target == player)
        {
            Debug.LogWarning("Zero Block");
            yield break;
        }
        while (currentNode != target)
        {
            currentNode = null;
            currentNode = BestNodeCostFrontier(bestSpeed);
            if (currentNode == null)
            {
                Debug.LogError("Target Node not found");
                yield break;
            }
            currentNode.UpdateDisplayCurrent();
            yield return new WaitForSeconds(timeStepFindSlow);
            if (IsNodeTarget(currentNode))
            {
                Debug.Log("Done");
                yield return StartCoroutine(nameof(ShowPathResult));
                yield break;
            }
            if (AddExplored(currentNode))
            {
                AddNeightborsFrontier(currentNode);
                yield return new WaitForSeconds(timeStepFindSlow);
            }
            else
            {
                Debug.LogError("Bug Frontier Node convert to Explored Node");
                yield break;
            }          
        }       
    }
    void MoveCharacter()
    {
        var listPosPath = resultPath.Select((node) => node.transform.position).ToList();
        PlayerController.Instance.StartMoveWithPath(listPosPath);
    }
}
