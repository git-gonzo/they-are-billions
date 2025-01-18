using Quantum;
using UnityEngine;

public class BuyBuildingController : BuyButtonController
{
    [SerializeField] BuildingPlacementController _buildingPlacementController;
    public BuildingController buildingPrefab;
    public AssetRef<BuildingConfig> buildingConfig;

    public void BuyBuilding()
    {
        if(!CanBuy()) return;
        _buildingPlacementController.PlaceBuildingPreview(buildingPrefab, PlaceBuiling);
    }

    public void PlaceBuiling(Vector3 pos)
    {
        CommandPlaceBuilding command = new CommandPlaceBuilding()
        {
            PlayerEntityRef = HudController.LocalPlayerEntity,
            building = buildingConfig,
            position = pos.ToFPVector3()
        };
        QuantumRunner.Default.Game.SendCommand(command);
    }
}
