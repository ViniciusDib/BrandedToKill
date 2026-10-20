using UnityEngine;
using UnityEngine.SceneManagement;

// Tutorial 17: GameManager com dados estáticos. Os valores "static" continuam existindo
// quando a cena muda, então a cena EndGame consegue mostrar o resultado da partida.
public class GameManager : MonoBehaviour
{
    public static int score;
    public static int lives;
    public static int coins;
    public static int totalCoins;
    public static float timeLeft;
    public static float timePlayed;
    public static bool victory;
    public static string endMessage = "";
    static bool gameEnded;
    static int currentCoinValue = 10;
    static string currentVictoryMessage = "";

    [Header("Configuração da partida")]
    [Tooltip("Desmarcado na fase secreta: continua com os pontos, vidas e tempo da fase principal.")]
    public bool newGame = true;
    public float startTime = 90f;
    public int startLives = 3;
    public int coinValue = 10;
    public string victoryMessage = "Você pegou todas as moedas de ouro!";

    void Start()
    {
        // lives <= 0: a cena foi aberta direto pelo editor, sem partida em andamento.
        if (newGame || lives <= 0)
        {
            score = 0;
            lives = startLives;
            timeLeft = startTime;
            timePlayed = 0f;
        }
        coins = 0;
        victory = false;
        gameEnded = false;
        totalCoins = GameObject.FindGameObjectsWithTag("Coletavel").Length;
        currentCoinValue = coinValue;
        currentVictoryMessage = victoryMessage;
    }

    void Update()
    {
        if (gameEnded) return;

        // Cronômetro: conta para baixo. Se zerar, perdeu.
        timeLeft -= Time.deltaTime;
        timePlayed += Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndGame(false, "O tempo acabou! O chefe fugiu de Kabukichō.");
        }
    }

    public static void AddCoin()
    {
        coins++;
        score += currentCoinValue;
        if (coins >= totalCoins)
            EndGame(true, currentVictoryMessage);
    }

    // Tempo como mecânica: relógios e inimigos derrotados dão segundos extras.
    public static void AddTime(float seconds)
    {
        timeLeft += seconds;
    }

    public static void EnemyKilled()
    {
        score += 50;
        AddTime(3f);
    }

    public static void LoseLife()
    {
        lives--;
        if (lives <= 0)
        {
            lives = 0;
            EndGame(false, "Os capangas da yakuza te pegaram.");
        }
    }

    static void EndGame(bool won, string message)
    {
        if (gameEnded) return;
        gameEnded = true;
        victory = won;
        endMessage = message;
        if (won) score += Mathf.RoundToInt(timeLeft) * 5; // bônus pelo tempo que sobrou
        SceneManager.LoadScene("EndGame");
    }

    public static string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60f);
        int s = (int)(seconds % 60f);
        return m.ToString("00") + ":" + s.ToString("00");
    }
}
