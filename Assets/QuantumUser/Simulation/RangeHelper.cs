using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class RangeHelper : MonoBehaviour
{
     public static bool IsInRange(Frame f, EntityRef from, EntityRef to, FP range)
     {
        if (!f.TryGet<Transform3D>(from, out var fromTransform)) return false;
        if (!f.TryGet<Transform3D>(to, out var toTransform)) return false;
        return FPVector3.Distance(fromTransform.Position, toTransform.Position) <= range;
     }
    public static bool IsInRange(Frame f, EntityRef entity, FPVector3 position, FP range)
     {
        if (!f.TryGet<Transform3D>(entity, out var fromTransform)) return false;
        return FPVector3.Distance(fromTransform.Position, position) <= range;
     }
}
