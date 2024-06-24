using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Models;
using UnityEngine;

public class BaseCurrencyController : MonoBehaviour
{
    protected Player player;
    public PlatformerModel model;
    protected virtual void Awake()
    {
        model = Simulation.GetModel<PlatformerModel>();
        player = model.Player;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //player = model.Player;
    }
}
