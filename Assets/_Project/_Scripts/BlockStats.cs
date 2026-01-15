using UnityEngine;

public class BlockStats : MonoBehaviour
{
    [Header("Integridad de Datos")]
    public float maxIntegrity = 100f; // La vida total (100%)
    [SerializeField] public float currentIntegrity; // <--- Ahora es pública

    void Start()
    {
        currentIntegrity = maxIntegrity;
    }

    // Esta función la llamará el Player cuando le dispare con el láser
    public void Decrypt(float damageAmount)
    {
        currentIntegrity -= damageAmount;

        // Feedback visual (opcional por ahora): vibración o cambio de color aquí

        if (currentIntegrity <= 0)
        {
            DissolveBlock();
        }
    }

    void DissolveBlock()
    {
        // Aquí pondremos efectos de partículas o sonido de "Glitch" antes de morir
        Destroy(gameObject);
    }
}