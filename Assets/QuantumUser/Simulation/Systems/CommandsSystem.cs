namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class CommandsSystem : SystemMainThreadFilter<CommandsSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var command = f.GetPlayerCommand(filter.PlayerLink->PlayerRef);

            if (command is CommandAddWorker)
            {
                var c = command as CommandAddWorker;
                f.Signals.OnAddWorkerToBuilding(c.buildingEntityRef, c.amount);
            }
            else if (command is CommandConsumeCost)
            {
                var c = command as CommandConsumeCost;
                f.Signals.ConsumeCost(c.PlayerEntityRef, c.resourceAmount);
            }
            else if (command is CommandConsumeCost)
            {
                var c = command as CommandConsumeCost;
                f.Signals.ConsumeCost(c.PlayerEntityRef, c.resourceAmount);
            }
            else if (command is CommandBuyUnit)
            {
                var c = command as CommandBuyUnit;
                if (!Canbuy(f, c.PlayerEntityRef, c.resourceAmount)) return;
                
                f.Signals.ConsumeCost(c.PlayerEntityRef, c.resourceAmount);
                f.Signals.CreateUnit(c.PlayerEntityRef, c.unitType);
            }
            else if (command is CommandPlaceBuilding)
            {
                var c = command as CommandPlaceBuilding;
                var building = f.FindAsset(c.building);
                if (!Canbuy(f, c.PlayerEntityRef, building.cost)) return;
                
                f.Signals.ConsumeCost(c.PlayerEntityRef, building.cost);
                f.Signals.CreateBuilding(c.PlayerEntityRef, c.building, c.position);
            }
            else if (command is CommandMoveUnit)
            {
                var c = command as CommandMoveUnit;
                f.Signals.OnMoveUnit(c.entity, c.destination);
            }
        }
        private bool Canbuy(Frame f, EntityRef playerEntity, ResourceAmount cost) 
        {
            if (!f.TryGet<PlayerEconomyComponent>(playerEntity, out var economy)) return false;
            var resources = f.ResolveList(economy.resources);
            foreach (var r in resources)
            {
                if (r.Resource == cost.Resource)
                {
                    Debug.Log($"Simulation, cannot buy ${cost.Amount} {cost.Resource} --Hack?--");
                    if (r.Amount < cost.Amount) return false;
                }
            }
            return true;
        }
    }
}
