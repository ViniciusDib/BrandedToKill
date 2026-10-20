using UnityEngine;

// Bala do jogador: derruba inimigos e some ao bater numa parede.
public class Bullet : MonoBehaviour
{
    public AudioClip enemyDeathSound;
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime); // some sozinha depois de um tempo
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Inimigo"))
        {
            AudioSource.PlayClipAtPoint(enemyDeathSound, Camera.main.transform.position);
            Destroy(other.gameObject);
            Destroy(gameObject);
            GameManager.EnemyKilled();
        }
        else if (other.CompareTag("Parede"))
        {
            Destroy(gameObject);
        }
    }
}
