using NeonLadder.Mechanics.Enums;
using System;
using System.Collections;
using UnityEngine;

public class ChildObjectsAnimationSynchronization : MonoBehaviour
{
    public int ThrillerDanceAnimation = 9991;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DanceAfter(new TimeSpan(0, 0, 8), ThrillerDanceAnimation));
    }

    private IEnumerator DanceAfter(TimeSpan waitTime, int dance)
    {

        yield return new WaitForSeconds(Convert.ToInt64(waitTime.TotalSeconds));
        var animators = GetComponentsInChildren<Animator>();
        foreach (var animator in animators)
        {
            animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), dance);
        }
    }

    private void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
