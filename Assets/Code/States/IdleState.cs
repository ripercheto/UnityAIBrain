using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseState
{
    public float duration = 5;

    private float endTime;
    public override void OnEnter()
    {
        base.OnEnter();
        endTime = Time.time + duration;
    }
    public override void HandleUpdate()
    {
        base.HandleUpdate();
        Unit.velocity = Vector3.zero;
        if (Time.time > endTime)
        {
            ShouldSwitch = true;
        }
    }
}
