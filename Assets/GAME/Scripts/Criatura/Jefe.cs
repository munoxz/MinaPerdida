using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// El jefe de la guarida. Vida, fases, velocidad y daño vienen de config.json (jefe).
// Se le hace daño con la espada del jugador (tecla Z).
public class Jefe : MonoBehaviour
{
    [Header("Puntos de la guarida (mínimo 3)")]
    public Transform[] puntos;

    [Header("Barra de vida")]
    public GameObject panelBarra;
    public Image rellenoBarra;
    public TextMeshProUGUI textoFase;

    [Header("Comportamiento por fase")]
    public Color[] coloresFase = { Color.white, new Color(1f, 0.7f, 0.4f), new Color(1f, 0.35f, 0.35f) };
    public float pausaFase1 = 1.2f;
    public float pausaFase2 = 0.3f;
    public float invulnerableTrasGolpe = 0.6f;

    [Tooltip("Marcar si el dibujo original mira hacia la izquierda")]
    public bool spriteMiraIzquierda = false;

    private JefeConfig cfg;
    private int vidaMax, vida, fase;          // fase: 0, 1, 2 (se muestran como 1, 2, 3)
    private bool activo, derrotado, invulnerable;
    private int indice = 0, sentido = 1;
    private float pausa;
    private Vector3 posicionInicial;

    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;
    private PlayerVida jugadorVida;

    public bool CombateActivo => activo;
    public bool Derrotado => derrotado;

    void Start()
    {
        if (JsonService.Config == null) JsonService.CargarConfig();
        cfg = JsonService.Config.jefe;
        vidaMax = cfg.vida;
        vida = vidaMax;

        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        jugadorVida = FindFirstObjectByType<PlayerVida>();
        jugadorVida.AlMorir += AlMorirJugador;

        posicionInicial = transform.position;
        panelBarra.SetActive(false);
        if (anim != null) anim.enabled = false;
        AplicarFase();
    }

    void OnDestroy()
    {
        if (jugadorVida != null) jugadorVida.AlMorir -= AlMorirJugador;
    }

    // La llama la cola de eventos cuando el jugador entra a la guarida
    public void IniciarCombate()
    {
        if (derrotado || activo) return;
        activo = true;
        panelBarra.SetActive(true);
        ActualizarBarra();
        if (anim != null) anim.enabled = true;
    }

    void Update()
    {
        if (!activo) return;
        float velocidad = cfg.velocidadPorFase[fase];
        Vector3 objetivo;

        if (fase == 2)
        {
            // Fase 3: persigue al jugador (sin salirse de la guarida)
            float x = Mathf.Clamp(jugadorVida.transform.position.x, puntos[0].position.x, puntos[puntos.Length - 1].position.x);
            objetivo = new Vector3(x, puntos[0].position.y, 0);
        }
        else
        {
            // Fases 1 y 2: recorre los puntos, con pausas más cortas en la fase 2
            if (pausa > 0) { pausa -= Time.deltaTime; return; }
            objetivo = puntos[indice].position;
            if (Vector3.Distance(transform.position, objetivo) < 0.05f)
            {
                if (indice == puntos.Length - 1) sentido = -1;
                else if (indice == 0) sentido = 1;
                indice += sentido;
                pausa = fase == 0 ? pausaFase1 : pausaFase2;
                return;
            }
        }

        float antes = transform.position.x;
        transform.position = Vector3.MoveTowards(transform.position, objetivo, velocidad * Time.deltaTime);
        float dx = transform.position.x - antes;
        if (Mathf.Abs(dx) > 0.0001f) sr.flipX = spriteMiraIzquierda ? dx > 0 : dx < 0;
    }

    void OnTriggerStay2D(Collider2D c)
    {
        if (!activo) return;
        PlayerVida pv = c.GetComponent<PlayerVida>();
        if (pv == null) return;

        // Tocar al jefe hace daño según la fase actual (danoPorFase del JSON)
        pv.RecibirDano(cfg.danoPorFase[fase], Causas.Jefe);
    }

    // La llama el jugador cuando su espada golpea al jefe (daño = danoEspada del JSON)
    public void RecibirAtaqueEspada()
    {
        RecibirDano(cfg.danoEspada);
    }

    public void RecibirDano(int dano)
    {
        if (invulnerable || !activo) return;

        vida = Mathf.Max(0, vida - dano);
        ActualizarBarra();
        if (vida <= 0) { Morir(); return; }

        AudioManager.Sonar(a => a.golpeJefe);
        StartCoroutine(Parpadeo());

        // Cambio de fase según los umbrales del JSON (100, 66, 33)
        float porcentaje = vida * 100f / vidaMax;
        int nueva = porcentaje <= cfg.umbralesFase[2] ? 2 : porcentaje <= cfg.umbralesFase[1] ? 1 : 0;
        if (nueva != fase)
        {
            fase = nueva;
            int f = fase + 1;
            // Queue: el cambio de fase se encola como evento
            FindFirstObjectByType<ColaEventos>().Encolar("Cambio de fase del jefe", () =>
            {
                AplicarFase();
                Mensaje("¡La criatura entra en la fase " + f + "!");
            }, 0.5f);
        }
    }

    IEnumerator Parpadeo()
    {
        invulnerable = true;
        for (int i = 0; i < 4; i++)
        {
            sr.color = Color.white * 0.4f + Color.red * 0.6f;
            yield return new WaitForSeconds(invulnerableTrasGolpe / 8f);
            sr.color = coloresFase[fase];
            yield return new WaitForSeconds(invulnerableTrasGolpe / 8f);
        }
        invulnerable = false;
    }

    void AplicarFase()
    {
        if (sr != null) sr.color = coloresFase[fase];
        if (textoFase != null) textoFase.text = "Fase " + (fase + 1);
    }

    void ActualizarBarra()
    {
        rellenoBarra.fillAmount = (float)vida / vidaMax;
    }

    void Morir()
    {
        activo = false;
        derrotado = true;
        StopAllCoroutines();
        col.enabled = false;
        if (anim != null) anim.enabled = false;
        sr.color = new Color(1, 1, 1, 0.4f);

        AudioManager.Sonar(a => a.victoria);
        if (GameManager.Instance != null) GameManager.Instance.AgregarPuntos(cfg.puntosVictoria);
        Mensaje("¡Derrotaste a la criatura!  +" + cfg.puntosVictoria);

        CriaturaController controlador = FindFirstObjectByType<CriaturaController>();
        if (controlador != null) controlador.Victoria();
    }

    // Si el jugador muere durante el combate, el jefe reinicia su vida y su fase
    void AlMorirJugador(string causa)
    {
        if (!activo) return;
        StopAllCoroutines();
        activo = false;
        invulnerable = false;
        vida = vidaMax;
        fase = 0;
        indice = 0; sentido = 1; pausa = 0;
        transform.position = posicionInicial;
        panelBarra.SetActive(false);
        if (anim != null) anim.enabled = false;
        AplicarFase();
        Mensaje("La criatura recuperó toda su vida");
    }

    void Mensaje(string texto)
    {
        if (HUDController.Instance != null) HUDController.Instance.MostrarMensaje(texto, 2.5f);
    }
}
