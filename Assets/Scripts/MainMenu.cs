using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu inicial (tutorial 16).
public class MainMenu : MonoBehaviour
{
    public Button playButton;
    public Button howToButton;
    public Button quitButton;
    public Button backButton;
    public GameObject mainPanel;
    public GameObject howToPanel;

    void Start()
    {
        playButton.onClick.AddListener(Play);
        howToButton.onClick.AddListener(() => ShowHowTo(true));
        backButton.onClick.AddListener(() => ShowHowTo(false));
        quitButton.onClick.AddListener(Application.Quit);

#if UNITY_WEBGL
        quitButton.gameObject.SetActive(false); // no navegador não dá para "sair"
#endif
        ShowHowTo(false);
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    void ShowHowTo(bool show)
    {
        mainPanel.SetActive(!show);
        howToPanel.SetActive(show);
        // Seleciona um botão para o controle conseguir navegar no menu.
        EventSystem.current.SetSelectedGameObject(show ? backButton.gameObject : playButton.gameObject);
    }
}
