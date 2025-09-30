using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchManager : MonoBehaviour
{
    // Variables estáticas para que el PlayerController pueda leerlas
    public static Vector2 InputMoveValue;
    public static Vector2 InputLookValue;

    // Asigna tus áreas de botones en el Inspector
    public RectTransform holdButtonArea;
    public RectTransform cutButtonArea;

    // Referencia al PlayerController para las acciones de los botones
    public PlayerController playerController;

    private int leftTouchFingerId = -1;
    private int rightTouchFingerId = -1;

    void Update()
    {
        // Reinicia los valores si no hay toques
        InputMoveValue = Vector2.zero;
        InputLookValue = Vector2.zero;

        if (Input.touchCount == 0)
        {
            leftTouchFingerId = -1;
            rightTouchFingerId = -1;
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            
            if (touch.position.x < Screen.width * 0.25f)
            {
                ManejarMovimiento(touch);
            }
            else
            {
                ManejarRotacionYAcciones(touch);
            }
        }
    }

    private void ManejarMovimiento(Touch touch)
    {
        if (touch.phase == UnityEngine.TouchPhase.Began)
        {
            leftTouchFingerId = touch.fingerId;
        }
        else if (touch.fingerId == leftTouchFingerId && touch.phase != UnityEngine.TouchPhase.Ended && touch.phase != UnityEngine.TouchPhase.Canceled)
        {
            // Guarda el valor en la variable estática
            InputMoveValue = touch.deltaPosition;
        }
    }

    private void ManejarRotacionYAcciones(Touch touch)
    {
        // Guarda el valor en la variable estática
        InputLookValue = touch.deltaPosition;

        // Lógica de las acciones
        bool isOverHoldButton = RectTransformUtility.RectangleContainsScreenPoint(holdButtonArea, touch.position);
        bool isOverCutButton = RectTransformUtility.RectangleContainsScreenPoint(cutButtonArea, touch.position);

        if (touch.phase == UnityEngine.TouchPhase.Began)
        {
            if (isOverHoldButton)
            {
                playerController.OnHoldButtonPressed();
            }
            else if (isOverCutButton)
            {
                playerController.OnCutButtonPressed();
            }
        }
        else if (touch.phase == UnityEngine.TouchPhase.Ended)
        {
            if (isOverHoldButton)
            {
                playerController.OnHoldButtonReleased();
            }
            else if (isOverCutButton)
            {
                playerController.OnCutButtonReleased();
            }
        }
    }
}