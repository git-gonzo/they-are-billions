using Quantum;
using UnityEngine;

public class GameUtils 
{

    public static bool IsEntityLocalPlayer(Frame f, EntityRef entity)
    {
        if (!f.TryGet(entity, out PlayerLink playerLink)) return false;
        return f.IsPlayerVerifiedOrLocal(playerLink.PlayerRef);
    }

    public static bool IsPointInside(Vector3 point, Vector3 from, Vector3 to)
    {
        float minX = Mathf.Min(from.x, to.x);
        float maxX = Mathf.Max(from.x, to.x);
        float minZ = Mathf.Min(from.z, to.z);
        float maxZ = Mathf.Max(from.z, to.z);

        return (point.x >= minX && point.x <= maxX) &&
               (point.z >= minZ && point.z <= maxZ);
    }
}