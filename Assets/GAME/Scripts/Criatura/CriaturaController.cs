using System.Collections;
using UnityEngine;

// Lógica de la escena Criatura únicamente. Consulta y envía datos al GameManager.
public class CriaturaController : MonoBehaviour
{
    public PanelEstadisticas panelEstadisticas;
    public float esperaAntesDelPanel = 2f;

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.IniciarCronometro();
        else
            Debug.LogWarning("No hay GameManager: inicia el juego desde el Menú para que se registren los datos.");
    }

    // La llama el jefe al morir
    public void Victoria()
    {
        PlayerController jugador = FindFirstObjectByType<PlayerController>();
        if (jugador != null) jugador.PuedeMoverse = false;

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        gm.DetenerCronometro();                       // se detiene el cronómetro
        string ruta = gm.GuardarResumen("victoria");  // se escribe resumen_partida.json
        StartCoroutine(MostrarPanel(gm.GenerarResumen("victoria"), ruta));
    }

    IEnumerator MostrarPanel(ResumenPartida resumen, string ruta)
    {
        yield return new WaitForSeconds(esperaAntesDelPanel);
        panelEstadisticas.Mostrar(resumen, ruta);
    }
}
