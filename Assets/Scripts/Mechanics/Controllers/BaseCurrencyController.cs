using TMPro;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class BaseCurrencyController : MonoBehaviour
    {
        protected Player player;
        protected TextMeshProUGUI currencyTextMeshPro;
        protected virtual void Awake()
        {
            player = transform.parent.transform.parent.GetComponentInChildren<Player>();
            currencyTextMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        }
        protected virtual void Update()
        {

        }
    }
}
