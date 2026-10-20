using UnityEngine;
using UnityEngine.SceneManagement;

// Entrada da fase secreta: um bueiro escondido. Quem pisar nele cai no cofre da yakuza.
// Pontos, vidas e tempo continuam, porque ficam guardados no GameManager estático.
public class SecretEntrance : MonoBehaviour
{
    public string secretScene = "FaseSecreta";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            SceneManager.LoadScene(secretScene);
    }
}
