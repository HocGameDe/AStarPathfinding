﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour,IPointerClickHandler,IBeginDragHandler, IPointerEnterHandler, IPointerUpHandler,IDragHandler
{
    [Header("Node Information")]
    public float gCost;
    public float hCost;
    public float FCost => gCost + hCost;

    public Node previousNode;

    public List<Node> neighbors = new List<Node>();
    public bool isObstacle;

    [Header("Text Node")]
    public TextMeshProUGUI gText;
    public TextMeshProUGUI hText;
    public TextMeshProUGUI fText;
    public Canvas canvasText;

    static int pickPrepare;
    static bool isDaging;
    public static bool IsChangePlayer;
    public static bool IsChangePlayerUpdate;
    float radius;
    void Start()
    {
        radius = GridManager.Instance.transform.localScale.x;
        canvasText.gameObject.SetActive(false);
        neighbors = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero)
            .Select(hit => hit.transform.GetComponent<Node>()).Where(hit => hit.transform.GetComponent<Node>()!=this)
            .ToList();        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(ChangePlayer()) return;
        pickPrepare += 1;
        if(pickPrepare ==1)
        {
            UpdateDisplayPlayer();
        }
        else if(pickPrepare ==2)
        {
            PathfindingAStar.Instance.target = this;
            isObstacle = false;
            GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spriteTarget;
            GridManager.Instance.AddHCostAllNode();
        }
        else
        {
            OnPointerSetStatus();
        }
    }
    bool ChangePlayer()
    {
        if (IsChangePlayer)
        {
            UpdateDisplayPlayer();
            PathfindingAStar.Instance.RestartFind();
            IsChangePlayer = false;
            return true;
        }
        return false;
    }
    void OnPointerSetStatus()
    {
        if (this == PathfindingAStar.Instance.target) return;
        if (this == PathfindingAStar.Instance.player) return;
        if (!isObstacle) UpdateDisplayObstacle();
        else UpdateDisplayOrigin();
    }
    public void UpdateDisplayPath()
    {
        if (this == PathfindingAStar.Instance.target) return;
        if (this == PathfindingAStar.Instance.player) return;
        GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spritePath;
    }
    public void UpdateDisplayFrontier()
    {
        if (this == PathfindingAStar.Instance.target) return;
        GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spriteFrontier;
        SetTextNode();
    }
    public void UpdateDisplayExplored()
    {
        if (this == PathfindingAStar.Instance.target) return;
        GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spriteExplored;
    }
    void UpdateDisplayObstacle()
    {
        isObstacle = true;
        GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spriteObstacle;
        canvasText.gameObject.SetActive(false);
    }
    public void UpdateDisplayCurrent()
    {
        if (this == PathfindingAStar.Instance.player) return;
        if (this == PathfindingAStar.Instance.target) return;
        GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spriteCurrent;
    }
    public void UpdateDisplayPlayer()
    {
        if(PathfindingAStar.Instance.player!=null) PathfindingAStar.Instance.player.UpdateDisplayOrigin(false,true);
        PathfindingAStar.Instance.player = this;
        isObstacle=false;
        GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spritePlayer;
    }
    public void UpdateDisplayOrigin(bool checkPlayer=true,bool checkTarget=true)
    {
        if(checkPlayer) if (this == PathfindingAStar.Instance.player) return;
        if(checkTarget) if (this == PathfindingAStar.Instance.target) return;
        isObstacle = false;
        GetComponent<SpriteRenderer>().sprite = GridManager.Instance.spriteOrigin;
        canvasText.gameObject.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDaging && IsChangePlayerUpdate)
        {
            SetPlayerLockDrag();
            UpdateDisplayPlayer();
            SetPlayerUnLockDrag();
            PathfindingAStar.Instance.RestartFind();
            return;
        }
        if (isDaging&& pickPrepare>=2) OnPointerSetStatus();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (pickPrepare >= 2) OnPointerSetStatus();
        isDaging = true;
        if (this != PathfindingAStar.Instance.player)
        {
            SetPlayerLockDrag();
            IsChangePlayerUpdate = false;
        }
    }
    public void SetPlayerUnLockDrag()
    {
        PathfindingAStar.Instance.player.transform.localScale = Vector3.one*1.5f;
        PathfindingAStar.Instance.player.transform.GetComponent<SpriteRenderer>().sortingOrder = 2;
    }
    public void SetPlayerLockDrag()
    {
        PathfindingAStar.Instance.player.transform.localScale = Vector3.one;
        PathfindingAStar.Instance.player.transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDaging =false;
        if (IsChangePlayerUpdate)
        {
            SetPlayerLockDrag();
            IsChangePlayerUpdate = false;
        }
    }
    public void SetTextNode()
    {
        //return;
        gText.text = Math.Round(gCost,1).ToString();
        hText.text = Math.Round(hCost,1).ToString();
        fText.text = Math.Round(FCost,1).ToString();
        canvasText.gameObject.SetActive(true);
    }
    public void OnDrag(PointerEventData eventData)
    {

    }
}
