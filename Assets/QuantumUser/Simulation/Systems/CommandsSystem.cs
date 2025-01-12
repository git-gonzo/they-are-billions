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
            else if (command is CommandBuyUnit)
            {
                var c = command as CommandBuyUnit;
                f.Signals.ConsumeCost(c.PlayerEntityRef, c.resourceAmount);
                f.Signals.CreateUnit(c.PlayerEntityRef);
            }
        }
    }
}
