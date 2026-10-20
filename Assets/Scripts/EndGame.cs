using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tela de fim (tutorial 17): lê o resultado guardado no GameManager estático.
// Sempre oferece jogar de novo ou voltar ao menu, então o jogador nunca fica preso.
public class EndGame : MonoBehaviour
{
    public Text titleText;
    public Text messageText;
    public Text statsText;
    public Button playAgainButton;
    public Button menuButton;

    public AudioSource audioSource;
    public AudioClip victorySound;
    public AudioClip gameOverSound;

    void Start()
    {
        bool won = GameManager.victory;
        titleText.text = won ? "MISSÃO CUMPRIDA" : "FIM DE JOGO";
        titleText.color = won ? new Color(0.2f, 1f, 1f) : new Color(1f, 0.25f, 0.45f);
        messageText.text = GameManager.endMessage;
        statsText.text =
            "Tempo de jogo: " + GameManager.FormatTime(GameManager.timePlayed) + "\n" +
            "Tempo restante: " + GameManager.FormatTime(GameManager.timeLeft) + "\n" +
            "Moedas: " + GameManager.coins + " / " + GameManager.totalCoins + "\n" +
            "Pontos: " + GameManager.score;

        audioSource.PlayOneShot(won ? victorySound : gameOverSound);

        playAgainButton.onClick.AddListener(() => SceneManager.LoadScene("Game"));
        menuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        EventSystem.current.SetSelectedGameObject(playAgainButton.gameObject);
    }
}
