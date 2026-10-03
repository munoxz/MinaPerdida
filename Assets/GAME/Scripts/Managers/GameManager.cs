using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

// Guarda TODOS los datos de la partida y sobrevive al cambiar de escena.
// Solo debe existir en la escena Mina.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ConfigData Config { get; private set; }

    // Puntaje total de la partida
    public int Puntaje { get; private set; }

    // List: inventario general (cada objeto recogido)
    public List<RegistroRecoleccion> Inventario { get; private set; } = new List<RegistroRecoleccion>();

    // List: historial de golpes y muertes
    public List<RegistroEvento> Historial { get; private set; } = new List<RegistroEvento>();

    // Dictionary: cantidad recolectada por tipo ("mineral", "bateria", "mapa")
    public Dictionary<string, int> RecursosPorTipo { get; private set; } = new Dictionary<string, int>();

    // Stack: historial de checkpoints (el del tope es donde se reaparece)
    public Stack<CheckpointData> Checkpoints { get; private set; } = new Stack<CheckpointData>();

    // Estadísticas de cada escena (tiempo, puntaje, objetos, golpes, muertes)
    private Dictionary<string, EscenaResumen> estadisticas = new Dictionary<string, EscenaResumen>();
    private List<string> ordenEscenas = new List<string>();

    // Cronómetro de la escena actual
    private bool cronometroActivo = false;
    private string escenaCronometro;

    public string EscenaActual => SceneManager.GetActiveScene().name;
    public float TiempoEscenaActual => ObtenerEscena(EscenaActual).tiempo;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // solo puede existir uno
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Si se probó la Mina directamente sin pasar por el Menú, se carga aquí
        if (JsonService.Config == null) JsonService.CargarConfig();
        Config = JsonService.Config;

        // Inicializa el diccionario con los tipos de recurso del JSON
        if (Config != null)
        {
            foreach (RecursoData r in Config.recursos)
                if (!RecursosPorTipo.ContainsKey(r.tipo)) RecursosPorTipo[r.tipo] = 0;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (cronometroActivo)
            ObtenerEscena(escenaCronometro).tiempo += Time.deltaTime;
    }

    // ===================== ESCENAS Y TIEMPO =====================

    public EscenaResumen ObtenerEscena(string nombre)
    {
        if (!estadisticas.TryGetValue(nombre, out EscenaResumen e))
        {
            e = new EscenaResumen { nombre = nombre };
            estadisticas[nombre] = e;
            ordenEscenas.Add(nombre);
        }
        return e;
    }

    // Lo llama el controlador de cada escena al empezar
    public void IniciarCronometro()
    {
        escenaCronometro = EscenaActual;
        ObtenerEscena(escenaCronometro);
        cronometroActivo = true;
    }

    // Lo llama el controlador al salir de la escena o al ganar
    public void DetenerCronometro()
    {
        cronometroActivo = false;
    }

    // ===================== PUNTAJE Y RECURSOS =====================

    public void AgregarPuntos(int puntos)
    {
        Puntaje += puntos;
        ObtenerEscena(EscenaActual).puntaje += puntos;
    }

    // Lo llama cada recolectable al ser recogido
    public void RegistrarRecoleccion(RecursoData recurso)
    {
        Inventario.Add(new RegistroRecoleccion(recurso.id, recurso.tipo, EscenaActual, TiempoEscenaActual));

        if (!RecursosPorTipo.ContainsKey(recurso.tipo)) RecursosPorTipo[recurso.tipo] = 0;
        RecursosPorTipo[recurso.tipo]++;

        ObtenerEscena(EscenaActual).objetos++;
        AgregarPuntos(recurso.puntos);
    }

    public int CantidadDe(string tipo)
    {
        return RecursosPorTipo.TryGetValue(tipo, out int c) ? c : 0;
    }

    // Compara el Dictionary contra requisitoJefe. Si falta algo, lo dice en "faltantes".
    public bool CumpleRequisitoJefe(out string faltantes)
    {
        StringBuilder sb = new StringBuilder();
        foreach (RequisitoData req in Config.requisitoJefe)
        {
            int tengo = CantidadDe(req.tipo);
            if (tengo < req.cantidad)
                sb.AppendLine("Faltan " + (req.cantidad - tengo) + " de " + req.tipo);
        }
        faltantes = sb.ToString();
        return faltantes.Length == 0;
    }

    // Texto para el HUD, ej: "mineral 3/5   bateria 1/2   mapa 0/3"
    public string ProgresoRequisitos()
    {
        StringBuilder sb = new StringBuilder();
        foreach (RequisitoData req in Config.requisitoJefe)
            sb.Append(req.tipo + " " + CantidadDe(req.tipo) + "/" + req.cantidad + "   ");
        return sb.ToString().TrimEnd();
    }

    // ===================== GOLPES Y MUERTES =====================

    public void RegistrarGolpe(string causa)
    {
        Historial.Add(new RegistroEvento("golpe", causa, EscenaActual, TiempoEscenaActual));
        ObtenerEscena(EscenaActual).golpes++;
    }

    public void RegistrarMuerte(string causa)
    {
        Historial.Add(new RegistroEvento("muerte", causa, EscenaActual, TiempoEscenaActual));
        ObtenerEscena(EscenaActual).muertes++;
    }

    // ===================== CHECKPOINTS =====================

    // Se apila al activar un checkpoint (cada uno se cuenta una sola vez)
    public void ApilarCheckpoint(string id, Vector2 posicion)
    {
        foreach (CheckpointData c in Checkpoints)
            if (c.id == id) return;

        Checkpoints.Push(new CheckpointData { id = id, escena = EscenaActual, posicion = posicion });
        Debug.Log("Checkpoint apilado: " + id + " (total " + Checkpoints.Count + ")");
    }

    // El respawn toma el checkpoint del tope (Peek no lo saca de la pila)
    public CheckpointData CheckpointActual()
    {
        return Checkpoints.Count > 0 ? Checkpoints.Peek() : null;
    }

    // ===================== JSON DE SALIDA =====================

    public ResumenPartida GenerarResumen(string resultado)
    {
        ResumenPartida r = new ResumenPartida();
        r.jugador = Config.jugador.nombre;
        r.resultado = resultado;
        r.puntajeTotal = Puntaje;

        foreach (string nombre in ordenEscenas)
        {
            EscenaResumen e = estadisticas[nombre];
            r.escenas.Add(e);
            r.tiempoTotal += e.tiempo;
        }

        // El Dictionary no se puede serializar: se convierte a lista de pares
        foreach (KeyValuePair<string, int> par in RecursosPorTipo)
            r.recursos.Add(new RecursoCantidad { tipo = par.Key, cantidad = par.Value });

        r.totalObjetos = Inventario.Count;
        r.checkpoints = Checkpoints.Count;

        foreach (RegistroEvento ev in Historial)
        {
            if (ev.tipoEvento == "golpe") r.golpesRecibidos++;
            else
            {
                r.muertes.total++;
                if (ev.causa == Causas.Caida) r.muertes.caida++;
                else if (ev.causa == Causas.Enemigo) r.muertes.enemigo++;
                else if (ev.causa == Causas.Obstaculo) r.muertes.obstaculo++;
                else if (ev.causa == Causas.Jefe) r.muertes.jefe++;
            }
        }
        return r;
    }

    // Genera y guarda resumen_partida.json. Devuelve la ruta.
    public string GuardarResumen(string resultado)
    {
        return JsonService.GuardarResumen(GenerarResumen(resultado));
    }
}
