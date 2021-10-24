using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorController : MonoBehaviour
{
    public enum State { NA, Seeking, Tracking, HandsUp, Firing, Countdown, Retracking, Deactivated }

    public string jointToTrack = "Heart";
    public string rightHandJoint = "Hand Right";
    public string leftHandJoint = "Hand Left";
    public string headJoint = "Head";
    public string rightThumbJoint = "Thumb Right";
    public string leftThumbJoint = "Thumb Left";
    public string rightHandTipJoint = "Hand Tip Right";
    public string leftHandTipJoint = "Hand Tip Left";
    public string hipJoint = "Hip";
    public bool allowFire = true;
    public bool canDisable = true;
    public float countdownAfter = 3;
    public float countdownTime = 5;
    public Vector3 kinectMotorOffset;
    public Vector2Int deactivatedAngles;
    public Vector2Int seekingAngles = new Vector2Int(90, 90);
    public PersonTracker tracker;
    public SerialCommunication serial;
    public State state;
    public Vector2 angleMultiplier = Vector2.one;

    float horizAngle, vertAngle;
    float leftHandY, rightHandY, headY, rightThumbY, leftThumbY, rightHandTipY, leftHandTipY, hipY;
    float trackingStartTimestamp;
    float countDownStartTimestamp;
    bool trackedThisFrame = false;
    bool on = true;
    bool firing;

    public event Action<State> OnStateChanged;

    private void Awake()
    {
        if (tracker != null)
        {
            tracker.OnTrackedJointUpdated += HandleTrackedJointUpdated;
        }
        StartCoroutine(SendAnglesCoroutine());
    }

    private void Start()
    {
        SetState(State.Seeking);
    }

    private void LateUpdate()
    {
        if (state == State.Deactivated)
        {
            if (!trackedThisFrame)
                SetState(State.Seeking);
        }
        else if (canDisable && rightHandY > hipY && leftHandY > hipY && rightThumbY > rightHandTipY && leftThumbY > leftHandTipY)
        {
            SetState(State.Deactivated);
        }
        else if (rightHandY > headY && leftHandY > headY)
        {
                SetState(State.HandsUp);
        }
        else if (trackedThisFrame)
        {
            if (state == State.Seeking)
            {
                SetState(State.Tracking);

            }
            else if (state == State.HandsUp)
            {
                SetState(State.Retracking);
            }
            else if ((state == State.Tracking || state == State.Retracking) && Time.time - trackingStartTimestamp > countdownAfter)
            {
                countDownStartTimestamp = Time.time;
                SetState(State.Countdown);
            }
            else if (state == State.Countdown && Time.time - countDownStartTimestamp > countdownTime)
                SetState(State.Firing);
        }
        else
            SetState(State.Seeking);
        rightHandY = 0;
        leftHandY = 0;
        headY = 0;
        leftThumbY = 0;
        rightThumbY = 0;
        rightHandTipY = 0;
        leftHandTipY = 0;
        hipY = 0;
        trackedThisFrame = false;
    }

    private void SetState(State newState)
    {
        if (state == newState)
            return;
        state = newState;
        OnStateChanged(newState);
    }

    private void HandleTrackedJointUpdated(string jointName, Vector3 position)
    {
        if (jointName == jointToTrack)
        {
            if (state != State.Tracking && state != State.Retracking)
                trackingStartTimestamp = Time.time;
            trackedThisFrame = true;
            horizAngle = Mathf.Atan((position.x + kinectMotorOffset.x) / (position.z + kinectMotorOffset.z)) * Mathf.Rad2Deg;
            vertAngle = Mathf.Atan((position.y + kinectMotorOffset.y) / (position.z + kinectMotorOffset.z)) * Mathf.Rad2Deg;
            horizAngle = Mathf.Clamp((horizAngle * angleMultiplier.x) + 90, 0, 180);
            vertAngle = Mathf.Clamp((vertAngle * angleMultiplier.y) + 90, 0, 180);
        }
        else if (jointName == rightHandJoint)
        {
            rightHandY = position.y;
        }
        else if (jointName == leftHandJoint)
        {
            leftHandY = position.y;
        }
        else if (jointName == headJoint)
        {
            headY = position.y;
        }
        else if (jointName == rightHandTipJoint)
        {
            rightHandTipY = position.y;
        }
        else if (jointName == leftHandTipJoint)
        {
            leftHandTipY = position.y;
        }
        else if (jointName == rightThumbJoint)
        {
            rightThumbY = position.y;
        }
        else if (jointName == leftThumbJoint)
        {
            leftThumbY = position.y;
        }
        else if (jointName == hipJoint)
        {
            hipY = position.y;
        }
        //Debug.Log("Joint \"" + jointName + "\" found. Horizontal Angle: " + horizAngle.ToString("0.00") + ". Vertical Angle: " + vertAngle.ToString("0.00"));
    }

    private void SendAngles(int horizontal, int vertical)
    {
        serial.Send(Convert.ToByte('H')); //Prime Arduino to accept horizontal angle next.
        byte hVal = Convert.ToByte(horizontal + 1);
        serial.Send(hVal);
        serial.Send(Convert.ToByte('V')); //Prime Arduino to accept horizontal angle next.
        byte vVal = Convert.ToByte(vertical + 1);
        serial.Send(vVal);
    }

    private void SendFire(bool fire)
    {
        if(firing != fire)
            serial.Send((allowFire && fire) ? Convert.ToByte('F') : Convert.ToByte('S'));
        firing = fire;
    }

    IEnumerator SendAnglesCoroutine()
    {
        while (on)
        {
            yield return new WaitForSecondsRealtime(.05f);
            switch (state)
            {
                case State.Tracking:
                    SendAngles(Mathf.RoundToInt(horizAngle), Mathf.RoundToInt(vertAngle));
                    break;
                case State.Firing:
                    SendAngles(Mathf.RoundToInt(horizAngle), Mathf.RoundToInt(vertAngle));
                    break;
                case State.Seeking:
                    SendAngles(seekingAngles.x, seekingAngles.y);
                    break;
                case State.HandsUp:
                    SendAngles(Mathf.RoundToInt(horizAngle), Mathf.RoundToInt(vertAngle));
                    break;
                case State.Countdown:
                    SendAngles(Mathf.RoundToInt(horizAngle), Mathf.RoundToInt(vertAngle));
                    break;
                case State.Retracking:
                    SendAngles(Mathf.RoundToInt(horizAngle), Mathf.RoundToInt(vertAngle));
                    break;
                case State.Deactivated:
                    SendAngles(deactivatedAngles.x, deactivatedAngles.y);
                    break;
            }
            SendFire(state == State.Firing);
            serial.Write();
        }
    }
}
