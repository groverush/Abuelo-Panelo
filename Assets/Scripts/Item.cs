using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Datos del �tem")]
    public float peso = 10f;        // Peso individual del �tem
    public string tipo = "Sugarcane"; // Tipo del �tem (por defecto sugarcane)

    // Puedes extender esto si m�s adelante necesitas l�gica especial
    // por tipo, rareza, durabilidad, etc.
}
