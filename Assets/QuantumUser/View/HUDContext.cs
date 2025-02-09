using UnityEngine;

namespace Quantum
{
    public class HUDContext : QuantumMonoBehaviour, IQuantumViewContext
    {
        public HudController hudController;
        public Transform lifebarsContainer;
    }
    
}
