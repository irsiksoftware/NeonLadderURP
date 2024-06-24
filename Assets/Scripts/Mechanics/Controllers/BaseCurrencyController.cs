using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Models;
using TMPro;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class BaseCurrencyController : MonoBehaviour
    {
        protected Player player;
        protected PlatformerModel model;
        protected TextMeshProUGUI currencyTextMeshPro;
        protected virtual void Awake()
        {
            model = Simulation.GetModel<PlatformerModel>();
            player = model.Player;
            currencyTextMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        }
        protected virtual void Update()
        {

        }
    }
}
