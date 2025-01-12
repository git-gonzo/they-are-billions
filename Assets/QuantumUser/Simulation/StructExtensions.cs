using Photon.Deterministic;
using System;

namespace Quantum
{
    [Serializable]
    public unsafe partial struct ResourceAmount
    {
        public void Serialize(BitStream stream)
        {
            stream.Serialize(ref Amount);
            //Serializing enums like this:
            int resourceValue = (int)Resource;
            stream.Serialize(ref resourceValue);
            Resource = (ResourceType)resourceValue;
        }
    }
}
