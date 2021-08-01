using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : BaseState
{
    public float radius = 10;
    public float switchDistance = 1;

    private Vector3 targetPosition;
    public override void OnEnter()
    {
        base.OnEnter();
        var r = Random.insideUnitCircle * radius;
        targetPosition = transform.position + new Vector3(r.x, 0, r.y);
    }
    public override void HandleUpdate()
    {
        base.HandleUpdate();
        var dirToTarget = targetPosition - Unit.transform.position;
        dirToTarget.y = 0;
        if (dirToTarget.magnitude < switchDistance)
        {
            ShouldSwitch = true;
            Unit.velocity = Vector3.zero;
        }
        else
        {
            Unit.velocity = dirToTarget.normalized * Unit.movementSpeed;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (!IsActive)
        {
            return;
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}
