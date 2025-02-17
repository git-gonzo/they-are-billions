using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    public float moveSpeed = 10f;  // Velocidad 
    public float edgeThreshold = 20f;  // Distancia 
    public Vector2 screenBounds; 

    void Start()
    {
        screenBounds = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        Vector3 moveDirection = Vector3.zero;
        Vector3 mousePosition = Input.mousePosition;

        // Movimiento en el eje X
        if (mousePosition.x <= edgeThreshold) moveDirection.x = -1; 
        else if (mousePosition.x >= screenBounds.x - edgeThreshold) moveDirection.x = 1;

        // Movimiento en el eje Z 
        if (mousePosition.y <= edgeThreshold) moveDirection.z = -1;
        else if (mousePosition.y >= screenBounds.y - edgeThreshold) moveDirection.z = 1; 

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

}
