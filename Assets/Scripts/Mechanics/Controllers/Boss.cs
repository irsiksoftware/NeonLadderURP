using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class Boss : Enemy
    {
        public GameObject transformation;

        protected override void Start()
        {
            if (BossTransformations.bossTransformations.ContainsKey(gameObject.transform.parent.name))
            {
                var transformationName = BossTransformations.bossTransformations[gameObject.transform.parent.name];
                transformation = GameObject.Find(transformationName);

                if (transformation != null)
                {
                    transformation.SetActive(false);
                }
                else
                {
                    Debug.LogError($"Transformation game object '{transformationName}' not found.");
                }
            }
            base.Start();
        }
    }
}
