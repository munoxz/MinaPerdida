using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Corazones, invulnerabilidad con parpadeo, muerte y reaparición en el checkpoint.
public class PlayerVida : MonoBehaviour
{
    public int VidasMax { get; private set; }
    public int VidasActuales { get; private set; }
    public bool Invulnerable { get; private set; }

    // Aviso para otros scripts (ej: el jefe se reinicia cuando el jugador muere)
    public event Action<string> AlMorir;

    private float tiempoInvulnerable;
    private Vector2 puntoInicio;
    private SpriteRenderer sr;
    private PlayerController pc;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        pc = GetComponent<PlayerController>();
    }

    void Start()
    {
        if (JsonService.Config == null) JsonService.CargarConfig();
        if (JsonService.Config != null)
        {
            VidasMax = JsonService.Config.jugador.vidas;
            tiempoInvulnerable = JsonService.Config.jugador.invulnerabilidad;
        }
        VidasActuales = VidasMax;
        puntoInicio = transform.position;
    }

    // Lo llaman los peligros (enemigos, obstáculos, jefe)
    public void RecibirDano(int dano, string causa)
    {
        if (Invulnerable) return;

        VidasActuales = Mathf.Max(0, VidasActuales - dano);
        if (GameManager.Instance != null) GameManager.Instance.RegistrarGolpe(causa);

        if (VidasActuales <= 0) Morir(causa);
        else { AudioManager.Sonar(a => a.golpe); StartCoroutine(Invulnerabilidad()); }
    }

    // Muerte por daño o por caída (la caída mata sin importar los corazones)
    public void Morir(string causa)
    {
        if (GameManager.Instance != null) GameManager.Instance.RegistrarMuerte(causa);

        // Reaparece en el checkpoint del tope del Stack (si es de esta escena)
        Vector2 destino = puntoInicio;
        if (GameManager.Instance != null)
        {
            CheckpointData cp = GameManager.Instance.CheckpointActual();
            if (cp != null && cp.escena == SceneManager.GetActiveScene().name)
                destino = cp.posicion;
        }

        AudioManager.Sonar(a => a.muerte);
        pc.Reaparecer(destino);
        VidasActuales = VidasMax; // corazones completos

        StopAllCoroutines();
        StartCoroutine(Invulnerabilidad());

        Debug.Log("Muerte por: " + causa);
        AlMorir?.Invoke(causa);
    }

    // Inmunidad de jugador.invulnerabilidad segundos, con parpadeo del sprite
    IEnumerator Invulnerabilidad()
    {
        Invulnerable = true;
        float t = 0f;
        while (t < tiempoInvulnerable)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        sr.enabled = true;
        Invulnerable = false;
    }
}
