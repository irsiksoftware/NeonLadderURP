using UnityEngine;
using System.Collections;

public class DashAfterImage : MonoBehaviour
{
    public GameObject afterImagePrefab; // Assign the prefab of your character with the AfterImageMaterial
    public float afterImageDuration = 0.5f;
    public float spawnInterval = 0.1f;

    private bool isDashing = false;

    void Start()
    {
        StartCoroutine(SpawnAfterImage());
    }

    IEnumerator SpawnAfterImage()
    {
        while (true)
        {
            if (isDashing)
            {
                GameObject afterImage = Instantiate(afterImagePrefab, transform.position, transform.rotation);
                afterImage.transform.localScale = transform.localScale;

                //// Copy the skinned mesh renderer's bone transforms
                //SkinnedMeshRenderer[] smrOrig = GetComponentsInChildren<SkinnedMeshRenderer>();
                //SkinnedMeshRenderer[] smrCopy = afterImage.GetComponentsInChildren<SkinnedMeshRenderer>();

                //for (int i = 0; i < smrOrig.Length; i++)
                //{
                //    smrCopy[i].bones = smrOrig[i].bones;
                //    smrCopy[i].rootBone = smrOrig[i].rootBone;
                //}

                Destroy(afterImage, afterImageDuration);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void StartDash()
    {
        isDashing = true;
    }

    public void StopDash()
    {
        isDashing = false;
    }
}
