using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Plane Settings")]
    public GameObject plane; // Plane hijo asignado desde el Inspector
    public GameObject sugarcanePrefab; // Prefab de la caña

    [Header("Grid Settings")]
    public float spacing = 1f; // Espacio entre cañas

    private List<GameObject> currentSugarcanes = new List<GameObject>();

    void Start()
    {
        SpawnSugarcanes();
    }

    void Update()
    {
        // Verifica si ya no quedan cañas y respawnea
        if (currentSugarcanes.Count == 0)
        {
            SpawnSugarcanes();
        }
    }

    void SpawnSugarcanes()
    {
        // Limpia lista previa
        foreach (GameObject cane in currentSugarcanes)
        {
            Destroy(cane);
        }
        currentSugarcanes.Clear();

        Vector3 planeScale = plane.transform.localScale;
        Vector3 planePos = plane.transform.position;

        float planeWidth = planeScale.x * 10f;
        float planeHeight = planeScale.z * 10f;

        // Calcula cantidad de filas y columnas en base al spacing
        int rows = Mathf.FloorToInt(planeWidth / spacing);
        int columns = Mathf.FloorToInt(planeHeight / spacing);

        // Calcula inicio para centrar el grid en el plane
        float startX = planePos.x - (planeWidth / 2f) + (spacing / 2f);
        float startZ = planePos.z - (planeHeight / 2f) + (spacing / 2f);


        float totalGridWidth = rows * spacing;
        float totalGridHeight = columns * spacing;

        // Calcula el punto inicial como el centro del plane
        Vector3 planeCenter = plane.transform.position;

        // Calcula el offset desde el centro hacia la esquina inicial (izquierda-atrás)
        Vector3 cornerOffset = plane.transform.rotation * new Vector3(-totalGridWidth / 2f + spacing / 2f, 0f, -totalGridHeight / 2f + spacing / 2f);

        // Calcula la posición inicial real
        Vector3 startPoint = planeCenter + cornerOffset;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 gridOffset = new Vector3(i * spacing, 0f, j * spacing);

                // Rota ese offset
                Vector3 rotatedOffset = plane.transform.rotation * gridOffset;

                // Posición final
                Vector3 spawnPos = startPoint + rotatedOffset;
                GameObject cane = Instantiate(sugarcanePrefab, spawnPos, Quaternion.identity);
                currentSugarcanes.Add(cane);
            }
        }
    }

    // Llama este método cuando destruyas o recojas una caña
    public void RemoveSugarcane(GameObject cane)
    {
        currentSugarcanes.Remove(cane);
    }
}