using Photon.Deterministic;
using UnityEngine;


namespace Quantum
{
    public class EnemyAsset : UnitAsset 
    {
        [SerializeField] private FP attack;
        [SerializeField] private FP stopDistance;
        [SerializeField] private FP detectionRange;
        [SerializeField] private FP attackRange;
        [SerializeField] private FP attackCooldown;
        public override void UpdateUnit(Frame f, EntityRef entity)
        {
            base.UpdateUnit(f, entity);
            UpdateEnemy(f, entity);
        }

        public unsafe void UpdateEnemy(Frame f, EntityRef entity)
        {
            if (!f.Unsafe.TryGetPointer<UnitComponent>(entity, out var unit)) return;
            if (!f.Unsafe.TryGetPointer<Transform3D>(entity, out var transform)) return;

            //Todo: Confirm that this is needed
            TryToFindTarget(f, entity, unit, *transform);
            
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
                        unit->targetEntity = EntityRef.None;
                        if (!TryToFindTarget(f, entity, unit, *transform))
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
                    if (f.TryGet<FarmerComponent>(unit->targetEntity, out var farmer))
                    {
                        if (FPVector3.Distance(transform->Position, pathfinder->Target) > stopDistance)
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

                    //Has arrived 
                    pathfinder->IsActive = false;


                    if (unit->targetEntity == EntityRef.None) //Was patrolling
                        SetUnitState(f, entity, UnitState.Idle);
                    else
                        SetUnitState(f, entity, UnitState.Attacking); //Was Moving
                    break;

                case UnitState.Attacking:
                    unit->CurrentTime -= f.DeltaTime;
                    f.TryGet<Transform3D>(unit->targetEntity, out var farmerTransform);
                    if (FPVector3.Distance(farmerTransform.Position, transform->Position) <= attackRange )
                    {
                        if (unit->CurrentTime <= 0) //Attacking
                        {
                            unit->CurrentTime = attackCooldown;
                            f.Signals.OnHealthChanged(unit->targetEntity,-attack);
                        }
                    }
                    else
                    {
                        SetUnitState(f, entity, UnitState.Idle);
                    }
                        break;
            }

        }
        private unsafe bool TryToFindTarget(Frame f, EntityRef entity, UnitComponent* unit, Transform3D transform)
        {

            if (unit->state == UnitState.Idle)
            {
                var target = FindClosestFarmerInRange(f,transform.Position, detectionRange);
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

