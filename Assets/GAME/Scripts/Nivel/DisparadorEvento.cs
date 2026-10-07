using UnityEngine;

// Al cruzar este punto, ENCOLA eventos en la ColaEventos (no los ejecuta directamente):
// primero un mensaje y luego la aparición de cada objeto (ej: enemigos), uno por uno.
public class DisparadorEvento : MonoBehaviour
{
    [TextArea] public string mensaje = "¡Cuidado!";
    public GameObject[] objetosQueAparecen;
    public float esperaEntreObjetos = 0.6f;

    private bool usado = false;

    void Start()
    {
        foreach (GameObject o in objetosQueAparecen)
            if (o != null) o.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (usado || c.GetComponent<PlayerController>() == null) return;
        usado = true;

        ColaEventos cola = FindFirstObjectByType<ColaEventos>();
        cola.Encolar("Mostrar mensaje", () =>
        {
            if (HUDController.Instance != null) HUDController.Instance.MostrarMensaje(mensaje, 2.5f);
        }, 1f);

        foreach (GameObject o in objetosQueAparecen)
        {
            GameObject objeto = o;
            cola.Encolar("Aparecer " + objeto.name, () => objeto.SetActive(true), esperaEntreObjetos);
        }
    }
}
