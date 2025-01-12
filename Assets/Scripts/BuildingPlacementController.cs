using UnityEngine;

public class BuildingPlacementController : MonoBehaviour
{
    //public GameObject buildingPrefab; // Prefab del edificio
    private BuildingController currentPreview; // Modelo en modo vista previa
    public LayerMask placementLayer; // Capas válidas para la colocación

    void Update()
    {
        if (currentPreview != null)
        {
            HandlePreviewPlacement();
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceBuilding();
            }
        }
    }

    public void PlaceBuildingPreview(BuildingController buildingPrefab)
    {
        if (currentPreview == null)
        {
            currentPreview = Instantiate(buildingPrefab);
            currentPreview.SetPreview(true);
            currentPreview.GetComponent<Collider>().enabled = false; // Desactivar colisiones
            SetPreviewMaterial(currentPreview.gameObject, true); // Hacer el modelo transparente
        }
    }

    void HandlePreviewPlacement()
    {
        Debug.Log("HandlePreviewPlacement");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            Debug.Log("moving preview");
            currentPreview.transform.position = hit.point; // Mover el modelo
        }
    }

    void TryPlaceBuilding()
    {
        if (IsPlacementValid())
        {
            SetPreviewMaterial(currentPreview.gameObject, false); // Quitar transparente
            currentPreview.SetPreview(false);


        }
    }

    bool IsPlacementValid()
    {
        // Agregar lógica para verificar si la ubicación es válida
        return true; // Ejemplo básico
    }

    void SetPreviewMaterial(GameObject obj, bool isPreview)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = isPreview ? new Color(1, 1, 1, 0.5f) : Color.white;
        }
    }
}
