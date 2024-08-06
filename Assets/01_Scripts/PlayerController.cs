using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    Animator animator;
    [SerializeField] float speedMove;
    Queue<Vector3> gridsPos = new Queue<Vector3>();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Instance = this;
    }
    public void SetStateRunAnimator(int value)
    {
        if (value == 0) StopMoveAnimation();
        if (value == animator.GetInteger("Run")) return;
        animator.SetInteger("Run", value);
    }
    void MoveToPos(Vector3 newPos, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, newPos, speed * Time.deltaTime);
    }
    IEnumerator MoveAllGridPos()
    {
        while (gridsPos.Count != 0 || transform.position != gridsPos.Last())
        {
            if (transform.position == gridsPos.Peek())
            {
                if (gridsPos.Count != 0) gridsPos.Dequeue();
                if (gridsPos.Count == 0)
                {
                    SetStateRunAnimator(0);
                    yield break;
                }
                if (animator.GetInteger("Run") != GetAnimatorState(gridsPos.Peek()))
                {
                    SetStateRunAnimator(GetAnimatorState(gridsPos.Peek()));
                    Debug.Log(GetAnimatorState(gridsPos.Peek())+" "+transform.position+ " "+gridsPos.Peek());
                }
            }
            MoveToPos(gridsPos.Peek(), speedMove);
            yield return null;
        }
    }
    void SetGridsPos(List<Vector3> gridsPos)
    {
        this.gridsPos.Clear();
        transform.position = gridsPos[0];
        foreach (var grid in gridsPos) this.gridsPos.Enqueue(grid);
    }

    public void StartMoveWithPath(List<Vector3> gridsPos)
    {
        StopMoveAnimation();
        gridsPos.Reverse();
        SetGridsPos(gridsPos);        
        StartCoroutine(MoveAllGridPos());
    }
    public void StopMoveAnimation()
    {
        StopAllCoroutines();
    }
    int GetAnimatorState(Vector3 targetPos)
    {
        Vector3 currentPos = transform.position;
        Vector3 distance = targetPos - transform.position;
        float angle = 0;
        if (targetPos.x >= currentPos.x && targetPos.y >= currentPos.y)
        {
            angle = Vector3.Angle(Vector3.right, distance);
            if (angle <= 60) return 2;
            return 4;
        }
        else if (targetPos.x <= currentPos.x && targetPos.y >= currentPos.y)
        {
            angle = Vector3.Angle(Vector3.left, distance);
            if (angle <= 60) return 1;
            return 4;
        }
        else if (targetPos.x <= currentPos.x && targetPos.y <= currentPos.y)
        {
            angle = Vector3.Angle(Vector3.left, distance);
            if (angle <= 60) return 1;
            return 3;
        }
        else if (targetPos.x >= currentPos.x && targetPos.y <= currentPos.y)
        {
            angle = Vector3.Angle(Vector3.right, distance);
            if (angle <= 60) return 2;
            return 3;
        }
        return 0;
    }
}
