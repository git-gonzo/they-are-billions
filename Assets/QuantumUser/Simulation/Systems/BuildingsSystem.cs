using Photon.Deterministic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Quantum
{
    public unsafe class BuildingsSystem : SystemSignalsOnly, ISignalOnAddWorkerToBuilding, ISignalCreateBuilding
    {
        public void CreateBuilding(Frame f, EntityRef playerEntity, AssetRef<BuildingConfig> building, FPVector3 position)
        {
            var buildingConfig = f.FindAsset(building);
            var b = f.Create(buildingConfig.buildingPrototype);
            f.Unsafe.GetPointer<Transform3D>(b)->Position = position;
            f.Unsafe.GetPointer<ResourceCollectorComponent>(b)->playerEntity = playerEntity;
        }

        public void OnAddWorkerToBuilding(Frame f, EntityRef buildingEntity, int amount)
        {
            
            var building = f.Unsafe.GetPointer<ResourceCollectorComponent>(buildingEntity);
            var playerEntity = building->playerEntity;

            //if can find a free worker assign it to building
            var filter = f.Filter<UnitComponent, FarmerComponent>();
            while(filter.Next(out var unitEntity, out var unitComponent, out var farmer)) 
            {
                if (unitComponent.playerOwner != playerEntity) continue;

                if (amount > 0) //TRY TO ADD WORKER
                {
                    if (farmer.buildingAssigned != EntityRef.None) continue;
                    SetBuilding(f, playerEntity, unitEntity, buildingEntity, building, amount);
                    return;
                }
                else  // REMOVE WORKER FROM BUILDING, SET HIM FREE
                {
                    if (farmer.buildingAssigned != buildingEntity) continue;
                    SetBuilding(f, playerEntity, unitEntity, buildingEntity, building, -amount);
                    return;
                }
            }
        }

        private void SetBuilding(Frame f, EntityRef playerEntity, EntityRef unitEntity, EntityRef buildingEntity, ResourceCollectorComponent* building, int amount) 
        {
            var farmerPointer = f.Unsafe.GetPointer<FarmerComponent>(unitEntity);
            farmerPointer->buildingAssigned = buildingEntity;
            building->workersAssigned += amount;
            f.Events.UpdateWorkers(playerEntity);
        }
    }
}
