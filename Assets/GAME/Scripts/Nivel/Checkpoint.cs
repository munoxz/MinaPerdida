using UnityEngine;

// Al tocarlo, se apila en el Stack de checkpoints del GameManager.
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Nombre único del checkpoint (ej: Mina_1, Criatura_2)")]
    public string idCheckpoint = "Mina_1";
    public Color colorActivado = Color.green;

    private bool activado = false;
    private SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (activado || c.GetComponent<PlayerVida>() == null) return;

        activado = true;
        if (GameManager.Instance != null)
            GameManager.Instance.ApilarCheckpoint(idCheckpoint, transform.position);
        if (sr != null) sr.color = colorActivado;
    }
}
