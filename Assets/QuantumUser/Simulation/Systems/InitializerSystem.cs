using Photon.Deterministic;
using Quantum.Physics2D;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class InitializerSystem : SystemSignalsOnly, ISignalOnEntityPrototypeMaterialized
    {
        public void OnEntityPrototypeMaterialized(Frame f, EntityRef entity, EntityPrototypeRef prototypeRef)
        {
            if (f.TryGet(entity, out ResourcesSpawnPointComponent resourceSpawnPoint)) 
            {
                var spawnPointPos = f.Get<Transform3D>(entity).Position;
                var resources = f.ResolveList(resourceSpawnPoint.Resources);
                var resourcesAmount = f.RNG->Next(resourceSpawnPoint.treeCountRange.X, resourceSpawnPoint.treeCountRange.Y);

                var rPos = new List<FPVector3>();

                for (int i = 0; i < resourcesAmount; i++)
                {
                    var tries = 0;
                    var isValid = false;
                    while (!isValid && tries < 10)
                    {
                        FPVector3 pos = GetRandomPosition(f, resourceSpawnPoint.size);
                        if (IsPositionValid(rPos, pos, resourceSpawnPoint.separation))
                        {
                            rPos.Add(pos);
                            var index = f.RNG->Next(0, resources.Count);
                            var r = f.Create(resources[index]);
                            var t = f.Unsafe.GetPointer<Transform3D>(r);
                            t->Position = spawnPointPos + pos;
                            t->LookAt(t->Position + GetRandomPosition(f, new FPVector2(1, 1)));
                            isValid = true;
                        }
                        tries++;
                    }
                }
            }
        }

        FPVector3 GetRandomPosition(Frame f, FPVector2 area)
        {
            var x = f.RNG->Next(-area.X, area.X);
            var y = f.RNG->Next(-area.Y, area.Y);
            var pos = new FPVector3();
            pos.X = x;
            pos.Z = y;
            return pos;
        }

        bool IsPositionValid(List<FPVector3> listPos, FPVector3 position, FP distance)
        {
            for (int i = 0; i < listPos.Count; i++)
            {
                if (FPVector3.Distance(listPos[i], position) < distance) return false;
            }
            return true;
        }
    }
}
