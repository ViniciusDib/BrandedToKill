using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Testes automáticos (Window > General > Test Runner > PlayMode > Run All).
public class GameTests
{
    IEnumerator LoadGame()
    {
        SceneManager.LoadScene("Game");
        yield return null;
        yield return new WaitForSeconds(0.3f);
    }

    [UnityTest]
    public IEnumerator Menu_PlayButton_OpensGame()
    {
        SceneManager.LoadScene("MainMenu");
        yield return null;
        Object.FindFirstObjectByType<MainMenu>().playButton.onClick.Invoke();
        yield return null;
        yield return null;
        Assert.AreEqual("Game", SceneManager.GetActiveScene().name);
    }

    [UnityTest]
    public IEnumerator Game_StartsWithTimerLivesAndCoins()
    {
        yield return LoadGame();
        Assert.AreEqual(12, GameManager.totalCoins);
        Assert.AreEqual(3, GameManager.lives);
        Assert.Less(GameManager.timeLeft, 90f);
    }

    [UnityTest]
    public IEnumerator CollectingCoin_AddsScore()
    {
        yield return LoadGame();
        var player = GameObject.FindGameObjectWithTag("Player");
        var coin = GameObject.FindGameObjectWithTag("Coletavel");
        player.GetComponent<Rigidbody2D>().position = coin.transform.position;
        yield return new WaitForSeconds(0.2f);
        Assert.AreEqual(1, GameManager.coins);
        Assert.AreEqual(10, GameManager.score);
    }

    [UnityTest]
    public IEnumerator Clock_AddsTime()
    {
        yield return LoadGame();
        var player = GameObject.FindGameObjectWithTag("Player");
        var clock = GameObject.FindGameObjectWithTag("Relogio");
        float before = GameManager.timeLeft;
        player.GetComponent<Rigidbody2D>().position = clock.transform.position;
        yield return new WaitForSeconds(0.2f);
        Assert.Greater(GameManager.timeLeft, before + 9f);
    }

    [UnityTest]
    public IEnumerator TouchingEnemy_LosesLife()
    {
        yield return LoadGame();
        yield return new WaitForSeconds(1.6f); // espera a proteção do início acabar
        var player = GameObject.FindGameObjectWithTag("Player");
        var enemy = Object.FindFirstObjectByType<EnemyChaser>();
        player.GetComponent<Rigidbody2D>().position = (Vector2)enemy.transform.position + Vector2.right * 0.5f;
        yield return new WaitForSeconds(0.5f);
        Assert.AreEqual(2, GameManager.lives);
    }

    [UnityTest]
    public IEnumerator Bullet_KillsEnemy_AndGivesTime()
    {
        yield return LoadGame();
        var enemy = Object.FindFirstObjectByType<EnemyChaser>();
        var shoot = Object.FindFirstObjectByType<PlayerShoot>();
        float before = GameManager.timeLeft;
        // atira vindo do centro da arena (para a bala não nascer dentro de uma parede)
        Vector3 fromCenter = enemy.transform.position.x > 0 ? Vector3.left : Vector3.right;
        var bullet = Object.Instantiate(shoot.bulletPrefab, enemy.transform.position + fromCenter * 1.5f, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = -fromCenter * 14f;
        yield return new WaitForSeconds(0.4f);
        Assert.IsTrue(enemy == null, "O inimigo deveria ter sido destruído");
        Assert.AreEqual(50, GameManager.score);
        Assert.Greater(GameManager.timeLeft, before + 2f);
    }

    [UnityTest]
    public IEnumerator AllCoins_IsVictory()
    {
        yield return LoadGame();
        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        foreach (var coin in GameObject.FindGameObjectsWithTag("Coletavel"))
        {
            player.position = coin.transform.position;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            if (SceneManager.GetActiveScene().name != "Game") break;
        }
        yield return null;
        Assert.AreEqual("EndGame", SceneManager.GetActiveScene().name);
        Assert.IsTrue(GameManager.victory);
    }

    [UnityTest]
    public IEnumerator SecretManhole_OpensSecretLevel_KeepingProgress()
    {
        yield return LoadGame();
        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        player.position = GameObject.FindGameObjectWithTag("Coletavel").transform.position; // pega 1 moeda
        yield return new WaitForSeconds(0.2f);
        int score = GameManager.score;
        float time = GameManager.timeLeft;
        player.position = Object.FindFirstObjectByType<SecretEntrance>().transform.position;
        yield return new WaitForSeconds(0.3f);
        Assert.AreEqual("FaseSecreta", SceneManager.GetActiveScene().name);
        Assert.AreEqual(score, GameManager.score, "Os pontos devem continuar");
        Assert.AreEqual(3, GameManager.lives);
        Assert.Less(GameManager.timeLeft, time, "O tempo continua de onde parou");
        Assert.AreEqual(8, GameManager.totalCoins);
    }

    [UnityTest]
    public IEnumerator SecretLevel_AllCoins_IsSpecialVictory()
    {
        yield return LoadGame();
        SceneManager.LoadScene("FaseSecreta");
        yield return null;
        yield return new WaitForSeconds(0.3f);
        int score = GameManager.score;
        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        foreach (var coin in GameObject.FindGameObjectsWithTag("Coletavel"))
        {
            player.position = coin.transform.position;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            if (SceneManager.GetActiveScene().name != "FaseSecreta") break;
        }
        yield return null;
        Assert.AreEqual("EndGame", SceneManager.GetActiveScene().name);
        Assert.IsTrue(GameManager.victory);
        StringAssert.Contains("cofre", GameManager.endMessage);
        Assert.GreaterOrEqual(GameManager.score, score + 8 * 50);
    }

    [UnityTest]
    public IEnumerator TimeOut_IsGameOver_AndCanPlayAgain()
    {
        yield return LoadGame();
        GameManager.timeLeft = 0.1f;
        yield return new WaitForSeconds(0.3f);
        Assert.AreEqual("EndGame", SceneManager.GetActiveScene().name);
        Assert.IsFalse(GameManager.victory);
        Object.FindFirstObjectByType<EndGame>().playAgainButton.onClick.Invoke();
        yield return null;
        yield return null;
        Assert.AreEqual("Game", SceneManager.GetActiveScene().name);
    }
}
