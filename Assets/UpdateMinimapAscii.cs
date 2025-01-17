using NeonLadder.Common;
using TMPro;
using UnityEngine;

public class UpdateMinimap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var map = GetComponent<TextMeshProUGUI>().text;

        if (!string.IsNullOrWhiteSpace(Constants.Minimap) && map != Constants.Minimap)
        {
            GetComponent<TextMeshProUGUI>().text = Constants.Minimap;
        }
    }
}

