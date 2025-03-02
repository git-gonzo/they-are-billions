using UnityEngine;
using Quantum;
using System;

public class UnitControllerBase : QuantumEntityViewComponent<HUDContext>
{
    [SerializeField] LifeBar _lifeBarPrefab;
    [SerializeField] GameObject _view;
    [SerializeField] GameObject _selected;
    private LifeBar _lifeBar;
  
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);
        animator = GetComponentInChildren<Animator>();
        if(animator != null)
        {
            if(frame.TryGet<UnitComponent>(EntityRef, out var unit)) 
            { 
                animator.speed = unit.Speed.AsFloat;
            }
            else if(frame.TryGet<AnimalComponent>(EntityRef, out var animal)) 
            { 
                animator.speed = animal.Speed.AsFloat;
            }
        }
        QuantumEvent.Subscribe<EventUnitMoving>(this, OnUnitMoving);
        QuantumEvent.Subscribe<EventUnitHarvesting>(this, OnUnitHarvesting);
        QuantumEvent.Subscribe<EventUnitAttackRange>(this, OnUnitAttackRange);
        QuantumEvent.Subscribe<EventUnitIdle>(this, OnUnitIdle);
        QuantumEvent.Subscribe<EventOnHealthChanged>(this, OnHealthChanged);
        QuantumEvent.Subscribe<EventOnEntityDie>(this, OnEntityDie);

        //Instantiate Lifebar
        if (_lifeBarPrefab != null)
        {
            _lifeBar = Instantiate(_lifeBarPrefab, ViewContext.lifebarsContainer);
            _lifeBar.Init(transform);
        }
    }

    private void OnEntityDie(EventOnEntityDie e)
    {
        if (e.entity != EntityRef) return;
        var deadBody = Instantiate(_view);
        deadBody.transform.position = transform.position;
        var animator = deadBody.GetComponent<Animator>();
        animator.enabled = true;
        animator.applyRootMotion = true;
        var deathType = UnityEngine.Random.Range(0, 2) + 1;
        animator.SetTrigger("Die" + deathType);
        Destroy(deadBody,5);
    }

    private void OnHealthChanged(EventOnHealthChanged e)
    {
        if (e.entity != EntityRef) return;
        _lifeBar.SetLife(e.amount.AsFloat);
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
    private void OnUnitAttackRange(EventUnitAttackRange e)
    {
        if (e.unitEntity != EntityRef) return;
        if (animator == null) return;
        animator.SetTrigger("AttackRange");
    }

    public void SelectObject(bool value)
    {
        if (_selected == null) return;
        _selected.SetActive(value);
    }

    public void ShowLifeBar(bool value) 
    { 
        if (_lifeBar == null) return;
        if (_selected != null && _selected.activeSelf)
        {
            _lifeBar.Show(true);
            //_lifeBar.gameObject.SetActive(true);
            return;
        }
        _lifeBar.gameObject.SetActive(value);
    }
}
