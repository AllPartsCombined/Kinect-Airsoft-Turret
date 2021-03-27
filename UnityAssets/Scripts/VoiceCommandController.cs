using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceCommandController : MonoBehaviour
{
    public MotorController motorController;
    public AudioSource source;
    public AudioClip trackingClip, countdownClip, handsUpClip, retrackingClip, deactivateClip;

    private void Awake()
    {
        if (motorController == null)
            motorController = GetComponent<MotorController>();
        if (source == null)
            source = GetComponent<AudioSource>();
        motorController.OnStateChanged += HandleStateChange;
    }

    private void HandleStateChange(MotorController.State state)
    {
        switch (state)
        {
            case MotorController.State.Seeking:
                break;
            case MotorController.State.Tracking:
                PlayCommand(trackingClip);
                break;
            case MotorController.State.HandsUp:
                PlayCommand(handsUpClip);
                break;
            case MotorController.State.Firing:
                break;
            case MotorController.State.Countdown:
                PlayCommand(countdownClip);
                break;
            case MotorController.State.Retracking:
                PlayCommand(retrackingClip);
                break;
            case MotorController.State.Deactivated:
                PlayCommand(deactivateClip);
                break;
        }
    }

    private void PlayCommand(AudioClip clip)
    {
        source.Stop();
        source.PlayOneShot(clip);
    }
}