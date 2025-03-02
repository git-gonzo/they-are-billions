using UnityEngine;
using System.Collections.Generic;
using Quantum;
using System;
using Quantum.Physics2D;

public class UnitsManager : QuantumSceneViewComponent
{
    [SerializeField] private DestinationMark _destinationMark;
    [SerializeField] LineRenderer _selectionLine;

    private List<UnitControllerBase> _selectedUnits = new List<UnitControllerBase>();

    private Vector3 pointA;
    private Vector3 pointB;

    private Vector2 selectionStart;
    private Vector2 selectionEnd;
    private bool isSelecting = false;
    private List<UnitControllerBase> _playerUnits = new();
    private UnitControllerBase _overUnit;
    private DestinationMark _currentMark;


    public override void OnActivate(Frame f)
    {
        base.OnActivate(f);
        QuantumEvent.Subscribe<EventOnUnitCreated>(this, OnUnitCreated);
    }

    private void OnUnitCreated(EventOnUnitCreated e)
    {
        var f = QuantumRunner.Default.Game.Frames.Predicted;
        if (!GameUtils.IsEntityLocalPlayer(f, e.playerEntity)) return;

        var unitView = Updater.GetView(e.unitEntity);
        var unitController = unitView.gameObject.GetComponent<UnitControllerBase>();

        if (unitController == null)
        {
            Debug.LogWarning("Looks like there is a unit entity without unitController Zetun");
            return;
        }
        _playerUnits.Add(unitController);
    }

    public override void OnUpdateView()
    {
        base.OnUpdateView();
    }

    void Update()
    {
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            pointA = GetPointFromInput();
            isSelecting = true;
        }
        else if (UnityEngine.Input.GetMouseButtonUp(0))
        {
            isSelecting = false;       
        }

        else if (UnityEngine.Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0)
        {
            CleanLists();
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
        if (isSelecting) 
        {
            //Draw line
            CleanLists();
            DrawLine();
            SelectUnits();
        }
        else
        {
            CheckOverUnits();
            _selectionLine.enabled = false;
        }
    }

    private Vector3 GetPointFromInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
        RaycastHit hit;
        int layerMask = UnityEngine.LayerMask.GetMask("Floor");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private RaycastHit GetHitFromInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, Mathf.Infinity);
        return hit;
    }
    private UnitControllerBase GetUnitFromInput()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, Mathf.Infinity);
        if (hit.transform == null) return null;
        if (!hit.transform.TryGetComponent<QuantumEntityView>(out var entityView)) return null;

        var unit = _playerUnits.Find(u => u.EntityRef == entityView.EntityRef);
        return unit;
    }

    private void CheckOverUnits() 
    {
        var unit = GetUnitFromInput();

        //if(unit == null && _overUnit == null) return;

        if (unit == null && _overUnit != null)
        {
            _overUnit = null;
            HideAllLifeBars();
            return;
        }

        if(_overUnit == unit) return;

        _overUnit = unit;
        HideAllLifeBars(unit);
        unit.ShowLifeBar(true);       
    }

    private void HideAllLifeBars(UnitControllerBase skipUnit = null) 
    {
        foreach (var unit in _playerUnits)
        {
            if(unit == skipUnit) continue;
            unit.ShowLifeBar(false);
        }
    }

    private void DrawLine()
    {
        pointB = GetPointFromInput();
        _selectionLine.enabled = true;
        _selectionLine.positionCount = 5;
        Vector3 pointC = pointA;
        Vector3 pointD = pointA;
        pointC.x = pointB.x;
        pointD.z = pointB.z;
        _selectionLine.SetPosition(0, pointA);
        _selectionLine.SetPosition(1, pointC);
        _selectionLine.SetPosition(2, pointB);
        _selectionLine.SetPosition(3, pointD);
        _selectionLine.SetPosition(4, pointA);
    }

    private void SelectUnits() 
    {
        if (!(UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift)))
        {
            DeselectAllUnits();
        }
        
        foreach (var unit in _playerUnits)
        {
            var isInside = GameUtils.IsPointInside(unit.transform.position, pointA, pointB);
            if(isInside) _selectedUnits.Add(unit);
            unit.SelectObject(_selectedUnits.Contains(unit));
        }
    }

    private void AttackEnemy(EntityRef enemyEntity)
    {
        foreach (var unit in _selectedUnits)
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

    public void CleanLists() 
    {
        for (int i = _playerUnits.Count - 1; i >= 0; i--)
        {
            if (_playerUnits[i] == null) _playerUnits.RemoveAt(i);
        }
        for (int i = _selectedUnits.Count - 1; i >= 0; i--)
        {
            if (_selectedUnits[i] == null) _selectedUnits.RemoveAt(i);
        }
    }

    private void MoveUnits(Vector3 destination)
    {
        foreach (var unit in _selectedUnits)
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
        var mark = Instantiate(_destinationMark);
        var position = destination + Vector3.up * 0.2f;
        mark.transform.position = position;
        mark.Init();
        if(_currentMark != null) 
        {
            Destroy(_currentMark.gameObject);
        }
        _currentMark = mark;
    }

    private void DeselectAllUnits()
    {
        foreach (var unit in _selectedUnits)
        {
            if (unit != null)
            {
                unit.SelectObject(false);
            }
        }
        _selectedUnits.Clear();
    }
}
