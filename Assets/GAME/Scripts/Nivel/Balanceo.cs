using UnityEngine;

// Animación por código para sprites que no traen cuadros de animación:
// respira (se estira y encoge) y se tambalea mientras camina.
public class Balanceo : MonoBehaviour
{
    public float respiracion = 0.05f;
    public float velocidadRespiracion = 3f;
    public float inclinacion = 5f;
    public float velocidadPaso = 12f;

    private Vector3 escalaBase;
    private Vector3 ultimaPosicion;

    void Start()
    {
        escalaBase = transform.localScale;
        ultimaPosicion = transform.position;
    }

    void Update()
    {
        // Respiración: se estira hacia arriba mientras se adelgaza un poco
        float s = 1f + Mathf.Sin(Time.time * velocidadRespiracion) * respiracion;
        transform.localScale = new Vector3(escalaBase.x * (2f - s), escalaBase.y * s, 1f);

        // Tambaleo solo mientras se mueve
        bool moviendo = (transform.position - ultimaPosicion).sqrMagnitude > 0.000001f;
        float angulo = moviendo ? Mathf.Sin(Time.time * velocidadPaso) * inclinacion : 0f;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
        ultimaPosicion = transform.position;
    }
}
