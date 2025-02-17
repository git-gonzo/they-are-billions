using Photon.Deterministic;
using UnityEngine;


namespace Quantum
{
    public class TroopAsset : UnitAsset 
    {
        UnitType unitType => UnitType.Troop;

        [SerializeField] private FP attack;
        [SerializeField] private FP stopDistance;
        [SerializeField] private FP detectionRange;
        [SerializeField] private FP attackRange;
        [SerializeField] private FP attackCooldown;
        public override void UpdateUnit(Frame f, EntityRef entity)
        {
            base.UpdateUnit(f, entity);
            UpdateTroop(f, entity);
        }

        public unsafe void UpdateTroop(Frame f, EntityRef entity)
        {
            if (!f.Unsafe.TryGetPointer<UnitComponent>(entity, out var unit)) return;
            if (!f.Unsafe.TryGetPointer<Transform3D>(entity, out var transform)) return;
            
            switch (unit->state)
            {
                case UnitState.Idle:
                    unit->CurrentTime += f.DeltaTime;
                    if (unit->CurrentTime >= 1) //Patrol
                    {
                        if(unit->targetEntity != EntityRef.None)
                            SetUnitState(f, entity, UnitState.Moving);
                        //TryToFindTarget(f, entity, unit, *transform);
                        unit->CurrentTime = 0;
                        
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

                    //Update navmesh if target is moving
                    if(unit->targetEntity != EntityRef.None)
                    {
                        if (!RangeHelper.IsInRange(f, unit->targetEntity, pathfinder->Target, FP._0_10))
                        {
                            var t = f.Get<Transform3D>(unit->targetEntity).Position;
                            MoveTo(f, entity, t);
                        }
                    }
                    
                    
                    //Check if target is in range 
                    if (!RangeHelper.IsInRange(f, entity, unit->targetEntity, attackRange))
                    {
                        return;
                    }

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
                var target = FindClosestEnemyInRange(f,transform.Position, detectionRange);
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

        public override FPVector3 GetSpawnPoint(Frame f)
        {
            return SpawnPointHelper.GetSpawnPoint<TroopSpawnPointComponent>(f);
        }
    }
}

