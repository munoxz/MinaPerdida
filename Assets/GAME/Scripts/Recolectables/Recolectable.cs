using UnityEngine;

// Objeto que se recoge con un trigger. Su tipo, puntos y efecto vienen de config.json.
public class Recolectable : MonoBehaviour
{
    [Tooltip("id del recurso en config.json (hierro, cobre, bateria, fragmento)")]
    public string idRecurso = "hierro";

    [Header("Animación de flotar")]
    public float alturaFlotar = 0.08f;
    public float velocidadFlotar = 3f;

    private RecursoData datos;
    private Vector3 posicionInicial;
    private bool recogido = false;

    void Start()
    {
        if (JsonService.Config == null) JsonService.CargarConfig();
        datos = JsonService.Config != null ? JsonService.Config.BuscarRecurso(idRecurso) : null;
        if (datos == null) Debug.LogWarning("No existe el recurso '" + idRecurso + "' en config.json");

        posicionInicial = transform.position;
    }

    void Update()
    {
        // Flota suavemente para que se note que se puede recoger
        transform.position = posicionInicial + Vector3.up * Mathf.Sin(Time.time * velocidadFlotar) * alturaFlotar;
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        if (recogido || datos == null) return;

        PlayerController jugador = c.GetComponent<PlayerController>();
        if (jugador == null) return;
        recogido = true;

        // Inventario (List), cantidad por tipo (Dictionary) y puntaje
        if (GameManager.Instance != null) GameManager.Instance.RegistrarRecoleccion(datos);

        // Efecto del recurso según el JSON
        string mensaje = "+" + datos.puntos + "  " + datos.id;
        if (datos.efecto == "velocidad" || datos.efecto == "salto")
        {
            jugador.AplicarMejora(datos.efecto, datos.valor, datos.duracion);
            mensaje += "\n¡Mejora de " + datos.efecto + " x" + datos.valor + " por " + datos.duracion + " s!";
        }
        else if (datos.efecto == "activar")
        {
            mensaje += "\nÚsala en una palanca (tecla E)";
        }

        if (HUDController.Instance != null) HUDController.Instance.MostrarMensaje(mensaje, 2f);

        // Desaparece de la escena
        Destroy(gameObject);
    }
}
