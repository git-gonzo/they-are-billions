using Photon.Deterministic;
using UnityEngine.Scripting;
using static UnityEngine.EventSystems.EventTrigger;

namespace Quantum
{
    [Preserve]
    public unsafe class HealthSystem : SystemMainThreadFilter<HealthSystem.Filter>, ISignalOnHealthChanged
    {
        public struct Filter
        {
            public EntityRef Entity;
            public HealthComponent* healthComponent;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            if(filter.healthComponent->CurrentHealth <= 0) 
            {
                f.Events.OnEntityDie(filter.Entity);
                f.Signals.OnEntityDie(filter.Entity);
            }
        }

        public void OnHealthChanged(Frame f, EntityRef entity, FP amount)
        {
            if (!f.Unsafe.TryGetPointer<HealthComponent>(entity, out var healthComp)) return;
            var newHealth = healthComp->CurrentHealth + amount;
            healthComp->CurrentHealth = FPMath.Clamp(newHealth, 0, healthComp->MaxHealth);
            f.Events.OnHealthChanged(entity, newHealth/ healthComp->MaxHealth);
        }
    }
}
