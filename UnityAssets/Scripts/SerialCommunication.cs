using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class SerialCommunication : MonoBehaviour
{
    public string portName = "COM6";
    public int baudRate = 9600;
    public Parity parity = Parity.None;
    private SerialPort port;
    private List<byte> bytes = new List<byte>();

    public event Action<string> OnDataReceived;

    // Start is called before the first frame update
    void Start()
    {
        port = new SerialPort(portName, baudRate, parity);
        port.Open();
    }

    private void Update()
    {
        if (bytes.Count > 0)
        {
            port.Write(bytes.ToArray(), 0, bytes.Count);
            bytes.Clear();
        }
        while (port.BytesToRead > 0)
        {
            string indata = port.ReadLine();
            if (!String.IsNullOrEmpty(indata))
            {
                if (OnDataReceived != null)
                    OnDataReceived(indata + "\n");
            }
        }
    }


    public void Send(string val)
    {
        port.Write(val);
    }

    public void Send(byte val)
    {
        bytes.Add(val);
    }

    private void OnDestroy()
    {
        port.Close();
    }

}
