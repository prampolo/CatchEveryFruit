using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    Vector3 inputPosition;
    bool touched;

    private void Update()
    {
        MoveBasket();
    }

    void MoveBasket()
    {
        Vector3 inputPosition = GetCursorPosition(Input.mousePosition);
        Vector3 targetPosition = transform.position;
        targetPosition.x = inputPosition.x;
        float step = 30 * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }

    Vector3 GetCursorPosition(Vector3 input)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(input.x, input.y, input.z));
    }

}