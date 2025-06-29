using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Datos del Ítem")]
    public float peso = 10f;        // Peso individual del ítem
    public string tipo = "Sugarcane"; // Tipo del ítem (por defecto sugarcane)

    // Puedes extender esto si más adelante necesitas lógica especial
    // por tipo, rareza, durabilidad, etc.
}
