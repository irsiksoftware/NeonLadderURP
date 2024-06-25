using NeonLadder.Common;
using NeonLadder.Mechanics.Stats;
using UnityEngine;

public class HealthIncreaseStatManager : MonoBehaviour
{
    [SerializeField]
    public Health Health;
    [SerializeField]
    public bool AddToMax;
    [SerializeField]
    public int Amount;

    public TextMesh Mesh { get; set; }
    public Collider2D Collider { get; set; }


    protected virtual void Start()
    {
        Mesh = GetComponent<TextMesh>();
        Collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (AddToMax)
        {
            Health.max += Amount;
            Constants.MaxHealth += Amount;
        }
        else
        {
            Health.Increment(Amount);
        }
        MoveOutOfBounds();
    }

    private void MoveOutOfBounds()
    {
        Collider.enabled = false;
        Mesh.gameObject.transform.up = new Vector3(0, 0, 10); //Out of Bound
    }
}
