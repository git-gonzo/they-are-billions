namespace Quantum
{
    using Photon.Deterministic;
    using Photon.Deterministic.Protocol;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PlayerSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerConnected
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            RuntimePlayer playerData = f.GetPlayerData(player);
            EntityRef playerCharacter = f.Create(playerData.PlayerAvatar);

            // Cache EntityRef if is local player
            if (f.IsPlayerVerifiedOrLocal(player)) 
            {
                f.Events.OnLocalPlayerCreated(playerCharacter);
            }

            // ADD ECONOMY
            f.Add<PlayerEconomyComponent>(playerCharacter);

            // ADD WORKERS
            var config = f.SimulationConfig.GameConfig;
            for (int i = 0; i < config.initialWorkers; i++) f.Signals.CreateUnit(playerCharacter, UnitType.Farmer);

            // ADD TROOPS
            for (int i = 0; i < config.initialTroops; i++) f.Signals.CreateUnit(playerCharacter, UnitType.Troop);

            //Assign player to existing buildings (temp) will be replaced when buildings instanciated for the player
            var filter = f.Filter<ResourceCollectorComponent>();
            while (filter.NextUnsafe(out EntityRef entity, out var comp))
            {
                comp->playerEntity = playerCharacter;
            }

            // Store it's PlayerRef so we can later use it for input polling
            var playerLink = f.Unsafe.GetPointer<PlayerLink>(playerCharacter);
            playerLink->PlayerRef = player;
        }

        public void OnPlayerConnected(Frame f, PlayerRef player)
        {
            Debug.Log("Player Connected");
        }
    }

    public static class SpawnPointHelper 
    {
        public unsafe static FPVector3 GetSpawnPoint<T>(Frame f) where T : unmanaged, IComponent
        {
            //Todo: Could be optimized, trying not to call it multimple times if there is no need
            List<FPVector3> spawnPoints = new List<FPVector3>();
            var allSpawnPoints = f.Unsafe.GetComponentBlockIterator<T>();
            //allSpawnPoints.
            foreach (var (spawnPointEntity, spawnPointComponent) in allSpawnPoints)
            {
                spawnPoints.Add(f.Get<Transform3D>(spawnPointEntity).Position);
                //Debug.Log("Add spawnpoint to list");
            }
            //Debug.Log("total spawnpoints " + spawnPoints.Count);
            int i = f.RNG->Next(0, spawnPoints.Count);

            return spawnPoints[i];
        }
    }
}
