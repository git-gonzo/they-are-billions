namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class UnitsSystem : SystemMainThreadFilter<UnitsSystem.Filter>,
        ISignalCreateUnit,
        ISignalOnEntityPrototypeMaterialized
    {
        public void CreateUnit(Frame f, EntityRef playerEntity)
        {
            var config = f.SimulationConfig.GameConfig;
            var unit = f.Create(config.workerPrototype);
            var unitComponent = f.Unsafe.GetPointer<UnitComponent>(unit);
            unitComponent->playerOwner = playerEntity;
            f.Unsafe.GetPointer<Transform3D>(unit)->Position = SpawnPointHelper.GetSpawnPoint<WorkerSpawnPointComponent>(f);
            f.Events.UpdateWorkers(playerEntity);
        }

        public void OnEntityPrototypeMaterialized(Frame f, EntityRef entity, EntityPrototypeRef prototypeRef)
        {
            if(f.TryGet<UnitComponent>(entity, out var unit)) 
            {
                var pathfinder = f.Unsafe.GetPointer<NavMeshSteeringAgent>(entity);
                pathfinder->MaxSpeed = unit.Speed;
            }
        }

        public struct Filter
        {
            public EntityRef Entity;
            public UnitComponent* UnitComponent;
            public Transform3D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var config = f.SimulationConfig.GameConfig;
            var unit = filter.UnitComponent;
            TryToFindTarget(f, filter.Entity, unit, *filter.Transform);

            switch (unit->state) 
            {
                case UnitState.Idle:
                    unit->CurrentTime += f.DeltaTime;
                    if (unit->CurrentTime >= 2) //Patrol
                    {
                        unit->CurrentTime = 0;
                        NavMesh navMesh = f.Map.GetNavMesh("NavMesh");
                        if(navMesh.FindRandomPointOnNavmesh(filter.Transform->Position, 5, f.RNG, *f.NavMeshRegionMask, out var dest)) 
                        {
                            MoveTo(f, filter.Entity, dest);
                            SetUnitState(f, filter.Entity, UnitState.Moving);
                        }
                    }
                    break;

                case UnitState.Moving:
                    //Check if target still exist
                    if (unit->targetEntity != EntityRef.None && !f.Exists(unit->targetEntity)) 
                    {
                        if (!TryToFindTarget(f, filter.Entity, unit, *filter.Transform))
                        {
                            SetUnitState(f, filter.Entity, UnitState.Idle);
                        }
                        else
                        {
                            SetUnitState(f, filter.Entity, UnitState.Moving);
                        }
                        return;
                    }
                    NavMeshPathfinder* pathfinder = f.Unsafe.GetPointer<NavMeshPathfinder>(filter.Entity);
                    //if (FPVector3.Distance(filter.Transform->Position, pathfinder->Target) > 1) return;

                    //Check if target has stop distance
                    if (f.TryGet<ResourceContainerComponent>(unit->targetEntity, out var resourceContainer))
                    {
                        if (FPVector3.Distance(filter.Transform->Position, pathfinder->Target) > resourceContainer.workerStopDistance)
                        {
                            //Update navmesh if target is moving
                            var t = f.Get<Transform3D>(unit->targetEntity).Position;
                            if (t != pathfinder->Target)
                            {
                                MoveTo(f, filter.Entity, t);
                                Debug.Log("Target updated");
                            }
                            return;
                        }
                    }
                    else if (FPVector3.Distance(filter.Transform->Position, pathfinder->Target) > FP._0_50) return;

                    pathfinder->IsActive = false;

                    if (unit->targetEntity == EntityRef.None) //Was patrolling
                        SetUnitState(f, filter.Entity, UnitState.Idle);
                    else if (unit->inventory.Amount == 0) 
                        SetUnitState(f, filter.Entity, UnitState.Harversting);
                    else 
                        SetUnitState(f, filter.Entity, UnitState.Deploying);
                    break;

                case UnitState.Harversting:
                    unit->CurrentTime += f.DeltaTime;
                    //Check if target still exist
                    if (unit->targetEntity != EntityRef.None && !f.Exists(unit->targetEntity))
                    {
                        if(!TryToFindTarget(f, filter.Entity, unit, *filter.Transform)) 
                        {
                            SetUnitState(f, filter.Entity, UnitState.Idle);
                        }
                        else 
                        {
                            SetUnitState(f, filter.Entity, UnitState.Moving);
                        }
                        unit->CurrentTime = 0;
                        Debug.Log("Unit was harvesting but target was destroyed");
                        return;
                    }
                    if (unit->CurrentTime >= unit->HaverstTime) //Harvesting END
                    {
                        //Set Inventory and deplete ResourceContainer amount
                        var container = f.Unsafe.GetPointer<ResourceContainerComponent>(unit->targetEntity);
                        container->resources.Amount -= 1;
                        if(container->resources.Amount == 0) 
                        {
                            f.Destroy(unit->targetEntity);
                        }

                        var resource = container->resources.Resource;
                        unit->inventory.Amount = 1;
                        unit->inventory.Resource = resource;

                        //Set Target to House, state to moving 
                        if(f.TryGet(unit->buildingAssigned, out Transform3D t)) 
                        {
                            unit->targetEntity = unit->buildingAssigned;
                            var dest = f.Get<Transform3D>(unit->buildingAssigned).Position;
                            MoveTo(f, filter.Entity, dest);
                            SetUnitState(f, filter.Entity, UnitState.Moving);
                        }
                        else 
                        {
                            SetUnitState(f, filter.Entity, UnitState.Idle);
                        }
                        unit->CurrentTime = 0;
                    }
                    break;
                case UnitState.Deploying:
                    unit->CurrentTime += f.DeltaTime;
                    if (unit->CurrentTime >= unit->DeployTime) //Harvesting END
                    {
                        var resource = unit->inventory;
                        unit->inventory = new ResourceAmount();
                        unit->CurrentTime = 0;
                        SetUnitState(f, filter.Entity, UnitState.Idle);
                        f.Signals.OnResourceAdded(unit->playerOwner, resource);
                    }
                    break;
            }
        }

        private bool TryToFindTarget(Frame f, EntityRef entity, UnitComponent* unit, Transform3D transform)
        {
            if (unit->buildingAssigned != EntityRef.None &&
                unit->inventory.Amount <= 0 && unit->state == UnitState.Idle)
            {
                var resourceType = f.Get<ResourceCollectorComponent>(unit->buildingAssigned).lookForResource;
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

        private void SetUnitState(Frame f, EntityRef unitEntity, UnitState state) 
        {
            var unit = f.Unsafe.GetPointer<UnitComponent>(unitEntity);
            unit->state = state;
            switch (unit->state)
            {
                case UnitState.Idle:
                    f.Events.UnitIdle(unitEntity); break;
                case UnitState.Moving:
                    FPVector3 dest;
                    if (unit->targetEntity != EntityRef.None)
                        dest = f.Get<Transform3D>(unit->targetEntity).Position;
                    else
                        dest = unit->patrolTarget;
                    var t = f.Unsafe.GetPointer<Transform3D>(unitEntity);
                    t->LookAt(dest);
                    f.Events.UnitMoving(unitEntity); break;
                case UnitState.Harversting:
                    f.Events.UnitHarvesting(unitEntity, ResourceType.Stone); break;
            }
        }

        private EntityRef FindClosestResourceContainer(Frame f, ResourceType resourceType, FPVector3 pos)
        {
            var filter = f.Filter<ResourceContainerComponent, Transform3D>();
            var entityCandidate = EntityRef.None;
            FP minDistance = FP.MaxValue;
            while (filter.Next(out EntityRef entity, out var comp, out var t)) 
            {
                if(comp.resources.Resource == resourceType) 
                {
                    var distance = FPVector3.Distance(pos, t.Position);
                    if(distance < minDistance) 
                    {
                        entityCandidate = entity;
                        minDistance = distance;
                    }
                }
            }
            return entityCandidate;
        }

        private void MoveTo(Frame f, EntityRef entity, FPVector3 dest) 
        {
            NavMeshPathfinder* pathfinder = f.Unsafe.GetPointer<NavMeshPathfinder>(entity);
            NavMesh navMesh = f.Map.GetNavMesh("NavMesh");
            pathfinder->SetTarget(f, dest, navMesh);
        }


    }
}
