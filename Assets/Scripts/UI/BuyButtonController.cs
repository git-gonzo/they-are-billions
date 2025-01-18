using Quantum;
using UnityEngine;

public class BuyButtonController : MonoBehaviour
{
    [SerializeField] protected ResourceAmount _cost; //TODO: Set this with some logic and config

    public void Init(ResourceAmount cost)
    {
        _cost = cost;
    }

    public void BuyWorker() 
    {
        if(CanBuy())
        {
            CommandBuyUnit command = new CommandBuyUnit()
            {
                PlayerEntityRef = HudController.LocalPlayerEntity,
                resourceAmount = _cost
            };
            QuantumRunner.Default.Game.SendCommand(command);
        }
        else
        {
            Debug.Log("Not enough " + _cost.Resource);
        }
    }

    protected bool CanBuy() 
    {
        var f = QuantumRunner.Default.Game.Frames.Predicted;
        var economy = f.Get<PlayerEconomyComponent>(HudController.LocalPlayerEntity);
        var resources = f.ResolveList(economy.resources);
        foreach ( var r in resources )
        {
            if(r.Resource == _cost.Resource)
            {
                //Debug.Log("Trying to buy something that cost " + _cost.Amount + " " + _cost.Resource + " and player has " + r.Amount);
                return r.Amount >= _cost.Amount;
            }
        }
        return false;
    }
}
