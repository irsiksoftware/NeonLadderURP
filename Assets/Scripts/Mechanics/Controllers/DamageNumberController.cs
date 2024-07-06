using UnityEngine;
using DamageNumbersPro; 

namespace NeonLadder.Mechanics.Controllers
{
    public class DamageNumberController : MonoBehaviour
    {
        public DamageNumber popupPrefab;
        [SerializeField]
        private float yOffset = 1f; // Private backing field with a default minimum value of 1
        public float YOffset
        {
            get => yOffset;
            set => yOffset = Mathf.Max(1f, value); // Ensure the value is at least 1
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
            Vector3 spawnPosition = target.position + new Vector3(0, YOffset, 0);
            DamageNumber newPopup = popupPrefab.Spawn(spawnPosition, number);
            newPopup.SetFollowedTarget(target);

            if (number > 5)
            {
                newPopup.SetScale(1.5f);
                newPopup.SetColor(new Color(1, 0.2f, 0.2f));
            }
            else
            {
                newPopup.SetScale(1);
                newPopup.SetColor(new Color(1, 0.7f, 0.5f));
            }
        }
    }
}
