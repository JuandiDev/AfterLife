using UnityEngine;
using System.Collections;

public class TerrainManager : MonoBehaviour
{
    [Header("Configuración del Mundo")]
    public int worldSize = 64;
    public float blockSize = 4f;
    public int terrainDepth = 5;

    [Header("Orografía")]
    public float heightScale = 12f;
    public float noiseScale = 0.05f;
    public float heightOffset = 0f;

    [Range(0, 360)]
    public float cornerRotationFix = 90f; // <--- ¡NUEVA VARIABLE MAESTRA!

    [Header("Biomas")]
    public BlockData[] biomePalette;
    public float biomeNoiseScale = 0.03f;

    private bool isGenerating = false;
    private int[,] heightMap;

    void Start()
    {
        StartCoroutine(GenerateWorldRoutine());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isGenerating)
        {
            StartCoroutine(RegenerateWorld());
        }
    }

    IEnumerator RegenerateWorld()
    {
        isGenerating = true;
        foreach (Transform child in transform) Destroy(child.gameObject);
        yield return null;
        yield return StartCoroutine(GenerateWorldRoutine());
    }

    IEnumerator GenerateWorldRoutine()
    {
        isGenerating = true;
        heightMap = new int[worldSize, worldSize];

        // 1. CÁLCULO
        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                float xCoord = (float)x * noiseScale;
                float zCoord = (float)z * noiseScale;
                heightMap[x, z] = Mathf.RoundToInt(Mathf.PerlinNoise(xCoord, zCoord) * heightScale + heightOffset);
            }
        }

        // 2. SUAVIZADO (Vital para que funcione la lógica de rampas)
        bool terrainChanged = true;
        int maxPasses = 100;
        int pass = 0;

        while (terrainChanged && pass < maxPasses)
        {
            terrainChanged = SmoothTerrainLogic();
            pass++;
            if (pass % 5 == 0) yield return null;
        }

        // 3. CONSTRUCCIÓN
        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                SpawnColumn(x, z, heightMap[x, z]);
            }
            if (x % 2 == 0) yield return null;
        }

        isGenerating = false;
        Debug.Log("Mundo Generado con Lógica 'Slope-on-High'.");
    }

    bool SmoothTerrainLogic()
    {
        bool changed = false;
        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                if (x > 0) changed |= ClampHeight(x, z, x - 1, z);
                if (x < worldSize - 1) changed |= ClampHeight(x, z, x + 1, z);
                if (z > 0) changed |= ClampHeight(x, z, x, z - 1);
                if (z < worldSize - 1) changed |= ClampHeight(x, z, x, z + 1);
            }
        }
        return changed;
    }

    bool ClampHeight(int x1, int z1, int x2, int z2)
    {
        int h1 = heightMap[x1, z1];
        int h2 = heightMap[x2, z2];
        if (h1 - h2 > 1) { heightMap[x2, z2] = h1 - 1; return true; }
        else if (h2 - h1 > 1) { heightMap[x1, z1] = h2 - 1; return true; }
        return false;
    }

    void SpawnColumn(int x, int z, int currentHeight)
    {
        float biomeVal = Mathf.PerlinNoise((x * biomeNoiseScale) + 100f, (z * biomeNoiseScale) + 100f);
        BlockData biome = PickBiome(biomeVal);
        if (biome == null) return;

        Vector3 posMultiplier = new Vector3(x * blockSize, 0, z * blockSize);

        // RELLENO SÓLIDO (Base segura)
        int bottomLimit = currentHeight - terrainDepth;
        for (int y = bottomLimit; y < currentHeight; y++)
        {
            Vector3 fillPos = new Vector3(0, y * blockSize, 0) + posMultiplier;

            // Instanciamos el bloque de relleno
            GameObject fillBlock = Instantiate(biome.fullBlockPrefab, fillPos, Quaternion.identity);
            fillBlock.transform.parent = transform;

            // --- AQUÍ AÑADIMOS LA INYECCIÓN DE MATERIAL ---
            // (Para que el relleno tenga el mismo color que la superficie)
            if (biome.blockMaterial != null)
            {
                Renderer rend = fillBlock.GetComponent<Renderer>();
                if (rend == null) rend = fillBlock.GetComponentInChildren<Renderer>();

                if (rend != null)
                {
                    rend.material = biome.blockMaterial;
                }
            }
            // ---------------------------------------------

            fillBlock.isStatic = true;
        }

        // --- SUPERFICIE ---
        Vector3 topPos = new Vector3(0, currentHeight * blockSize, 0) + posMultiplier;
        SpawnSurfaceBlock(x, z, currentHeight, topPos, biome);
    }

    void SpawnSurfaceBlock(int x, int z, int currentHeight, Vector3 pos, BlockData biome)
    {
        GameObject shapeToSpawn = biome.fullBlockPrefab;
        Quaternion rotation = Quaternion.identity;

        // 1. VECINOS DIRECTOS (Cruz)
        int h_N = (z < worldSize - 1) ? heightMap[x, z + 1] : currentHeight;
        int h_S = (z > 0) ? heightMap[x, z - 1] : currentHeight;
        int h_E = (x < worldSize - 1) ? heightMap[x + 1, z] : currentHeight;
        int h_W = (x > 0) ? heightMap[x - 1, z] : currentHeight;

        bool low_N = h_N < currentHeight;
        bool low_S = h_S < currentHeight;
        bool low_E = h_E < currentHeight;
        bool low_W = h_W < currentHeight;

        // 2. VECINOS DIAGONALES (X) - ¡NUEVO PARA EL INNER CORNER!
        // Necesitamos saber si la diagonal cae, aunque los lados sean altos.

        int h_NE = (z < worldSize - 1 && x < worldSize - 1) ? heightMap[x + 1, z + 1] : currentHeight;
        int h_SE = (z > 0 && x < worldSize - 1) ? heightMap[x + 1, z - 1] : currentHeight;
        int h_SW = (z > 0 && x > 0) ? heightMap[x - 1, z - 1] : currentHeight;
        int h_NW = (z < worldSize - 1 && x > 0) ? heightMap[x - 1, z + 1] : currentHeight;

        bool low_NE = h_NE < currentHeight;
        bool low_SE = h_SE < currentHeight;
        bool low_SW = h_SW < currentHeight;
        bool low_NW = h_NW < currentHeight;

        // --- JERARQUÍA DE DECISIÓN ---

        // CASO A: CORNERS EXTERIORES (Azules) - "Punta de Montaña"
        // (Tengo dos precipicios a los lados)
        if (low_N && low_E)
        {
            shapeToSpawn = biome.cornerPrefab;
            rotation = Quaternion.Euler(0, 270, 0);
        }
        else if (low_S && low_E)
        {
            shapeToSpawn = biome.cornerPrefab;
            rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (low_S && low_W)
        {
            shapeToSpawn = biome.cornerPrefab;
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (low_N && low_W)
        {
            shapeToSpawn = biome.cornerPrefab;
            rotation = Quaternion.Euler(0, 180, 0);
        }

        // CASO B: RAMPAS (Slopes) - "Bajada Recta"
        // (Tengo un solo precipicio a un lado)
        else if (low_N) { shapeToSpawn = biome.slopePrefab; rotation = Quaternion.Euler(0, 180, 0); }
        else if (low_S) { shapeToSpawn = biome.slopePrefab; rotation = Quaternion.Euler(0, 0, 0); }
        else if (low_E) { shapeToSpawn = biome.slopePrefab; rotation = Quaternion.Euler(0, 270, 0); }
        else if (low_W) { shapeToSpawn = biome.slopePrefab; rotation = Quaternion.Euler(0, 90, 0); }

        // CASO C: INNER CORNERS (Morados) - "Rincón de Valle" <--- ¡AQUÍ ESTÁ LA SOLUCIÓN!
        // Lógica: Mis lados son ALTOS (soy pared), pero mi DIAGONAL es BAJA (hay camino).
        // Este bloque reemplaza al FullBlock verde para suavizar el paso.

        else if (low_NE)
        {
            shapeToSpawn = biome.innerCornerPrefab;
            // Orientar la "bajada" hacia el Noreste.
            // Prueba base: 270. Si no encaja, usa el truco de la tecla R y ajusta aquí.
            rotation = Quaternion.Euler(0, 270, 0);
        }
        else if (low_SE)
        {
            shapeToSpawn = biome.innerCornerPrefab;
            rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (low_SW)
        {
            shapeToSpawn = biome.innerCornerPrefab;
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (low_NW)
        {
            shapeToSpawn = biome.innerCornerPrefab;
            rotation = Quaternion.Euler(0, 180, 0);
        }

        // CASO D: FullBlock (Verde)
        // Si no pasa nada de lo anterior, soy un bloque sólido normal.

        GameObject block = Instantiate(shapeToSpawn, pos, rotation);
        block.transform.parent = transform;

        // --- INYECCIÓN DE MATERIAL ---
        if (biome.blockMaterial != null)
        {
            // Buscamos el Renderer en el objeto o sus hijos
            Renderer rend = block.GetComponent<Renderer>();

            // Si no está en la raíz (a veces al importar FBX queda en un hijo), buscamos en hijos
            if (rend == null) rend = block.GetComponentInChildren<Renderer>();

            if (rend != null)
            {
                rend.material = biome.blockMaterial;
            }
        }

        block.isStatic = true;
    }

    BlockData PickBiome(float value)
    {
        if (biomePalette == null || biomePalette.Length == 0) return null;
        int index = Mathf.FloorToInt(value * biomePalette.Length);
        index = Mathf.Clamp(index, 0, biomePalette.Length - 1);
        return biomePalette[index];
    }
}