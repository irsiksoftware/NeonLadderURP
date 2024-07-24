using NeonLadder.Managers;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using NeonLadder.Mechanics.Enums;
using Cinemachine;

public class SceneEndController : MonoBehaviour
{
    private float celebrationDuration = 6.3f; // Duration of the celebration animation

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ManagerController.Instance.steamManager.UnlockAchievement(nameof(Achievements.DEMO_LEVEL_COMPLETE));
            other.GetComponentInChildren<Player>().Actions.playerActionMap.Disable();
            var meleeweapons = other.GetComponentsInChildren<MeleeWeapons>();
            var firearms = other.GetComponentsInChildren<Firearms>();
            foreach (var melee in meleeweapons)
            {
                melee.gameObject.SetActive(false);
            }
            foreach (var firearm in firearms)
            {
                firearm.gameObject.SetActive(false);
            }
            other.GetComponent<Transform>().rotation = Quaternion.Euler(0, 180, 0);
            StartCoroutine(PlayExcitedAnimationAndLoadCredits(other.GetComponent<Animator>()));
            StartCoroutine(ZoomInOnPlayer());
        }
    }

    private IEnumerator PlayExcitedAnimationAndLoadCredits(Animator playerAnimator)
    {
        playerAnimator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), 9042); // Excited
        yield return new WaitForSeconds(celebrationDuration); // Wait for the celebration duration
        SceneManager.LoadScene("Credits");
    }

    private IEnumerator ZoomInOnPlayer()
    {
        CinemachineVirtualCamera playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<CinemachineVirtualCamera>();
        CinemachineFramingTransposer transposer = playerCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        float initialDistance = transposer.m_CameraDistance;
        float targetDistance = initialDistance - 5f; // Adjust this value for the desired zoom amount
        float elapsed = 0f;

        while (elapsed < celebrationDuration)
        {
            elapsed += Time.deltaTime;
            float currentDistance = Mathf.Lerp(initialDistance, targetDistance, elapsed / celebrationDuration);
            transposer.m_CameraDistance = currentDistance;
            yield return null;
        }
    }
}
