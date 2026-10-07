using UnityEngine;

// Enemigo que patrulla entre dos puntos. Su velocidad viene de config.json (a través de Peligro).
[RequireComponent(typeof(Peligro))]
public class EnemigoPatrulla : MonoBehaviour
{
    public Transform puntoA;
    public Transform puntoB;

    private Peligro peligro;
    private SpriteRenderer sr;
    private Vector3 a, b, destino;

    void Start()
    {
        peligro = GetComponent<Peligro>();
        sr = GetComponent<SpriteRenderer>();
        // Se guardan las posiciones porque los puntos pueden ser hijos del enemigo
        a = puntoA.position;
        b = puntoB.position;
        destino = b;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, destino, peligro.Velocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino) < 0.05f)
            destino = (destino == b) ? a : b;

        if (sr != null) sr.flipX = destino.x < transform.position.x;
    }
}
