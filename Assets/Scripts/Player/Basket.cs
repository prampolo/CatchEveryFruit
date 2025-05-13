using UnityEngine;
using Assets.SerialPortUtility.Scripts;
using System.Text;
using System;

public class Basket : MonoBehaviour
{

    [Header("Limiti movimento (coordinate mondo)")]
    public float xMax = 10f; // Limite massimo a destra
    private float xCenter;  // Posizione centrale del cestino (registrata a runtime)

    [Header("Porta seriale")]
    public string serialPortName = "COM1";
    public int baudRate = 9600;

    private SerialCommunicationFacade serialFacade;
    private int latestSensorValue = 0;

    void Start()
    {
        xCenter = transform.position.x;

        serialFacade = new SerialCommunicationFacade();
        serialFacade.Connect(baudRate, serialPortName);
        serialFacade.OnSerialMessageReceived += OnSerialData;
    }

    void Update()
    {
        MoveBasketWithSensor();
    }

    void MoveBasketWithSensor()
    {
        int clamped = Mathf.Clamp(latestSensorValue, 0, 1000);
        float t = clamped / 1000f;
        float targetX = Mathf.Lerp(xCenter, xMax, t);

        Debug.Log($"[POSITION] t={t:F2}, targetX={targetX:F2}");

        Vector3 currentPosition = transform.position;
        currentPosition.x = targetX;
        float speed = 30f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentPosition, speed);
    }

    void OnSerialData(byte[] data)
    {
        string raw = Encoding.ASCII.GetString(data).Trim();
        Debug.Log($"[SERIAL] Raw received string: '{raw}'");

        string[] parts = raw.Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            if (int.TryParse(part, out int value))
            {
                latestSensorValue = value;
                Debug.Log($"[SERIAL] Parsed sensor value: {latestSensorValue}");
                return;
            }
        }

        Debug.LogWarning($"[SERIAL] Could not extract numeric value from: '{raw}'");
    }


    void OnApplicationQuit()
    {
        serialFacade.Disconnect();
    }
}
