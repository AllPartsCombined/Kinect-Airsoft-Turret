using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateUI : MonoBehaviour
{
    public MotorController motorController;
    public Text text;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<Text>();
        motorController.OnStateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(MotorController.State state)
    {
        text.text = state.ToString();
    }
}
