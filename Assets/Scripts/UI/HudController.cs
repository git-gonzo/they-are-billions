using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] public BuildingPopupController _buildingPopupController;

    public void ShowBuildingPopup(BuildingData building, int freeWorkers = 0, int workers = 0) 
    {
        _buildingPopupController.gameObject.SetActive(true);
        _buildingPopupController.Init(building, freeWorkers, workers);
    }
}
