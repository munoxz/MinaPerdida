using UnityEngine;

// Mecanismo que se acciona con el Raycast frontal (tecla E).
// Gasta una batería y ENCOLA el evento de activar la plataforma (no la activa directamente).
public class Palanca : MonoBehaviour, IInteractuable
{
    public PlataformaMovil plataforma;
    [Tooltip("Tipo de recurso que necesita (de config.json)")]
    public string tipoRequerido = "bateria";
    public Sprite spriteApagada;
    public Sprite spriteEncendida;

    private bool activada = false;
    private SpriteRenderer sr;

    public string TextoAyuda => activada ? "" : "Presiona E para usar una " + tipoRequerido + " en la palanca";

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null && spriteApagada != null) sr.sprite = spriteApagada;
    }

    public void Interactuar(PlayerController jugador)
    {
        if (activada) return;

        if (GameManager.Instance == null || !GameManager.Instance.UsarRecurso(tipoRequerido))
        {
            Mensaje("Necesitas una " + tipoRequerido + " para activar la palanca");
            return;
        }

        activada = true;
        if (sr != null && spriteEncendida != null) sr.sprite = spriteEncendida;

        // Queue: se encolan los eventos y el procesador los atiende en orden
        ColaEventos cola = FindFirstObjectByType<ColaEventos>();
        if (cola != null)
        {
            cola.Encolar("Mostrar mensaje", () => Mensaje("¡Batería conectada!"), 1f);
            cola.Encolar("Activar plataforma", () => plataforma.Activar(), 0.2f);
            cola.Encolar("Mostrar mensaje", () => Mensaje("La plataforma está en movimiento"), 0.5f);
        }
        else plataforma.Activar();
    }

    void Mensaje(string texto)
    {
        if (HUDController.Instance != null) HUDController.Instance.MostrarMensaje(texto, 2.5f);
    }
}
