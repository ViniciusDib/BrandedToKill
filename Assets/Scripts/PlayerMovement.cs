using UnityEngine;
using UnityEngine.InputSystem;

// Movimento do jogador visto de cima (tutorial 08 a 15), coleta de itens (tutorial 12),
// som ao coletar (tutorial 14) e dano ao encostar em inimigos.
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float invulnerableTime = 1.5f;

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip coinSound;
    public AudioClip clockSound;
    public AudioClip hurtSound;

    // Direção para onde o jogador está olhando (usada pelo tiro).
    public Vector2 FacingDirection { get; private set; } = Vector2.up;

    Rigidbody2D rb;
    SpriteRenderer sprite;
    Vector2 movement;
    float invulnerableUntil;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        invulnerableUntil = Time.time + invulnerableTime; // começa protegido por um instante
    }

    void Update()
    {
        // Lê teclado (WASD ou setas) e controle (analógico esquerdo).
        movement = Vector2.zero;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y += 1;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y -= 1;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x -= 1;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x += 1;
        }
        var gamepad = Gamepad.current;
        if (gamepad != null) movement += gamepad.leftStick.ReadValue();

        // Tutorial 15: normalizar para a diagonal não ser mais rápida.
        if (movement.magnitude > 1f) movement.Normalize();

        if (movement.magnitude > 0.1f) FacingDirection = movement.normalized;

        // Pisca enquanto está invencível.
        bool invulnerable = Time.time < invulnerableUntil;
        sprite.enabled = !invulnerable || Mathf.Repeat(Time.time * 10f, 1f) > 0.5f;
    }

    void FixedUpdate()
    {
        // Tutorial 11: movimento pelo Rigidbody2D usando Time.fixedDeltaTime.
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

        // Gira o sprite para a direção em que está andando.
        float angle = Mathf.Atan2(FacingDirection.y, FacingDirection.x) * Mathf.Rad2Deg - 90f;
        rb.MoveRotation(angle);
    }

    // Tutorial 12: coletáveis com Is Trigger + tag.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coletavel"))
        {
            audioSource.PlayOneShot(coinSound);
            Destroy(other.gameObject);
            GameManager.AddCoin();
        }
        else if (other.CompareTag("Relogio"))
        {
            audioSource.PlayOneShot(clockSound);
            Destroy(other.gameObject);
            GameManager.AddTime(10f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Inimigo")) TakeHit();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Inimigo")) TakeHit();
    }

    void TakeHit()
    {
        if (Time.time < invulnerableUntil) return;
        invulnerableUntil = Time.time + invulnerableTime;
        audioSource.PlayOneShot(hurtSound);
        GameManager.LoseLife();
    }
}
