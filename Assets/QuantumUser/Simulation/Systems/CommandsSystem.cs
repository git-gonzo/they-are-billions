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
                //Debug.Log("Command ADD WORKER");
                var c = command as CommandAddWorker;
                f.Signals.OnWorkerAdded(c.buildingEntityRef, c.amount);
            }
        }
    }
}
