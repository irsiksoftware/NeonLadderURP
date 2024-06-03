using Assets.Scripts;
using NeonLadder.Core;
using NeonLadder.Models;
using UnityEngine;

public class Game : MonoBehaviour
{
    void Awake()
    {
        //AppLogger.Initialize();
        //AppLogger.Logger.Information("GameController Awake and logger initialized");
    }


    public static Game Instance { get; private set; }

    //This model field is public and can be therefore be modified in the 
    //inspector.
    //The reference actually comes from the InstanceRegister, and is shared
    //through the simulation and events. Unity will deserialize over this
    //shared reference when the scene loads, allowing the model to be
    //conveniently configured inside the inspector.
    public PlatformerModel model = Simulation.GetModel<PlatformerModel>();


    void OnEnable()
    {
        Instance = this;
        //Time.timeScale = 5f;
    }

    void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (Instance == this) Simulation.Tick();
    }

    public void NormalSpeed()
    {
        Time.timeScale = Constants.RegularTimeScale;
    }

    public void HyperSpeed()
    {
        Time.timeScale = Constants.SkipTimeScale;
    }

    public void LightSpeed()
    {
        Time.timeScale = Constants.LightSpeedScale;
    }

    public void TimeScaleMultiplier()
    {
        Time.timeScale = model.timeScaleMultiplier;
    }
}

