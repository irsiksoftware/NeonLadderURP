using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    SceneChangeController sceneChangeController;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Awake()
    {
        sceneChangeController = GetComponentInParent<SceneChangeController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            sceneChangeController.ChangeScene();
        }
    }
}
