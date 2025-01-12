using Quantum;
using UnityEngine;

public class HudController : QuantumSceneViewComponent
{
    public static EntityRef LocalPlayerEntity;

    [SerializeField] public BuildingPopupController _buildingPopupController;

    public override void OnActivate(Frame frame)
    {
        base.OnActivate(frame);

        //Find local player
        FindLocalPlayer(frame);
    }

    public void ShowBuildingPopup(BuildingData building, int freeWorkers = 0, int workers = 0) 
    {
        _buildingPopupController.gameObject.SetActive(true);
        _buildingPopupController.Init(building, freeWorkers, workers);
    }

    private void FindLocalPlayer(Frame f)
    {
        if (LocalPlayerEntity != null)
        {
            var filter = f.Filter<PlayerLink>();
            while(filter.Next(out var entity, out var link)) 
            {
                if (f.IsPlayerVerifiedOrLocal(link.PlayerRef)) 
                {
                    LocalPlayerEntity = entity;
                    return;
                }
            }
        }
    }
}
