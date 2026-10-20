using UnityEngine;

// Ajusta o zoom da câmera para a área do jogo caber inteira em qualquer resolução
// (600x480 no itch.io, tela cheia 16:9, etc.). Assim nenhuma parte da arena fica fora da tela.
[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    public float width = 32f;   // largura (em unidades) que precisa aparecer
    public float height = 18f;  // altura (em unidades) que precisa aparecer

    Camera cam;

    void Awake()
    {
        Fit();
    }

    void Update()
    {
        Fit(); // a janela pode mudar de tamanho (ex.: botão de tela cheia)
    }

    public void Fit()
    {
        if (cam == null) cam = GetComponent<Camera>();
        // orthographicSize = metade da altura visível. Escolhe o que for maior:
        // a altura pedida ou a altura necessária para caber a largura pedida.
        cam.orthographicSize = Mathf.Max(height / 2f, width / 2f / cam.aspect);
    }
}
