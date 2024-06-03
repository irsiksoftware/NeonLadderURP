using NeonLadder.Core;
using NeonLadder.Gameplay;
using NeonLadder.Mechanics.Collectibles;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePowerUpManager : MonoBehaviour
{
    [SerializeField]
    protected BuffTypes buffType;
    [SerializeField]
    protected int PowerUpDuration = 10;
    protected Player player{ get; set; }
    private List<Buff> ActiveBuffs = new List<Buff>();
    public Collider2D Collider { get; set; }
    [SerializeField]
    protected float RemainingDuration { get; set; }

    [SerializeField]
    private TimeTrackerUI _timeTrackerUI; // Private serialized field

    public TimeTrackerUI timeTrackerUI // Public property
    {
        get { return _timeTrackerUI; }
        set { _timeTrackerUI = value; }
    }

    public TextMesh Mesh { get; set; }

    [SerializeField]
    [Range(0.1f, 0.5f)]
    protected float Magnitude = 0.25f;

    protected virtual void Start()
    {

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        player = model.player;
        Mesh = GetComponent<TextMesh>();
        Collider = GetComponent<Collider2D>();
    }

    protected void ApplyBuff(Buff buff)
    {
        ActiveBuffs.Add(buff);
        if (buff.Type == BuffTypes.Permanent)
        {
            ApplyPermanentBuffEffect(buff);
        }
        else
        {
            StartCoroutine(HandleTemporaryBuff(buff));
        }
        MoveOutOfBounds();
    }

    /// <summary>
    /// We cannot "destroy" the powerup or else it cancels the effect, so we move the game object out of bounds until the timer is up
    /// </summary>
    private void MoveOutOfBounds()
    {
        Collider.enabled = false;
        Mesh.gameObject.transform.up = new Vector3(0, 0, 10); //Out of Bound
    }

    protected void ApplyPermanentBuffEffect(Buff buff)
    {

    }

    private IEnumerator HandleTemporaryBuff(Buff buff)
    {
        ApplyTemporaryBuffEffect(buff);

        // Store the start time of the buff
        float startTime = Time.time;

        while (Time.time - startTime < buff.Duration)
        {
            // Calculate the remaining duration
            RemainingDuration = buff.Duration - (Time.time - startTime);

            // Update the time tracker UI if needed
            UpdateTimeTrackerUI();

            yield return null;
        }

        RemainingDuration = 0;
        UpdateTimeTrackerUI();

        Collider.gameObject.SetActive(false);
        RemoveBuffEffect(buff);
        ActiveBuffs.Remove(buff);
    }

    private void UpdateTimeTrackerUI()
    {
        // Example: Update a time tracker UI element with the remaining duration
        if (timeTrackerUI != null)
        {
            timeTrackerUI.UpdateTimeRemaining(RemainingDuration);
        }
    }

    protected abstract void ApplyTemporaryBuffEffect(Buff buff);
    protected virtual void RemoveBuffEffect(Buff buff)
    {
        Collider.gameObject.SetActive(false);
    }
}