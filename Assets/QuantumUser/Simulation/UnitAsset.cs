namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public class UnitAsset : AssetObject
    {
        public virtual unsafe void Init(Frame f, EntityRef entity)
        {
            var healthComponent = f.Unsafe.GetPointer<HealthComponent>(entity);
            healthComponent->CurrentHealth = healthComponent->MaxHealth;
        }
        public virtual void UpdateUnit(Frame f, EntityRef entity) 
        { 

        }

        protected unsafe void SetUnitState(Frame f, EntityRef unitEntity, UnitState state)
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
                case UnitState.Attacking:
                    f.Events.UnitHarvesting(unitEntity, ResourceType.Stone); break;
            }
        }

        protected EntityRef FindClosestResourceContainer(Frame f, ResourceType resourceType, FPVector3 pos)
        {
            var filter = f.Filter<ResourceContainerComponent, Transform3D>();
            var entityCandidate = EntityRef.None;
            FP minDistance = FP.MaxValue;
            while (filter.Next(out EntityRef entity, out var comp, out var t))
            {
                if (comp.resources.Resource == resourceType)
                {
                    var distance = FPVector3.Distance(pos, t.Position);
                    if (distance < minDistance)
                    {
                        entityCandidate = entity;
                        minDistance = distance;
                    }
                }
            }
            return entityCandidate;
        }
        protected EntityRef FindClosestFarmerInRange(Frame f, FPVector3 pos, FP detectionRange)
        {
            var entityCandidate = EntityRef.None;
            FP minDistance = FP.MaxValue;
            var filter = f.Filter<FarmerComponent, Transform3D>();
            while (filter.Next(out EntityRef entity, out var farmer, out var t))
            {
                var distance = FPVector3.Distance(pos, t.Position);
                if (distance < minDistance && distance<detectionRange)
                {
                    entityCandidate = entity;
                    minDistance = distance;
                }              
            }
            return entityCandidate;
        }

        protected unsafe void MoveTo(Frame f, EntityRef entity, FPVector3 dest)
        {
            NavMeshPathfinder* pathfinder = f.Unsafe.GetPointer<NavMeshPathfinder>(entity);
            NavMesh navMesh = f.Map.GetNavMesh("NavMesh");
            pathfinder->SetTarget(f, dest, navMesh);
        }
    }
}
