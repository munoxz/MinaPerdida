using UnityEngine;
using UnityEngine.SceneManagement;

// Lógica de la escena Mina únicamente. Consulta y envía datos al GameManager.
public class MinaController : MonoBehaviour
{
    public string siguienteEscena = "Criatura";

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.IniciarCronometro();
        else
            Debug.LogWarning("No hay GameManager en la escena Mina");
    }

    // La llamará el elevador cuando se cumpla el requisito
    public void SalirDeLaMina()
    {
        if (GameManager.Instance != null) GameManager.Instance.DetenerCronometro();
        SceneManager.LoadScene(siguienteEscena);
    }
}
