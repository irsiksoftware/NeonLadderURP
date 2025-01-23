using NeonLadder.Common;
using TMPro;
using UnityEngine;

public class MapAndSeedDataPopulation : MonoBehaviour
{
    TextMeshProUGUI map;
    TMP_InputField seed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        map = GetComponentInChildren<TextMeshProUGUI>();
        seed = GetComponentInChildren<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrWhiteSpace(Constants.Seed) && seed.text != Constants.Seed)
        {
            seed.text = Constants.Seed;
        }

        if (!string.IsNullOrWhiteSpace(Constants.Minimap) && map.text != Constants.Minimap)
        {
            map.text = Constants.Minimap;
        }
    }
}
