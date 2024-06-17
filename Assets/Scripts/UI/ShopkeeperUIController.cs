using UnityEngine;

namespace Neonladder.UI
{
    public class ShopkeeperUIController : MonoBehaviour
    {
        public GameObject[] panels;

        public void SetActivePanel(int index)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                var active = i == index;
                var g = panels[i];
                if (g.activeSelf != active) g.SetActive(active);
            }
        }

        void OnEnable()
        {
            SetActivePanel(0);
        }

        public void CloseShop()
        {
            gameObject.SetActive(false);
        }
    }
}
