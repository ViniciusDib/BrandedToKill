using UnityEngine;
using UnityEngine.UI;

// Mostra pontos, moedas, tempo e vidas durante o jogo.
public class HUD : MonoBehaviour
{
    public Text scoreText;
    public Text coinsText;
    public Text timeText;
    public Text livesText;

    void Update()
    {
        scoreText.text = "PONTOS  " + GameManager.score;
        coinsText.text = "MOEDAS  " + GameManager.coins + " / " + GameManager.totalCoins;
        timeText.text = GameManager.FormatTime(GameManager.timeLeft);
        livesText.text = "VIDAS  " + GameManager.lives;

        // Tempo acabando: o relógio fica vermelho.
        timeText.color = GameManager.timeLeft < 10f ? new Color(1f, 0.3f, 0.35f) : Color.white;
    }
}
