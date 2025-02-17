namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.Scripting;
    using static UnityEngine.EventSystems.EventTrigger;

    [Preserve]
    public unsafe class UnitsSystem : SystemMainThreadFilter<UnitsSystem.Filter>,
        ISignalCreateUnit,
        ISignalOnEntityPrototypeMaterialized,
        ISignalOnEntityDie,
        ISignalOnMoveUnit,
        ISignalOnSetAttack
    {

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

            f.FindAsset(unit->unitAsset).UpdateUnit(f, filter.Entity);
        }

        public void OnEntityDie(Frame f, EntityRef entity)
        {
            f.Destroy(entity);
        }

        public void CreateUnit(Frame f, EntityRef playerEntity, UnitType unitType)
        {
            var unit = f.Create(GetUnitPrototype(f, unitType));
            var unitComponent = f.Unsafe.GetPointer<UnitComponent>(unit);
            unitComponent->playerOwner = playerEntity;
            var asset = f.FindAsset(unitComponent->unitAsset);
            asset.Init(f, unit);
            f.Unsafe.GetPointer<Transform3D>(unit)->Position = asset.GetSpawnPoint(f);
            f.Events.UpdateWorkers(playerEntity);
        }

        private AssetRef<EntityPrototype> GetUnitPrototype(Frame f, UnitType unitType) 
        {
            var config = f.SimulationConfig.GameConfig;
            switch (unitType)
            {
                case UnitType.Farmer:
                    return config.workerPrototype;
                case UnitType.Troop:
                    return config.troopsConfig.basicTroopPrototype;
            }
            return null;
        }

        public void OnMoveUnit(Frame f, EntityRef entity, FPVector3 destination)
        {
            if (!f.TryGet(entity, out UnitComponent unitComponent)) return;

            var asset = f.FindAsset(unitComponent.unitAsset);
            asset.MoveUnitTo(f, entity, destination);
        }

        public void OnSetAttack(Frame f, EntityRef attacker, EntityRef enemyTarget, FPVector3 destination)
        {
            if (!f.TryGet(attacker, out UnitComponent unitComponent)) return;

            var asset = f.FindAsset(unitComponent.unitAsset);
            asset.MoveUnitToAttack(f, attacker,enemyTarget, destination);
        }
    }
}
