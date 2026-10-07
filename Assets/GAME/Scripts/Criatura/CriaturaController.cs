using UnityEngine;

// Lógica de la escena Criatura únicamente. Consulta y envía datos al GameManager.
public class CriaturaController : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.IniciarCronometro();
        else
            Debug.LogWarning("No hay GameManager: inicia el juego desde el Menú para que se registren los datos.");
    }
}
