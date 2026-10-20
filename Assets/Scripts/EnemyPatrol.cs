using UnityEngine;

// Inimigo que anda de um ponto até outro e volta (patrulha com direção pré-determinada).
public class EnemyPatrol : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 2.5f;

    Rigidbody2D rb;
    Vector2 target;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = pointB;
    }

    void FixedUpdate()
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        Vector2 direction = target - rb.position;
        rb.MovePosition(newPosition);

        if (direction.magnitude > 0.01f)
            rb.MoveRotation(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);

        // Chegou no ponto? Troca o destino.
        if (Vector2.Distance(rb.position, target) < 0.1f)
            target = target == pointA ? pointB : pointA;
    }
}
