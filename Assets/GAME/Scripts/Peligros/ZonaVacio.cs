using UnityEngine;

// Zona de caída: mata de inmediato, sin importar los corazones.
public class ZonaVacio : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D c)
    {
        PlayerVida vida = c.GetComponent<PlayerVida>();
        if (vida != null) vida.Morir(Causas.Caida);
    }
}
