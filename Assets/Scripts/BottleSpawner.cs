using UnityEngine;
using System.Collections.Generic;

public class BottleSpawner : MonoBehaviour
{
    [Header("Table Settings")]
    public GameObject table; // Asigna la mesa en el Inspector
    public GameObject bottlePrefab; // Prefab de la botella

    [Header("Grid Settings")]
    public float spacing = 0.2f; // Espacio entre botellas
    public int rows = 3;
    public int columns = 4;

    private List<GameObject> currentBottles = new List<GameObject>();
    [Header("Height Offset")]
    public float heightOffset = 0.05f;
    void Start()
    {
        SpawnBottles();
    }

    public void SpawnBottles()
    {
        // Limpia botellas previas si existen
        foreach (GameObject bottle in currentBottles)
        {
            Destroy(bottle);
        }
        currentBottles.Clear();

        // Obtiene dimensiones de la mesa
        MeshFilter meshFilter = table.GetComponent<MeshFilter>();
        Vector3 tableScale = table.transform.localScale;
        Bounds meshBounds = meshFilter.sharedMesh.bounds;
        Vector3 tableSize = Vector3.Scale(meshBounds.size, tableScale);

        float tableWidth = tableSize.x;
        float tableDepth = tableSize.z;

        // Calcula la altura real superior de la mesa
        float tableTopY = table.GetComponent<Renderer>().bounds.max.y;

        // Calcula inicio para centrar el grid sobre la mesa
        float startX = -tableWidth / 2f + (spacing / 2f);
        float startZ = -tableDepth / 2f + (spacing / 2f);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Posición local respecto a la mesa
                float localX = startX + (i * spacing);
                float localZ = startZ + (j * spacing);
                Vector3 localPos = new Vector3(localX, 0f, localZ);

                // Convierte a posición global respetando rotación y posición
                Vector3 spawnPos = table.transform.TransformPoint(localPos);

                // Ajusta la Y para colocarla exactamente sobre la superficie
                spawnPos.y = tableTopY + heightOffset;

                // Instancia la botella
                GameObject bottle = Instantiate(bottlePrefab, spawnPos, Quaternion.identity);
                currentBottles.Add(bottle);
            }
        }
    }
}