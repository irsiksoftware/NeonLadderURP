using UnityEngine;
// using DamageNumbersPro; // TODO: Re-enable when DamageNumbersPro package is installed
using NeonLadder.Common;

namespace NeonLadder.Mechanics.Controllers
{
    public class DamageNumberController : MonoBehaviour
    {
        public GameObject popupPrefab; // TODO: Change back to DamageNumber when package is installed
        [SerializeField]
        private float yOffset = Constants.UI.DamageNumbers.YOffset; // Private backing field with a default minimum value of 1
        public float YOffset
        {
            get => yOffset;
            set => yOffset = Mathf.Max(Constants.UI.DamageNumbers.YOffset, value); // Ensure the value is at least 1
        }
        private Transform target;

        private void Start()
        {
            //ugh
            YOffset = yOffset;

            target = transform;
        }

        public void SpawnPopup(float number)
        {
            // TODO: Re-implement when DamageNumbersPro package is installed
            /*
            Vector3 spawnPosition = target.position + new Vector3(0, YOffset, 0);
            DamageNumber newPopup = popupPrefab.Spawn(spawnPosition, number);
            newPopup.SetFollowedTarget(target);

            if (number > 5)
            {
                newPopup.SetScale(Constants.UI.DamageNumbers.CriticalHitScale);
                newPopup.SetColor(new Color(1, 0.2f, 0.2f));
            }
            else
            {
                newPopup.SetScale(Constants.UI.DamageNumbers.NormalHitScale);
                newPopup.SetColor(new Color(1, 0.7f, 0.5f));
            }
            */
            
            // Temporary fallback - just log the damage
            UnityEngine.Debug.Log($"Damage: {number} at {target.position}");
        }
    }
}
