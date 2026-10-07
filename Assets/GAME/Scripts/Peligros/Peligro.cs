using UnityEngine;

// Cualquier cosa que hace daño (enemigo u obstáculo).
// El daño y la velocidad se leen de config.json usando el id.
public class Peligro : MonoBehaviour
{
    [Tooltip("id del peligro en config.json (ej: murcielago, pinchos)")]
    public string idPeligro = "pinchos";

    public int Dano { get; private set; }
    public float Velocidad { get; private set; }
    public string Causa { get; private set; }

    void Awake()
    {
        if (JsonService.Config == null) JsonService.CargarConfig();
        PeligroData datos = JsonService.Config != null ? JsonService.Config.BuscarPeligro(idPeligro) : null;

        if (datos == null)
        {
            Debug.LogWarning("No existe el peligro '" + idPeligro + "' en config.json");
            return;
        }

        Dano = datos.dano;
        Velocidad = datos.velocidad;
        Causa = datos.tipo == "enemigo" ? Causas.Enemigo : Causas.Obstaculo;
    }

    // Stay: si el jugador sigue tocándolo, vuelve a dañar cuando se acabe la invulnerabilidad
    void OnCollisionStay2D(Collision2D c) { Golpear(c.collider); }
    void OnTriggerStay2D(Collider2D c) { Golpear(c); }

    void Golpear(Collider2D c)
    {
        PlayerVida vida = c.GetComponent<PlayerVida>();
        if (vida != null) vida.RecibirDano(Dano, Causa);
    }
}
