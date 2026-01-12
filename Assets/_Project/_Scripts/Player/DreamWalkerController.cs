using UnityEngine;

namespace AfterLife.Core.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class DreamWalkerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float lookXLimit = 85.0f;

        [Header("References")]
        [SerializeField] private Camera playerCamera;

        // Estado interno
        private CharacterController characterController;
        private Vector3 moveDirection = Vector3.zero;
        private float rotationX = 0;

        void Start()
        {
            characterController = GetComponent<CharacterController>();

            // Bloquear el cursor en el centro de la pantalla
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            // Estamos en el suelo?
            if (characterController.isGrounded && moveDirection.y < 0)
            {
                moveDirection.y = -2f; // Pequeña fuerza hacia abajo para asegurar contacto
            }

            // Inputs WASD (Legacy Input System)
            // Nota: Si usas el New Input System, avísame para actualizar esto.
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            float curSpeedX = walkSpeed * Input.GetAxis("Vertical");
            float curSpeedY = walkSpeed * Input.GetAxis("Horizontal");

            // Calculamos el movimiento en X y Z (preservando la gravedad en Y)
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            // Reaplicamos gravedad
            moveDirection.y = movementDirectionY;
            moveDirection.y += gravity * Time.deltaTime;

            // Mover el controlador
            characterController.Move(moveDirection * Time.deltaTime);
        }

        private void HandleRotation()
        {
            // Rotación de la Cámara (Arriba/Abajo)
            rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            // Rotación del Cuerpo (Izquierda/Derecha)
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
        }
    }
}