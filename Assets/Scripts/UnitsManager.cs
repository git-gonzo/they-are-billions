using UnityEngine;
using System.Collections.Generic;
using Quantum;

public class UnitsManager : MonoBehaviour
{
    private List<UnitControllerBase> selectedUnits = new List<UnitControllerBase>();

    private Vector2 selectionStart;
    private Vector2 selectionEnd;
    private bool isSelecting = false;

    void Update()
    {
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            selectionStart = UnityEngine.Input.mousePosition;
            isSelecting = true;
        }
        else if (UnityEngine.Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            SelectUnitsInRectangle();
        }

        else if (UnityEngine.Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Intentar obtener la entidad Quantum del objeto impactado
                //REVISAR
                if (hit.transform.TryGetComponent<QuantumEntityView>(out var entityView))
                {
                    var frame = QuantumRunner.Default.Game.Frames.Predicted;
                    var entity = entityView.EntityRef;

                    if (!frame.TryGet(entity, out UnitComponent unitComponent)) return;

                    var unitAsset = frame.FindAsset(unitComponent.unitAsset);

                    if (unitAsset != null && unitAsset.unitType.Equals(UnitType.Enemy))
                    {
                        AttackEnemy(entityView.EntityRef);
                    }
                    else
                    {
                        MoveUnits(hit.point);
                    }
                }
                else
                {
                    MoveUnits(hit.point);
                }
            }
        }
    }

    private void AttackEnemy(EntityRef enemyEntity)
    {
        foreach (var unit in selectedUnits)
        {
            if (unit.TryGetComponent<QuantumEntityView>(out var entityView))
            {
                CommandAttackUnit command = new CommandAttackUnit()
                {
                    attackerEntity = entityView.EntityRef,
                    enemyEntity = enemyEntity
                };
                QuantumRunner.Default.Game.SendCommand(command);
            }
        }
    }

    private void MoveUnits(Vector3 destination)
    {
        foreach (var unit in selectedUnits)
        {
            if (unit.TryGetComponent<QuantumEntityView>(out var entityView))
            {
                CommandMoveUnit command = new CommandMoveUnit()
                {
                    entity = entityView.EntityRef,
                    destination = destination.ToFPVector3()
                };
                QuantumRunner.Default.Game.SendCommand(command);
            }
        }
    }

    private void OnGUI()
    {
        if (isSelecting)
        {
            selectionEnd = UnityEngine.Input.mousePosition;
            Rect rect = GetScreenRect(selectionStart, selectionEnd);
            DrawSelectionBox(rect);
        }
    }

    private void SelectUnitsInRectangle()
    {
        Rect selectionRect = GetScreenRect(selectionStart, selectionEnd);

        if (!(UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift)))
        {
            DeselectAllUnits();
        }

        foreach (var unit in FindObjectsByType<UnitControllerBase>(FindObjectsSortMode.None))
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
            screenPos.y = Screen.height - screenPos.y;

            if (selectionRect.Contains(screenPos, true) && !selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                unit.SelectObject(true);
            }
        }
    }

    private void DeselectAllUnits()
    {
        foreach (var unit in selectedUnits)
        {
            if(unit != null)
            {
                unit.SelectObject(false);
            }
        }
        selectedUnits.Clear();
    }

    private Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        float x = Mathf.Min(start.x, end.x);
        float y = Mathf.Min(Screen.height - start.y, Screen.height - end.y);
        float width = Mathf.Abs(start.x - end.x);
        float height = Mathf.Abs(start.y - end.y);

        return new Rect(x, y, width, height);
    }

    private void DrawSelectionBox(Rect rect)
    {
        Color originalColor = GUI.color;
        GUI.color = new Color(0, 1, 1, 0.3f); 
        GUI.Box(rect, "");
        GUI.color = originalColor;
    }
}
