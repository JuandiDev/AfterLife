using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración General")]
    public Camera playerCamera;
    public float reachDistance = 30f; // AUMENTADO a 30 (porque somos gigantes)
    public float blockSize = 4f;      // El tamaño de tus bloques
    public LayerMask whatIsGround;    // Qué capas podemos tocar

    [Header("Visuales")]
    public GameObject cursorPrefab;
    private GameObject currentCursor;

    [Header("Construcción")]
    [Tooltip("Arrastra aquí el PREFAB del bloque sólido que quieres poner")]
    public GameObject blockToBuild;

    [Header("Calibración Manual")]
    [Tooltip("Ajusta esto en Play para centrar el cursor. Prueba (2,2,2) o (0,0,0)")]
    public Vector3 cursorOffset = new Vector3(2f, 2f, 2f); // Valor por defecto común

    void Start()
    {
        // Instanciamos el cursor apagado
        if (cursorPrefab != null)
        {
            currentCursor = Instantiate(cursorPrefab);
            currentCursor.SetActive(false);
        }
    }

    void Update()
    {
        RaycastHit hit;
        // Rayo desde el centro de la pantalla
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Lanzamos el rayo
        if (Physics.Raycast(ray, out hit, reachDistance, whatIsGround))
        {
            // --- 1. ACTUALIZAR CURSOR (Visual) ---
            if (currentCursor != null)
            {
                currentCursor.SetActive(true);

                // Cálculo: Punto de impacto - un poquito de normal (para entrar al bloque)
                Vector3 targetPos = hit.point - (hit.normal * 0.1f);

                // Matemáticas de Grid (Redondeo al múltiplo de 4)
                float x = Mathf.FloorToInt(targetPos.x / blockSize) * blockSize;
                float y = Mathf.FloorToInt(targetPos.y / blockSize) * blockSize;
                float z = Mathf.FloorToInt(targetPos.z / blockSize) * blockSize;

                // Posición final + Tu calibración manual
                currentCursor.transform.position = new Vector3(x, y, z) + cursorOffset;
            }

            // --- 2. ROMPER (Click Izquierdo) ---
            if (Input.GetMouseButtonDown(0))
            {
                GameObject hitBlock = hit.collider.gameObject;

                // Truco de seguridad: Si golpeamos un hijo (modelo), borramos al padre
                if (hitBlock.transform.parent != null && hitBlock.transform.parent.GetComponent<Collider>() == null)
                {
                    // Si la estructura es compleja, ajusta esto. Por ahora, destroy directo suele servir.
                }

                Destroy(hitBlock);
                // Aquí podrías poner: AudioSource.PlayClipAtPoint(sonidoRomper, hit.point);
            }

            // --- 3. CONSTRUIR (Click Derecho) ---
            if (Input.GetMouseButtonDown(1) && blockToBuild != null)
            {
                // LÓGICA DE AIRE:
                // Tomamos el punto y salimos hacia afuera media unidad de bloque
                Vector3 pointInAir = hit.point + (hit.normal * (blockSize * 0.5f));

                // Redondeamos ese punto en el aire a la rejilla
                float x = Mathf.FloorToInt(pointInAir.x / blockSize) * blockSize;
                float y = Mathf.FloorToInt(pointInAir.y / blockSize) * blockSize;
                float z = Mathf.FloorToInt(pointInAir.z / blockSize) * blockSize;

                Vector3 buildPos = new Vector3(x, y, z) + cursorOffset;

                Instantiate(blockToBuild, buildPos, Quaternion.identity);
                // Aquí podrías poner: AudioSource.PlayClipAtPoint(sonidoPoner, buildPos);
            }
        }
        else
        {
            // Si miramos al cielo, escondemos el cursor
            if (currentCursor != null) currentCursor.SetActive(false);
        }
    }

    // Dibujo de ayuda en el editor para ver el alcance del brazo
    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * reachDistance);
        }
    }
}