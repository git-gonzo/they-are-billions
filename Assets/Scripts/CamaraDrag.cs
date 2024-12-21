using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    public float dragSpeed = 10f; // Velocidad de arrastre
    private Vector3 dragOrigin; // Posición inicial del clic del mouse
    private bool isDragging = false; // Para verificar si estamos arrastrando

    void Update()
    {
        // Detectar el clic izquierdo del ratón
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }

        // Detectar si se deja de hacer clic
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Si estamos arrastrando, mover la cámara
        if (isDragging)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 difference = dragOrigin - currentMousePosition;

            // Convertir el movimiento del mouse en movimiento de la cámara
            Vector3 move = new Vector3(difference.x, 0, difference.y) * dragSpeed * Time.deltaTime;

            // Mover la cámara en el plano XZ
            transform.Translate(move, Space.World);

            // Actualizar la posición inicial para el siguiente frame
            dragOrigin = currentMousePosition;
        }
    }
}
