using Quantum;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class UnitsManager : MonoBehaviour
{
    UnitControllerBase unitSelected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (!hit.transform.TryGetComponent<UnitControllerBase>(out var unitControllerBase)) return;
                if(unitSelected != null)
                    unitSelected.SelectObject(false);
                unitSelected = unitControllerBase;
                unitSelected.SelectObject(true);
            }
        }
        else if (UnityEngine.Input.GetMouseButtonDown(1)) 
        {
            if (unitSelected != null) 
            {
                Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                { 
                    //If click on Floor
                    if(unitSelected.TryGetComponent<QuantumEntityView>(out var entityView)) 
                    {
                        CommandMoveUnit command = new CommandMoveUnit()
                        {
                            entity = entityView.EntityRef,
                            destination = hit.transform.position.ToFPVector3()
                        };
                        QuantumRunner.Default.Game.SendCommand(command);

                    }
                }
            }
        }


    }
}
