using UnityEngine;
using Quantum;
using TMPro;
using UnityEngine.UI;
using System;
public class EconomyViewController : QuantumSceneViewComponent
{
    [SerializeField] TextMeshProUGUI _txtWorkers;
    EntityRef _localPlayerEntity;
    public override void OnActivate(Frame f)
    {
        base.OnActivate(f);
        GetLocalPlayerEntity();
        QuantumEvent.Subscribe<EventUpdateWorkers>(this,OnUpdateWorkers);
        UpdateWorkers();
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
    private void OnUpdateWorkers(EventUpdateWorkers callback)
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
}
