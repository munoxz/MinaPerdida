using System.Collections.Generic;
using UnityEngine;

// Plataforma que va y viene entre dos puntos. Puede empezar apagada y activarse con una palanca.
[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour
{
    public Transform puntoA;
    public Transform puntoB;
    public float velocidad = 1.5f;
    public bool activa = true;
    public float esperaEnExtremos = 0.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 a, b, destino;
    private float espera;
    private List<Rigidbody2D> pasajeros = new List<Rigidbody2D>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        col = GetComponent<Collider2D>();
        a = puntoA.position;
        b = puntoB.position;
        destino = b;
    }

    void FixedUpdate()
    {
        if (!activa) return;
        if (espera > 0) { espera -= Time.fixedDeltaTime; return; }

        Vector2 antes = rb.position;
        Vector2 nueva = Vector2.MoveTowards(antes, destino, velocidad * Time.fixedDeltaTime);
        rb.MovePosition(nueva);

        // Mueve también a quien esté parado encima
        Vector2 delta = nueva - antes;
        foreach (Rigidbody2D p in pasajeros)
            if (p != null) p.position += delta;

        if (Vector2.Distance(nueva, destino) < 0.01f)
        {
            destino = destino == b ? a : b;
            espera = esperaEnExtremos;
        }
    }

    // La llama la palanca (a través de la cola de eventos)
    public void Activar()
    {
        activa = true;
    }

    void OnCollisionStay2D(Collision2D c)
    {
        // Solo cuenta como pasajero si está encima de la plataforma
        bool encima = c.collider.bounds.min.y >= col.bounds.max.y - 0.1f;
        Rigidbody2D r = c.rigidbody;
        if (r == null) return;
        if (encima && !pasajeros.Contains(r)) pasajeros.Add(r);
        if (!encima) pasajeros.Remove(r);
    }

    void OnCollisionExit2D(Collision2D c)
    {
        if (c.rigidbody != null) pasajeros.Remove(c.rigidbody);
    }
}
