using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class FarmerAsset : UnitAsset 
    {
        [SerializeField] private FP deployTime;
        [SerializeField] private int resourcesCapacity;
        [SerializeField] private FP haverstTime;

        public override void UpdateUnit(Frame f, EntityRef entity)
        {
            base.UpdateUnit(f, entity);
            UpdateFarmer(f, entity);
        }

        public unsafe void UpdateFarmer(Frame f, EntityRef entity)
        {
            if (!f.Unsafe.TryGetPointer<UnitComponent>(entity, out var unit)) return;
            if (!f.Unsafe.TryGetPointer<FarmerComponent>(entity, out var farmComp)) return;
            if (!f.Unsafe.TryGetPointer<Transform3D>(entity, out var transform)) return;

            //Todo: Confirm that this is needed
            TryToFindTarget(f, entity, unit, farmComp, *transform);
            
            switch (unit->state)
            {
                case UnitState.Idle:
                    unit->CurrentTime += f.DeltaTime;
                    if (unit->CurrentTime >= 2) //Patrol
                    {
                        unit->CurrentTime = 0;
                        NavMesh navMesh = f.Map.GetNavMesh("NavMesh");
                        if (navMesh.FindRandomPointOnNavmesh(transform->Position, 5, f.RNG, *f.NavMeshRegionMask, out var dest))
                        {
                            MoveTo(f, entity, dest);
                            SetUnitState(f, entity, UnitState.Moving);
                        }
                    }
                    break;

                case UnitState.Moving:
                    //Check if target still exist
                    if (unit->targetEntity != EntityRef.None && !f.Exists(unit->targetEntity))
                    {
                        if (!TryToFindTarget(f, entity, unit, farmComp, *transform))
                        {
                            SetUnitState(f, entity, UnitState.Idle);
                        }
                        else
                        {
                            SetUnitState(f, entity, UnitState.Moving);
                        }
                        return;
                    }
                    NavMeshPathfinder* pathfinder = f.Unsafe.GetPointer<NavMeshPathfinder>(entity);
                    //if (FPVector3.Distance(filter.Transform->Position, pathfinder->Target) > 1) return;

                    //Check if target has stop distance
                    if (f.TryGet<ResourceContainerComponent>(unit->targetEntity, out var resourceContainer))
                    {
                        if (FPVector3.Distance(transform->Position, pathfinder->Target) > resourceContainer.workerStopDistance)
                        {
                            //Update navmesh if target is moving
                            var t = f.Get<Transform3D>(unit->targetEntity).Position;
                            if (t != pathfinder->Target)
                            {
                                MoveTo(f, entity, t);
                            }
                            return;
                        }
                    }
                    else if (FPVector3.Distance(transform->Position, pathfinder->Target) > FP._0_50) return;

                    pathfinder->IsActive = false;

                    if (unit->targetEntity == EntityRef.None) //Was patrolling
                        SetUnitState(f, entity, UnitState.Idle);
                    else if (farmComp->inventory.Amount == 0)
                        SetUnitState(f, entity, UnitState.Harversting);
                    else
                        SetUnitState(f, entity, UnitState.Deploying);
                    break;

                case UnitState.Harversting:
                    unit->CurrentTime += f.DeltaTime;
                    //Check if target still exist
                    if (unit->targetEntity != EntityRef.None && !f.Exists(unit->targetEntity))
                    {
                        if (!TryToFindTarget(f, entity, unit, farmComp, *transform))
                        {
                            SetUnitState(f, entity, UnitState.Idle);
                        }
                        else
                        {
                            SetUnitState(f, entity, UnitState.Moving);
                        }
                        unit->CurrentTime = 0;
                        return;
                    }
                    if (unit->CurrentTime >= haverstTime) //Harvesting END
                    {
                        //Set Inventory and deplete ResourceContainer amount
                        var container = f.Unsafe.GetPointer<ResourceContainerComponent>(unit->targetEntity);
                        container->resources.Amount -= 1;
                        if (container->resources.Amount == 0)
                        {
                            f.Destroy(unit->targetEntity);
                        }

                        var resource = container->resources.Resource;
                        farmComp->inventory.Amount = 1;
                        farmComp->inventory.Resource = resource;

                        //Set Target to House, state to moving 
                        if (f.TryGet(farmComp->buildingAssigned, out Transform3D t))
                        {
                            unit->targetEntity = farmComp->buildingAssigned;
                            var dest = f.Get<Transform3D>(farmComp->buildingAssigned).Position;
                            MoveTo(f, entity, dest);
                            SetUnitState(f, entity, UnitState.Moving);
                        }
                        else
                        {
                            SetUnitState(f, entity, UnitState.Idle);
                        }
                        unit->CurrentTime = 0;
                    }
                    break;
                case UnitState.Deploying:
                    unit->CurrentTime += f.DeltaTime;
                    if (unit->CurrentTime >= deployTime) //Harvesting END
                    {
                        var resource = farmComp->inventory;
                        farmComp->inventory = new ResourceAmount();
                        unit->CurrentTime = 0;
                        SetUnitState(f, entity, UnitState.Idle);
                        f.Signals.OnResourceAdded(unit->playerOwner, resource);
                    }
                    break;
            }

        }
        private unsafe bool TryToFindTarget(Frame f, EntityRef entity, UnitComponent* unit, FarmerComponent* farmer, Transform3D transform)
        {
            
            if (farmer->buildingAssigned != EntityRef.None &&
                farmer->inventory.Amount <= 0 && unit->state == UnitState.Idle)
            {
                var resourceType = f.Get<ResourceCollectorComponent>(farmer->buildingAssigned).lookForResource;
                var target = FindClosestResourceContainer(f, resourceType, transform.Position);
                if (target == EntityRef.None) return false;

                var dest = f.Get<Transform3D>(target).Position;
                unit->targetEntity = target;
                MoveTo(f, entity, dest);
                SetUnitState(f, entity, UnitState.Moving);
                f.Events.UnitMoving(entity);
                return true;
            }
            return false;
        }
        
    }
}

