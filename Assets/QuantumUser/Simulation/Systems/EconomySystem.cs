namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class EconomySystem : SystemSignalsOnly, ISignalOnResourceAdded
    {
        public void OnResourceAdded(Frame f, EntityRef playerEntity, ResourceAmount resource)
        {
            //TODO: Add player entity
            var economy = f.Get<PlayerEconomyComponent>(playerEntity);
            //economy
        }
    }
}
