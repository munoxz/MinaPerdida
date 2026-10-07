using UnityEngine;

// Salida de la Mina. Solo funciona si el inventario cumple requisitoJefe del JSON.
public class Elevador : MonoBehaviour
{
    public float esperaAntesDeSalir = 1.5f;
    private bool activado = false;

    void OnTriggerEnter2D(Collider2D c)
    {
        if (activado || c.GetComponent<PlayerController>() == null) return;
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.CumpleRequisitoJefe(out string faltantes))
        {
            activado = true;
            c.GetComponent<PlayerController>().PuedeMoverse = false;

            MinaController mina = FindFirstObjectByType<MinaController>();
            ColaEventos cola = FindFirstObjectByType<ColaEventos>();
            cola.Encolar("Mostrar mensaje", () => Mensaje("¡El elevador se activó!\nBajando al territorio de la criatura..."), esperaAntesDeSalir);
            cola.Encolar("Cambiar de escena", () => mina.SalirDeLaMina(), 0f);
        }
        else
        {
            Mensaje("El elevador no funciona.\n" + faltantes.Trim().Replace("\n", "   "));
        }
    }

    void Mensaje(string texto)
    {
        if (HUDController.Instance != null) HUDController.Instance.MostrarMensaje(texto, 3f);
    }
}
