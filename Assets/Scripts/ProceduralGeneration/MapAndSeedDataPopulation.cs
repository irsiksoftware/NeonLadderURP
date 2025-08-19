using NeonLadder.Common;
using NeonLadder.Mechanics.Controllers;
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
        if (!string.IsNullOrWhiteSpace(Game.Instance.ProceduralMap.Seed) && seed.text != Game.Instance.ProceduralMap.Seed)
        {
            seed.text = Game.Instance.ProceduralMap.Seed;
        }
    }
}
