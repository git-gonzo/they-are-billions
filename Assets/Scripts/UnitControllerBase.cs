using UnityEngine;
using Quantum;
using System;

public class UnitControllerBase : QuantumEntityViewComponent
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);
        animator = GetComponentInChildren<Animator>();
        if(animator != null)
        {
            animator.speed = frame.Get<UnitComponent>(EntityRef).Speed.AsFloat;
        }
        QuantumEvent.Subscribe<EventUnitMoving>(this, OnUnitMoving);
        QuantumEvent.Subscribe<EventUnitHarvesting>(this, OnUnitHarvesting);
        QuantumEvent.Subscribe<EventUnitIdle>(this, OnUnitIdle);
    }

    private void OnUnitMoving(EventUnitMoving e)
    {
        if (e.unitEntity != EntityRef) return;
        if (animator == null) return;
        animator.SetTrigger("Walk");
    }
    private void OnUnitHarvesting(EventUnitHarvesting e)
    {
        if (e.unitEntity != EntityRef) return;
        if (animator == null) return;
        animator.SetTrigger("Harvest");
    }
    private void OnUnitIdle(EventUnitIdle e)
    {
        if (e.unitEntity != EntityRef) return;
        if (animator == null) return;
        animator.SetTrigger("Idle");
    }
}
