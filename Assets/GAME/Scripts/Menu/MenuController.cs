using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Lógica de la escena Menú únicamente
public class MenuController : MonoBehaviour
{
    public TextMeshProUGUI textoSaludo;
    public TextMeshProUGUI textoError;
    public Button botonJugar;
    public Button botonSalir;
    public string escenaJuego = "Mina";

    void Awake()
    {
        // Si se vuelve al menú después de una partida, se borra el GameManager anterior
        // para que la siguiente partida empiece de cero.
        if (GameManager.Instance != null)
            Destroy(GameManager.Instance.gameObject);

        // Lee config.json al iniciar el juego
        if (JsonService.CargarConfig())
        {
            textoSaludo.text = "¡Hola, " + JsonService.Config.jugador.nombre + "!";
            textoError.gameObject.SetActive(false);
            botonJugar.interactable = true;
        }
        else
        {
            // El juego no se cierra: muestra el error y bloquea Jugar
            textoSaludo.text = "";
            textoError.gameObject.SetActive(true);
            textoError.text = "Error al leer la configuración:\n" + JsonService.Error;
            botonJugar.interactable = false;
        }

        botonJugar.onClick.AddListener(Jugar);
        botonSalir.onClick.AddListener(Salir);
    }

    public void Jugar()
    {
        SceneManager.LoadScene(escenaJuego);
    }

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
