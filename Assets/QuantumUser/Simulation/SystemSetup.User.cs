namespace Quantum
{
    using System;
    using System.Collections.Generic;

    public static partial class DeterministicSystemSetup
    {
        static partial void AddSystemsUser(ICollection<SystemBase> systems, RuntimeConfig gameConfig, SimulationConfig simulationConfig, SystemsConfig systemsConfig)
        {
            // The system collection is already filled with systems coming from the SystemsConfig. 
            // Add or remove systems to the collection: systems.Add(new SystemFoo());
            systems.Add(new InitializerSystem());
            systems.Add(new UnitsSystem());
            systems.Add(new AnimalsSystem());
            systems.Add(new PlayerSystem());
            systems.Add(new CommandsSystem());
            systems.Add(new BuildingsSystem());
            systems.Add(new EconomySystem());
            systems.Add(new HealthSystem());
    }
  }
}