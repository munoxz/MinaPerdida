using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Pantalla final con las estadísticas de la partida (lee el resumen del GameManager).
public class PanelEstadisticas : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoTabla;
    public TextMeshProUGUI textoDetalle;
    public TextMeshProUGUI textoRuta;
    public Button botonMenu;
    public string escenaMenu = "Menu";

    void Start()
    {
        panel.SetActive(false);
        botonMenu.onClick.AddListener(() => SceneManager.LoadScene(escenaMenu));
    }

    public void Mostrar(ResumenPartida r, string ruta)
    {
        panel.SetActive(true);
        textoTitulo.text = r.jugador + "  —  " + r.resultado.ToUpper();

        EscenaResumen mina = Buscar(r, "Mina");
        EscenaResumen cri = Buscar(r, "Criatura");

        StringBuilder t = new StringBuilder();
        t.AppendLine("<b>Dato<pos=45%>Mina<pos=63%>Criatura<pos=82%>Total</b>");
        t.AppendLine("Tiempo<pos=45%>" + mina.tiempo.ToString("0.0") + " s<pos=63%>" + cri.tiempo.ToString("0.0") + " s<pos=82%>" + r.tiempoTotal.ToString("0.0") + " s");
        t.AppendLine("Puntaje<pos=45%>" + mina.puntaje + "<pos=63%>" + cri.puntaje + "<pos=82%>" + r.puntajeTotal);
        t.AppendLine("Objetos recolectados<pos=45%>" + mina.objetos + "<pos=63%>" + cri.objetos + "<pos=82%>" + r.totalObjetos);
        t.AppendLine("Golpes recibidos<pos=45%>" + mina.golpes + "<pos=63%>" + cri.golpes + "<pos=82%>" + r.golpesRecibidos);
        t.AppendLine("Muertes<pos=45%>" + mina.muertes + "<pos=63%>" + cri.muertes + "<pos=82%>" + r.muertes.total);
        textoTabla.text = t.ToString();

        StringBuilder d = new StringBuilder();
        d.Append("<b>Recursos:</b> ");
        foreach (RecursoCantidad rc in r.recursos) d.Append(rc.tipo + " " + rc.cantidad + "   ");
        d.AppendLine();
        d.AppendLine("<b>Muertes por causa:</b> caída " + r.muertes.caida + "   enemigo " + r.muertes.enemigo +
                     "   obstáculo " + r.muertes.obstaculo + "   jefe " + r.muertes.jefe);
        d.AppendLine("<b>Checkpoints activados:</b> " + r.checkpoints);
        textoDetalle.text = d.ToString();

        textoRuta.text = "resumen_partida.json guardado en:\n" + ruta;
    }

    EscenaResumen Buscar(ResumenPartida r, string nombre)
    {
        foreach (EscenaResumen e in r.escenas) if (e.nombre == nombre) return e;
        return new EscenaResumen { nombre = nombre };
    }
}
