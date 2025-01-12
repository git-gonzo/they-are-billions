using UnityEngine;
using UnityEngine.UIElements;

namespace Quantum
{
    public unsafe class BuildingsSystem : SystemSignalsOnly, ISignalOnAddWorkerToBuilding
    {
        public void OnAddWorkerToBuilding(Frame f, EntityRef buildingEntity, int amount)
        {
            
            var building = f.Unsafe.GetPointer<ResourceCollectorComponent>(buildingEntity);
            var playerEntity = building->playerEntity;

            //if can find a free worker assign it to building
            var filter = f.Filter<UnitComponent>();
            while(filter.Next(out var unitEntity, out var unitComponent)) 
            {
                if (unitComponent.playerOwner != playerEntity) continue;

                if (amount > 0) //TRY TO ADD WORKER
                {
                    if (unitComponent.buildingAssigned != EntityRef.None) continue;
                    
                    var unitPointer = f.Unsafe.GetPointer<UnitComponent>(unitEntity);
                    unitPointer->buildingAssigned = buildingEntity;
                    building->workersAssigned += amount;
                    f.Events.UpdateWorkers(playerEntity);
                    return;
                }
                else  // REMOVE WORKER FROM BUILDING, SET HIM FREE
                {
                    if (unitComponent.buildingAssigned != buildingEntity) continue;
                    
                    var unitPointer = f.Unsafe.GetPointer<UnitComponent>(unitEntity);
                    unitPointer->buildingAssigned = EntityRef.None;
                    building->workersAssigned += amount;
                    f.Events.UpdateWorkers(playerEntity);
                    return;
                }
            }
        }
    }
}
