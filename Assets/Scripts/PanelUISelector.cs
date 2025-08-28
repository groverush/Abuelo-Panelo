using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelUISelector : MonoBehaviour
{
    // Asigna el botón inicial de este panel en el Inspector
    [SerializeField] private Button firstSelectedButton;

    // Este método se llama automáticamente cada vez que el GameObject se activa
    private void OnEnable()
    {
        // Asegúrate de que el EventSystem y el botón existan
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            // Establece el botón de este panel como el objeto seleccionado para la navegación
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }
}