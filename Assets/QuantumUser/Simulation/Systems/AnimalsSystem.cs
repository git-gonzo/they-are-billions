namespace Quantum
{
    using Photon.Deterministic;
    using System;

    public unsafe class AnimalsSystem : SystemMainThreadFilter<AnimalsSystem.Filter>, ISignalOnEntityPrototypeMaterialized
    {
        public void OnEntityPrototypeMaterialized(Frame f, EntityRef entity, EntityPrototypeRef prototypeRef)
        {
            if (f.TryGet<AnimalComponent>(entity, out var unit))
            {
                var pathfinder = f.Unsafe.GetPointer<NavMeshSteeringAgent>(entity);
                pathfinder->MaxSpeed = unit.Speed;
            }
        }

        public struct Filter
        {
            public EntityRef Entity;
            public AnimalComponent* AnimalComponent;
            public Transform3D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var unit = filter.AnimalComponent;
            switch (unit->state)
            {
                case AnimalState.Idle:
                    unit->CurrentTime += f.DeltaTime;
                    if (unit->CurrentTime >= 2) //Patrol
                    {
                        unit->CurrentTime = 0;
                        NavMesh navMesh = f.Map.GetNavMesh("NavMesh");
                        if (navMesh.FindRandomPointOnNavmesh(filter.Transform->Position, 5, f.RNG, *f.NavMeshRegionMask, out var dest))
                        {
                            MoveTo(f, filter.Entity, dest);
                            SetUnitState(f, filter.Entity, AnimalState.Moving);
                        }
                    }
                    break;
                case AnimalState.Moving:
                    NavMeshPathfinder* pathfinder = f.Unsafe.GetPointer<NavMeshPathfinder>(filter.Entity);
                    if (!pathfinder->IsActive) 
                    {
                        SetUnitState(f, filter.Entity, AnimalState.Idle);
                    }
                    break;
            }
        }

        private void MoveTo(Frame f, EntityRef entity, FPVector3 dest)
        {
            NavMeshPathfinder* pathfinder = f.Unsafe.GetPointer<NavMeshPathfinder>(entity);
            var t = f.Get<Transform3D>(entity);
            NavMesh navMesh = f.Map.GetNavMesh("NavMesh");
            pathfinder->SetTarget(f, dest, navMesh);
            t.LookAt(dest);
        }

        private void SetUnitState(Frame f, EntityRef unitEntity, AnimalState state)
        {
            var unit = f.Unsafe.GetPointer<AnimalComponent>(unitEntity);
            unit->state = state;
            switch (unit->state)
            {
                case AnimalState.Idle:
                    f.Events.UnitIdle(unitEntity); break;
                case AnimalState.Moving:
                    f.Events.UnitMoving(unitEntity); break;
                case AnimalState.Running:
                    //f.Events.UnitHarvesting(unitEntity, ResourceType.Stone); break;
                    break;
            }
        }

    }
}
