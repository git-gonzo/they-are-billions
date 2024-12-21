namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class UnitsSystem2 : SystemMainThreadFilter<UnitsSystem2.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public UnitComponent* UnitComponent;
            public Transform3D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var unit = filter.UnitComponent;
            if(unit->buildingAssigned != EntityRef.None && 
                unit->inventory.Amount <= 0 && filter.UnitComponent->state == UnitState.Idle)
            {
                var resourceType = f.Get<ResourceCollectorComponent>(unit->buildingAssigned).lookForResource;
                var target = FindClosestResourceContainer(f, resourceType);
                unit->targetEntity = target;
                unit->state = UnitState.Moving;
            }

            switch (unit->state) 
            {
                case UnitState.Moving:
                    var dest = f.Get<Transform3D>(unit->targetEntity).Position;
                    var pos = FPVector3.Lerp(filter.Transform->Position, dest, unit->Speed * FP._0_01);
                    filter.Transform->Position = pos;
                    
                    //Check if has arrived
                    if(FPMath.Abs((filter.Transform->Position - dest).Magnitude) < 1) 
                    {
                        if(unit->inventory.Amount == 0) 
                        {
                            unit->state = UnitState.Harversting;
                        }
                        else 
                        {
                            unit->state = UnitState.Deploying;
                        }
                    }
                    break;
                case UnitState.Harversting:
                    unit->CurrentTime += f.DeltaTime;
                    if(unit->CurrentTime >= unit->HaverstTime) //Harvesting END
                    {
                        //Set Inventory and deplete ResourceContainer amount
                        var container = f.Unsafe.GetPointer<ResourceContainerComponent>(unit->targetEntity);
                        container->resources.Amount -= 1;

                        var resource = container->resources.Resource;
                        unit->inventory.Amount = 1;
                        unit->inventory.Resource = resource;

                        //Set Target to House, state to moving 
                        if(f.TryGet(unit->buildingAssigned, out Transform3D t)) 
                        { 
                            unit->state = UnitState.Moving;
                            unit->targetEntity = unit->buildingAssigned;
                        }
                        else 
                        {
                            unit->state = UnitState.Idle;
                        }
                        unit->CurrentTime = 0;
                    }
                    break;
                case UnitState.Deploying:
                    unit->CurrentTime += f.DeltaTime;
                    if (unit->CurrentTime >= unit->DeployTime) //Harvesting END
                    {
                        var resource = unit->inventory;
                        unit->inventory = new ResourceAmount();
                        unit->CurrentTime = 0;
                        unit->state = UnitState.Idle;
                        f.Signals.OnResourceAdded(unit->playerOwner, resource);
                    }
                    break;
            }
        }

        private EntityRef FindClosestResourceContainer(Frame f, ResourceType resourceType)
        {
            var filter = f.Filter<ResourceContainerComponent, Transform3D>();
            while (filter.Next(out EntityRef entity, out var comp, out var t)) 
            {
                if(comp.resources.Resource == resourceType)
                return entity;
            }
            return EntityRef.None;
        }


    }
}
