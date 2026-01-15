using UnityEngine;
using UnityEngine.UI; // Necesario para la UI

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración General")]
    public Camera playerCamera;
    public float reachDistance = 30f;
    public float blockSize = 4f;
    public LayerMask whatIsGround;

    [Header("Visuales 3D")]
    public GameObject cursorPrefab;
    private GameObject currentCursor;

    [Header("Herramienta Desencriptado")]
    public bool hasDecryptorTool = true;
    public float miningSpeed = 50f;

    // --- ESTA ES LA PARTE QUE TE FALTABA ---
    [Header("Inventario / Hotbar")]
    public GameObject[] buildableBlocks; // ARRAY: Tus 3 bloques
    public RectTransform[] uiSlots;      // ARRAY: Tus 3 slots de UI
    public RectTransform highlightBorder;// El borde que se mueve
    private int currentBlockIndex = 0;   // Cuál tenemos seleccionado
    // ---------------------------------------

    [Header("Interfaz Minado")]
    public Image progressCircle;

    [Header("Calibración")]
    public Vector3 cursorOffset = Vector3.zero;

    void Start()
    {
        if (cursorPrefab != null)
        {
            currentCursor = Instantiate(cursorPrefab);
            currentCursor.SetActive(false);
        }

        // Iniciar la UI en el slot correcto
        UpdateUI();
    }

    void Update()
    {
        HandleInventoryInput(); // <--- IMPORTANTE: Escuchar teclas 1, 2, 3

        RaycastHit hit;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out hit, reachDistance, whatIsGround))
        {
            // --- ACTUALIZAR CURSOR ---
            if (currentCursor != null)
            {
                currentCursor.SetActive(true);
                Vector3 targetPos = hit.point - (hit.normal * 0.1f);

                float x = Mathf.FloorToInt(targetPos.x / blockSize) * blockSize;
                float y = Mathf.FloorToInt(targetPos.y / blockSize) * blockSize;
                float z = Mathf.FloorToInt(targetPos.z / blockSize) * blockSize;

                currentCursor.transform.position = new Vector3(x, y, z) + cursorOffset;
            }

            // --- ROMPER (Clic Izquierdo) ---
            bool isMining = false;
            if (Input.GetMouseButton(0) && hasDecryptorTool)
            {
                BlockStats block = hit.collider.GetComponent<BlockStats>();
                if (block == null && hit.collider.transform.parent != null)
                    block = hit.collider.transform.parent.GetComponent<BlockStats>();

                if (block != null)
                {
                    isMining = true;
                    block.Decrypt(miningSpeed * Time.deltaTime);
                    if (progressCircle != null)
                        progressCircle.fillAmount = 1f - (block.currentIntegrity / block.maxIntegrity);
                }
            }
            if (!isMining && progressCircle != null) progressCircle.fillAmount = 0f;

            // --- CONSTRUIR (Clic Derecho) ---
            // Verificamos que tengamos bloques en la lista antes de intentar construir
            if (Input.GetMouseButtonDown(1) && buildableBlocks.Length > 0)
            {
                Vector3 pointInAir = hit.point + (hit.normal * (blockSize * 0.5f));

                float x = Mathf.FloorToInt(pointInAir.x / blockSize) * blockSize;
                float y = Mathf.FloorToInt(pointInAir.y / blockSize) * blockSize;
                float z = Mathf.FloorToInt(pointInAir.z / blockSize) * blockSize;

                Vector3 buildPos = new Vector3(x, y, z) + cursorOffset;

                // USAMOS EL BLOQUE SELECCIONADO DE LA LISTA
                Instantiate(buildableBlocks[currentBlockIndex], buildPos, Quaternion.identity);
            }
        }
        else
        {
            if (currentCursor != null) currentCursor.SetActive(false);
            if (progressCircle != null) progressCircle.fillAmount = 0f;
        }
    }

    // --- LÓGICA DEL INVENTARIO ---
    void HandleInventoryInput()
    {
        // Teclas 1, 2, 3
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentBlockIndex = 0; UpdateUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentBlockIndex = 1; UpdateUI(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentBlockIndex = 2; UpdateUI(); }

        // Rueda del Ratón
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            if (scroll > 0) currentBlockIndex--;
            else currentBlockIndex++;

            // Ciclo infinito (Loop)
            if (currentBlockIndex > buildableBlocks.Length - 1) currentBlockIndex = 0;
            if (currentBlockIndex < 0) currentBlockIndex = buildableBlocks.Length - 1;

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Mover el borde brillante al slot correspondiente
        if (highlightBorder != null && uiSlots.Length > currentBlockIndex)
        {
            highlightBorder.position = uiSlots[currentBlockIndex].position;
        }
    }

    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * reachDistance);
        }
    }
}