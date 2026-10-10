using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Muestra en pantalla los datos del jugador y de la partida.
// Solo LEE datos (del GameManager y del jugador), no guarda nada.
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Corazones")]
    public Transform contenedorCorazones;
    public Sprite corazonLleno;
    public Sprite corazonVacio;
    public float tamanoCorazon = 56f;

    [Header("Textos")]
    public TextMeshProUGUI textoPuntaje;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoRequisitos;
    public TextMeshProUGUI textoMejora;
    public TextMeshProUGUI textoAyuda;
    public TextMeshProUGUI textoMensaje;

    [Header("Inventario (tecla Tab)")]
    public GameObject panelInventario;
    public TextMeshProUGUI textoInventario;

    private PlayerVida vida;
    private PlayerController jugador;
    private List<Image> corazones = new List<Image>();
    private float tiempoMensaje;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        vida = FindFirstObjectByType<PlayerVida>();
        jugador = FindFirstObjectByType<PlayerController>();
        panelInventario.SetActive(false);
        textoMensaje.text = "";
    }

    void Update()
    {
        ActualizarCorazones();

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            textoPuntaje.text = "Puntaje: " + gm.Puntaje;
            float t = gm.TiempoEscenaActual;
            textoTiempo.text = string.Format("{0:00}:{1:00}", (int)(t / 60), (int)(t % 60));
            textoRequisitos.text = gm.ProgresoRequisitos();
        }
        else
        {
            // Sin GameManager (se abrió la escena sin pasar por el Menú)
            textoPuntaje.text = "Puntaje: -";
            textoRequisitos.text = "Modo prueba: inicia desde el Menú para registrar puntaje y tiempo";
        }

        if (jugador != null)
        {
            textoMejora.text = jugador.MejoraActiva != ""
                ? "Mejora de " + jugador.MejoraActiva + ": " + Mathf.CeilToInt(jugador.TiempoMejora) + " s"
                : "";
            textoAyuda.text = jugador.InteractuableCerca != null ? jugador.InteractuableCerca.TextoAyuda : "";
        }

        if (tiempoMensaje > 0)
        {
            tiempoMensaje -= Time.deltaTime;
            if (tiempoMensaje <= 0) textoMensaje.text = "";
        }

        if (TeclaInventario()) panelInventario.SetActive(!panelInventario.activeSelf);
        if (panelInventario.activeSelf) textoInventario.text = TextoInventario();
    }

    // Crea tantos corazones como vidas máximas (del JSON) y pinta los que quedan
    void ActualizarCorazones()
    {
        if (vida == null) return;

        while (corazones.Count < vida.VidasMax)
        {
            GameObject go = new GameObject("Corazon", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(contenedorCorazones, false);
            Image img = go.GetComponent<Image>();
            img.preserveAspect = true;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(tamanoCorazon, tamanoCorazon);
            corazones.Add(img);
        }

        for (int i = 0; i < corazones.Count; i++)
            corazones[i].sprite = i < vida.VidasActuales ? corazonLleno : corazonVacio;
    }

    // Otros scripts pueden mostrar un mensaje (ej: el elevador o la cola de eventos)
    public void MostrarMensaje(string mensaje, float duracion = 3f)
    {
        textoMensaje.text = mensaje;
        tiempoMensaje = duracion;
    }

    // Arma el texto del inventario a partir del Dictionary y la List del GameManager
    string TextoInventario()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return "Sin datos de partida";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>INVENTARIO</b>  (Tab para cerrar)");
        sb.AppendLine();
        sb.AppendLine("<b>Por tipo:</b>");
        foreach (KeyValuePair<string, int> par in gm.RecursosPorTipo)
            sb.AppendLine("  " + par.Key + ": " + par.Value);

        sb.AppendLine();
        sb.AppendLine("<b>Últimos objetos recogidos:</b>");
        int desde = Mathf.Max(0, gm.Inventario.Count - 8);
        for (int i = gm.Inventario.Count - 1; i >= desde; i--)
        {
            RegistroRecoleccion r = gm.Inventario[i];
            sb.AppendLine("  " + r.id + " (" + r.tipo + ") - " + r.escena + ", " + r.momento.ToString("0.0") + " s");
        }
        if (gm.Inventario.Count == 0) sb.AppendLine("  (vacío)");
        return sb.ToString();
    }

    bool TeclaInventario()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && (kb.tabKey.wasPressedThisFrame || kb.iKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I);
#endif
    }
}
