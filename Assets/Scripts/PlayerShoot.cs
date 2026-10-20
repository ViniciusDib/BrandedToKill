using UnityEngine;
using UnityEngine.InputSystem;

// Atira na direção em que o jogador está olhando.
// Espaço, clique esquerdo ou RT/A no controle (segure para atirar sem parar).
public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 14f;
    public float timeBetweenShots = 0.3f;

    public AudioSource audioSource;
    public AudioClip shootSound;

    PlayerMovement player;
    float nextShotTime;

    void Start()
    {
        player = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (FirePressed() && Time.time >= nextShotTime)
        {
            nextShotTime = Time.time + timeBetweenShots;
            Shoot();
        }
    }

    bool FirePressed()
    {
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool gamepad = Gamepad.current != null &&
                       (Gamepad.current.rightTrigger.isPressed || Gamepad.current.buttonSouth.isPressed);
        return keyboard || mouse || gamepad;
    }

    void Shoot()
    {
        Vector2 direction = player.FacingDirection;
        Vector2 spawnPosition = (Vector2)transform.position + direction * 0.7f;

        // Tutorial 13: o prefab da bala é usado como parâmetro do script.
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletSpeed;

        audioSource.PlayOneShot(shootSound);
    }
}
