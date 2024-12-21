using Quantum;
using System;
using UnityEngine;

namespace Quantum
{
    using UnityEngine;
    using Quantum;

    public class BuildingController : QuantumEntityViewComponent<HUDContext>
    {
        [SerializeField] BuildingData _buildingData;

        public override void OnActivate(Frame frame)
        {
            _buildingData.Entity = EntityRef;
        }

        public void OnClickBuilding() 
        {
            var buildingComp = PredictedFrame.Get<ResourceCollectorComponent>(EntityRef);
            var freeWorkers = GetFreeWorkers(buildingComp.playerEntity);

            ViewContext.hudController.ShowBuildingPopup(_buildingData, freeWorkers, buildingComp.workersAssigned);
            Debug.Log("Click on Building " + gameObject.name);
        }

        private int GetFreeWorkers(EntityRef playerEntity) 
        {
            var freeWorkers = 0;
            var filter = PredictedFrame.Filter<UnitComponent>();
            while (filter.Next(out var unitEntity, out var unitComponent))
            {
                if (unitComponent.playerOwner != playerEntity) continue;
                if (unitComponent.buildingAssigned == EntityRef.None) freeWorkers++;
            }
            return freeWorkers;
        }
    }
    
}

[Serializable]
public class BuildingData 
{
    public EntityRef Entity;
    public string buildingName;
    public string buildingDescrip;
    public Sprite icon;
    public int maxWorkers;
}