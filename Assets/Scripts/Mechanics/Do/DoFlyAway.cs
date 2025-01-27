using DG.Tweening;
using UnityEngine;

public class DoFlyRight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DOLocalMoveZ(3.61f, 30f);
        transform.DOScale(.0f, 30f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
