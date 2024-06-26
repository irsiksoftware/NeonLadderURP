using NeonLadder.Mechanics.Enums;
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
            var x = GameObject.FindGameObjectWithTag(Tags.Player.ToString());

            player = GameObject.FindGameObjectWithTag(Tags.Player.ToString()).GetComponent<Player>();
            currencyTextMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        }
        protected virtual void Update()
        {

        }
    }
}
