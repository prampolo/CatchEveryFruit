using UnityEngine;
using Assets.SerialPortUtility.Scripts;
using System.Text;
using System;

public class Basket : MonoBehaviour
{

    [Header("Limiti movimento (coordinate mondo)")]
    public float xMin = -10f; // nuovo limite a sinistra
    public float xMax = 10f;  // esistente
    private float xCenter;    // posizione centrale

    [Header("Porta seriale")]
    public string serialPortName = "COM1";
    public int baudRate = 9600;

    private float maxExpectedDiff = 500f; // definisce l'estensione massima dello sbilanciamento

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
        float targetX = Mathf.Lerp(xMin, xMax, t);

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

        // Atteso formato: Timestamp; S1,S2,S3,S4 ; S5,S6,S7,S8
        string[] mainParts = raw.Split(';');
        if (mainParts.Length != 3)
        {
            Debug.LogWarning("[SERIAL] Formato inatteso, attesi 3 sezioni separate da ';'");
            return;
        }

        try
        {
            string[] rightSensors = mainParts[1].Split(',');
            string[] leftSensors = mainParts[2].Split(',');

            if (rightSensors.Length != 4 || leftSensors.Length != 4)
            {
                Debug.LogWarning("[SERIAL] Numero errato di sensori (attesi 4+4)");
                return;
            }

            float rightSum = 0f;
            float leftSum = 0f;

            for (int i = 0; i < 4; i++)
            {
                rightSum += int.Parse(rightSensors[i]);
                leftSum += int.Parse(leftSensors[i]);
            }

            float rightAvg = rightSum / 4f;
            float leftAvg = leftSum / 4f;

            float difference = rightAvg - leftAvg;

            // Clampa la differenza tra [-max, +max] per evitare movimenti estremi
            float clamped = Mathf.Clamp(difference, -maxExpectedDiff, maxExpectedDiff);

            // Normalizza in [0, 1] dove 0.5 è il centro
            float t = (clamped / (2 * maxExpectedDiff)) + 0.5f;

            // Converti in scala 0–1000 per compatibilità con MoveBasketWithSensor
            latestSensorValue = Mathf.RoundToInt(t * 1000f);

            Debug.Log($"[SERIAL] RightAvg={rightAvg:F2}, LeftAvg={leftAvg:F2}, Diff={difference:F2}, t={t:F2}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SERIAL] Errore parsing valori: {ex.Message}");
        }
    }



    void OnApplicationQuit()
    {
        serialFacade.Disconnect();
    }
}
