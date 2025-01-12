using UnityEngine;
using Quantum;
using TMPro;
using UnityEngine.UI;
using System;
public class EconomyViewController : QuantumSceneViewComponent
{
    [SerializeField] TextMeshProUGUI _txtWorkers;
    [SerializeField] TextMeshProUGUI _txtWood;
    [SerializeField] TextMeshProUGUI _txtStone;
    [SerializeField] TextMeshProUGUI _txtMeat;
    EntityRef _localPlayerEntity;
    public override void OnActivate(Frame f)
    {
        base.OnActivate(f);
        GetLocalPlayerEntity();
        QuantumEvent.Subscribe<EventUpdateWorkers>(this,OnUpdateWorkers);
        QuantumEvent.Subscribe<EventUpdateResources>(this,OnUpdateResources);
        UpdateWorkers();
        UpdateResources(_localPlayerEntity);
    }

    private void GetLocalPlayerEntity() 
    {
        var f = VerifiedFrame.Filter<PlayerLink>();
        while(f.Next(out var entity, out var link))
        {
            if (VerifiedFrame.IsPlayerVerifiedOrLocal(link.PlayerRef))
            {
                _localPlayerEntity = entity;
                return;
            }
        }
    }
    private void OnUpdateWorkers(EventUpdateWorkers e)
    {
        UpdateWorkers();
    }

    private void UpdateWorkers()
    {
        Debug.Log($"Update workers");
        var totalworkers = 0;
        var freeworkers = 0;
        var f = PredictedFrame.Filter<UnitComponent>();
        while(f.Next(out var entity,out var unit)) 
        {
            if(unit.playerOwner ==  _localPlayerEntity)
            {
                totalworkers++;
                if(unit.buildingAssigned == EntityRef.None) 
                {
                    freeworkers++;
                }
            }
        }
        Debug.Log($"Update workers {freeworkers}/{totalworkers}");
        _txtWorkers.text = $"{freeworkers}/{totalworkers}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void OnUpdateResources(EventUpdateResources e)
    {
        UpdateResources(e.playerEntity);
    }

    private void UpdateResources(EntityRef playerEntity)
    {
        var economy = PredictedFrame.Get<PlayerEconomyComponent>(playerEntity);
        var resources = PredictedFrame.ResolveList(economy.resources);
        _txtWood.text = "0";
        _txtStone.text = "0";
        _txtMeat.text = "0";

        foreach ( var resource in resources )
        {
            if(resource.Resource == ResourceType.Wood)
            {
                _txtWood.text = resource.Amount.ToString();
            }
            else if(resource.Resource == ResourceType.Stone)
            {
                _txtStone.text = resource.Amount.ToString();
            }
            else if(resource.Resource == ResourceType.Meat)
            {
                _txtMeat.text = resource.Amount.ToString();
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
