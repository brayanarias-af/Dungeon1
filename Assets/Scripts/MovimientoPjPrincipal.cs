using UnityEngine;

public class MovimientoPjPrincipal : MonoBehaviour
{
   // Velocidad de movimiento del personaje
    public float velocidad = 5f;

    // Referencia al Rigidbody2D
    private Rigidbody2D rb;

    // Dirección de movimiento
    private Vector2 movimiento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Capturar las teclas del jugador
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Establecer dirección
        movimiento = new Vector2(horizontal, vertical).normalized;
    }

    void FixedUpdate()
    {
        // Mover al personaje mediante físicas
        rb.MovePosition(rb.position + movimiento * velocidad * Time.fixedDeltaTime);
    }
}
