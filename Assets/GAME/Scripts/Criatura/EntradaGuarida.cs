using UnityEngine;

// Al entrar a la guarida se ENCOLA el evento que inicia el combate.
public class EntradaGuarida : MonoBehaviour
{
    public Jefe jefe;
    private bool encolado = false;

    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.GetComponent<PlayerController>() == null) return;
        if (jefe.Derrotado || jefe.CombateActivo || encolado) return;
        encolado = true;

        ColaEventos cola = FindFirstObjectByType<ColaEventos>();
        cola.Encolar("Mostrar mensaje", () =>
        {
            if (HUDController.Instance != null) HUDController.Instance.MostrarMensaje("¡La criatura despertó!", 2f);
        }, 1.2f);
        cola.Encolar("Iniciar combate", () => { jefe.IniciarCombate(); encolado = false; }, 0f);
    }
}
