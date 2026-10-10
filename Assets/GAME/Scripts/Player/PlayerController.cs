using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Movimiento, salto con Raycast de suelo, Raycast frontal para interactuar,
// mejoras temporales y animaciones del personaje.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Raycast de suelo")]
    public float distanciaSuelo = 0.1f;

    [Header("Raycast frontal")]
    public float distanciaInteraccion = 1.2f;

    [Header("Ataque con espada (tecla Z)")]
    public float tiempoEntreAtaques = 0.45f;
    public float retrasoGolpe = 0.12f;          // espera al cuadro en que la espada baja
    public Vector2 tamanoGolpe = new Vector2(1.2f, 1.0f);
    private float esperaAtaque;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Animator anim;

    // Valores que vienen de config.json
    private float velocidadBase;
    private float saltoBase;

    // Multiplicadores de las mejoras temporales (1 = sin mejora)
    private float multVelocidad = 1f;
    private float multSalto = 1f;

    private float moveX;
    private bool pedirSalto;
    private int direccion = 1; // 1 derecha, -1 izquierda

    public bool EnSuelo { get; private set; }
    public bool PuedeMoverse { get; set; } = true;
    public string MejoraActiva { get; private set; } = "";
    public float TiempoMejora { get; private set; }
    public IInteractuable InteractuableCerca { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (JsonService.Config == null) JsonService.CargarConfig();
        if (JsonService.Config != null)
        {
            velocidadBase = JsonService.Config.jugador.velocidad;
            saltoBase = JsonService.Config.jugador.fuerzaSalto;
        }
    }

    void Update()
    {
        moveX = PuedeMoverse ? LeerHorizontal() : 0f;

        if (moveX > 0) direccion = 1;
        else if (moveX < 0) direccion = -1;
        if (sr != null) sr.flipX = direccion < 0;

        EnSuelo = DetectarSuelo();

        // Solo se puede saltar si el Raycast detecta suelo
        if (PuedeMoverse && LeerSalto() && EnSuelo) pedirSalto = true;

        DetectarInteractuable();
        if (PuedeMoverse && InteractuableCerca != null && LeerInteractuar())
            InteractuableCerca.Interactuar(this);

        // Ataque con espada
        if (esperaAtaque > 0) esperaAtaque -= Time.deltaTime;
        if (PuedeMoverse && LeerAtaque() && esperaAtaque <= 0) Atacar();

        // Cuenta regresiva de la mejora temporal
        if (TiempoMejora > 0)
        {
            TiempoMejora -= Time.deltaTime;
            if (TiempoMejora <= 0) QuitarMejora();
        }

        if (anim != null)
        {
            anim.SetFloat("Velocidad", Mathf.Abs(moveX));
            anim.SetBool("EnSuelo", EnSuelo);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * velocidadBase * multVelocidad, rb.linearVelocity.y);

        if (pedirSalto)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, saltoBase * multSalto);
            pedirSalto = false;
            AudioManager.Sonar(a => a.salto);
        }
    }

    // ===================== RAYCAST DE SUELO =====================
    // Lanza 3 rayos hacia abajo (izquierda, centro y derecha de los pies).
    bool DetectarSuelo()
    {
        Bounds b = col.bounds;
        float[] xs = { b.min.x + 0.05f, b.center.x, b.max.x - 0.05f };

        foreach (float x in xs)
        {
            Vector2 origen = new Vector2(x, b.min.y - 0.01f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origen, Vector2.down, distanciaSuelo);
            bool toca = false;
            foreach (RaycastHit2D h in hits)
            {
                if (h.collider != col && !h.collider.isTrigger) { toca = true; break; }
            }
            Debug.DrawRay(origen, Vector2.down * distanciaSuelo, toca ? Color.green : Color.red);
            if (toca) return true;
        }
        return false;
    }

    // ===================== RAYCAST FRONTAL =====================
    // Busca al frente un objeto que implemente IInteractuable (palanca, interruptor...)
    void DetectarInteractuable()
    {
        Vector2 origen = col.bounds.center;
        Vector2 dir = Vector2.right * direccion;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origen, dir, distanciaInteraccion);
        Debug.DrawRay(origen, dir * distanciaInteraccion, Color.cyan);

        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == col) continue;
            IInteractuable i = h.collider.GetComponentInParent<IInteractuable>();
            if (i != null) { InteractuableCerca = i; return; }
        }
        InteractuableCerca = null;
    }

    // ===================== MEJORAS TEMPORALES =====================
    // efecto, valor y duración vienen del recurso en config.json
    public void AplicarMejora(string efecto, float valor, float duracion)
    {
        QuitarMejora();
        if (efecto == "velocidad") multVelocidad = valor;
        else if (efecto == "salto") multSalto = valor;
        else return;

        MejoraActiva = efecto;
        TiempoMejora = duracion;
    }

    void QuitarMejora()
    {
        multVelocidad = 1f;
        multSalto = 1f;
        MejoraActiva = "";
        TiempoMejora = 0f;
    }

    // Lo usa PlayerVida al reaparecer en un checkpoint
    public void Reaparecer(Vector2 posicion)
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = posicion;
    }

    // ===================== ATAQUE =====================
    void Atacar()
    {
        esperaAtaque = tiempoEntreAtaques;
        if (anim != null) anim.SetTrigger("Atacar");
        AudioManager.Sonar(a => a.espada);
        Invoke(nameof(AplicarGolpe), retrasoGolpe);
    }

    // Revisa con una caja lo que hay al frente y le hace daño al jefe si está ahí
    void AplicarGolpe()
    {
        Vector2 centro = (Vector2)col.bounds.center + new Vector2(direccion * (col.bounds.extents.x + tamanoGolpe.x / 2f), 0f);
        foreach (Collider2D c in Physics2D.OverlapBoxAll(centro, tamanoGolpe, 0f))
        {
            Jefe jefe = c.GetComponentInParent<Jefe>();
            if (jefe != null) { jefe.RecibirAtaqueEspada(); return; }
        }
    }

    // ===================== ENTRADAS (sistema nuevo y viejo) =====================
    float LeerHorizontal()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return 0f;
        float x = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        return x;
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    bool LeerSalto()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#endif
    }

    bool LeerAtaque()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.zKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Z);
#endif
    }

    bool LeerInteractuar()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        return kb != null && kb.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
