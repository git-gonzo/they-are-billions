using Quantum;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class BuildingPopupController : PopupControllerBase
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtSubtitle;
    [SerializeField] private TextMeshProUGUI txtAmount;
    [SerializeField] private Button btnAdd;
    [SerializeField] private Button btnRemove;

    BuildingData _buildingData;
    int _freeWorkers;
    int _workers;
   

    protected override void Start()
    {
        base.Start();
        btnAdd.onClick.AddListener(AddWorker);
        btnRemove.onClick.AddListener(RemoveWorker);
    }
    public void Init(BuildingData building, int freeWorkers = 0, int workers = 0)
    {
        _freeWorkers = freeWorkers;
        _workers = workers;
        _buildingData = building;
        txtTitle.text = building.buildingName;
        txtSubtitle.text = building.buildingDescrip;
        icon.sprite = building.icon;
        UpdateButtons();
        ShowAnim();
    }

    private void AddWorker()
    {
        AddWorkers(1);
    }
    private void RemoveWorker()
    {
        AddWorkers(-1);
    }

    private void AddWorkers(int amount)
    {
        _workers += amount;
        _freeWorkers -= amount;
        CommandAddWorker command = new CommandAddWorker()
        {
            buildingEntityRef = _buildingData.Entity,
            amount = amount
        };
        QuantumRunner.Default.Game.SendCommand(command);
        UpdateButtons();
    }

    private void UpdateButtons() 
    {
        Debug.Log("Update Buttons, freeWorkers = " + _freeWorkers);
        btnRemove.interactable = _workers > 0;
        btnAdd.interactable = _workers < _buildingData.maxWorkers && _freeWorkers > 0;
        txtAmount.text = $"{_workers}/{_buildingData.maxWorkers}";
    }
}
