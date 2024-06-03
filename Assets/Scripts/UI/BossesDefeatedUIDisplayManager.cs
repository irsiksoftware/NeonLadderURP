using Assets.Scripts;
using TMPro;
using UnityEngine;

public class BossesDefeatedUIDisplayManager : MonoBehaviour
{
    TextMeshProUGUI tmp;
    // Start is called before the first frame update
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Constants.DefeatedBosses.Count > 0)
        {
            tmp.text = "Sins Defeated: " + string.Join(",", Constants.DefeatedBosses);
        }
        else
        {
            tmp.text = "Sins Defeated: ???";
        }
    }
}
