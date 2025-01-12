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

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref PlayerEntityRef);
            resourceAmount.Serialize(stream);
        }

        public void Execute(Frame f)
        {

        }
    }
}
