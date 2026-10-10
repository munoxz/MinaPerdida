using UnityEngine;

// Fogata de checkpoint: al tocarla se enciende y se apila en el Stack del GameManager.
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Nombre único del checkpoint (ej: Mina_1, Criatura_2)")]
    public string idCheckpoint = "Mina_1";
    public Color colorApagado = new Color(0.35f, 0.35f, 0.4f);
    public Color colorActivado = Color.white;

    private bool activado = false;
    private SpriteRenderer sr;
    private Animator anim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (sr != null) sr.color = colorApagado;
        if (anim != null) anim.enabled = false;   // apagada: sin animación
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (activado || c.GetComponent<PlayerVida>() == null) return;

        activado = true;
        if (GameManager.Instance != null)
            GameManager.Instance.ApilarCheckpoint(idCheckpoint, transform.position);

        if (sr != null) sr.color = colorActivado;
        if (anim != null) anim.enabled = true;
        AudioManager.Sonar(a => a.checkpoint);    // encendida: el fuego se anima
        if (HUDController.Instance != null) HUDController.Instance.MostrarMensaje("Checkpoint activado", 1.5f);
    }
}
