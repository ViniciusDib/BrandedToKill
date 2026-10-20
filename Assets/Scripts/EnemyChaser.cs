using UnityEngine;

// Inimigo que fica parado e persegue o jogador quando ele chega perto.
public class EnemyChaser : MonoBehaviour
{
    public float speed = 3f;
    public float detectionRange = 6f;

    Rigidbody2D rb;
    Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        if (toPlayer.magnitude < detectionRange)
        {
            Vector2 direction = toPlayer.normalized;
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            rb.MoveRotation(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }
    }
}
