using UnityEngine;

// Definimos los tipos de bloques según tu narrativa técnica
public enum BlockType
{
    Prismafosil,    // A
    Mnemico,        // B
    Basal,          // C
    LimoAstral,     // D
    CumuloEter,     // E
    SiliceSedoso,   // F
    GramaEdenica,   // G
    VoxelVoltaico   // H
}

// Creamos el objeto de datos
[CreateAssetMenu(fileName = "NewBlockData", menuName = "AfterLife/Block Data", order = 1)]
public class BlockData : ScriptableObject
{
    [Header("Identidad")]
    public string blockName;
    public BlockType type;

    [Header("Geometría (Voxel Shapes 4x4)")]
    [Tooltip("El cubo sólido. Arrastra aquí el prefab 'fullblock'")]
    public GameObject fullBlockPrefab;

    [Tooltip("La rampa. Arrastra aquí el prefab 'slope' (o Slope_Final)")]
    public GameObject slopePrefab;

    [Tooltip("La esquina. Arrastra aquí el prefab 'corner'")]
    public GameObject cornerPrefab;

    [Tooltip("El bloque morado para rincones interiores")]
    public GameObject innerCornerPrefab;  // <--- ¡NUEVO!

    [Header("Feedback Sensorial")]
    [Tooltip("Sonido al pisar")]
    public AudioClip stepSound;

    [Header("Visuales (Look & Feel)")]
    [Tooltip("El material que vestirá a los bloques de este bioma")]
    public Material blockMaterial; // <--- AQUÍ VA LA TEXTURA/SHADER

    [Tooltip("Perfil de Volumen (Post-Procesado) al entrar en este bioma")]
    // Usamos string o un objeto VolumeProfile. Por ahora dejémoslo simple.
    public UnityEngine.Rendering.VolumeProfile biomePostProcess;

}