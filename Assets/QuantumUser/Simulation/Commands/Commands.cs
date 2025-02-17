using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class CommandAddWorker : DeterministicCommand
    {
        public EntityRef buildingEntityRef;
        public int amount;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref buildingEntityRef);
            stream.Serialize(ref amount);
        }

        public void Execute(Frame f) 
        {
            
        }
    }    
    public class CommandMoveUnit : DeterministicCommand
    {
        public EntityRef entity;
        public FPVector3 destination;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref entity);
            stream.Serialize(ref destination);
        }

        public void Execute(Frame f) 
        {
            
        }
    }
    public class CommandAttackUnit : DeterministicCommand
    {
        public EntityRef attackerEntity;
        public EntityRef enemyEntity;
        public FPVector3 destination;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref attackerEntity);
            stream.Serialize(ref enemyEntity);
            stream.Serialize(ref destination);
        }

        public void Execute(Frame f) 
        {
            
        }
    }
    public class CommandBuyWorker : DeterministicCommand
    {
        public EntityRef buildingEntityRef;
        public int amount;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref buildingEntityRef);
            stream.Serialize(ref amount);
        }

        public void Execute(Frame f) 
        {
            
        }
    }
    public class CommandConsumeCost : DeterministicCommand
    {
        public EntityRef PlayerEntityRef;
        public ResourceAmount resourceAmount;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref PlayerEntityRef);
            resourceAmount.Serialize(stream);
        }

        public void Execute(Frame f)
        {

        }
    }
    public class CommandBuyUnit : DeterministicCommand
    {
        public EntityRef PlayerEntityRef;
        public ResourceAmount resourceAmount;
        public UnitType unitType;
            
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref PlayerEntityRef);
            resourceAmount.Serialize(stream);
            //Serialize UnitType
            int unitTypeSer = (int)unitType;
            stream.Serialize(ref unitTypeSer);
            unitType = (UnitType)unitTypeSer;
        }

        public void Execute(Frame f)
        {

        }
    }
    public class CommandPlaceBuilding : DeterministicCommand
    {
        public EntityRef PlayerEntityRef;
        public AssetRef<BuildingConfig> building;
        public FPVector3 position;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref PlayerEntityRef);
            stream.Serialize(ref building);
            stream.Serialize(ref position);
        }

        public void Execute(Frame f)
        {

        }
    }
}
