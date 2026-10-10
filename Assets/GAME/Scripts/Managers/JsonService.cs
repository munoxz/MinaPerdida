using System;
using System.IO;
using UnityEngine;

// Se encarga SOLO de leer y escribir archivos JSON.
// Es estático para que el Menú pueda leer el config antes de que exista el GameManager.
public static class JsonService
{
    public static ConfigData Config { get; private set; }
    public static string Error { get; private set; }

    public static string RutaConfig => Path.Combine(Application.streamingAssetsPath, "config.json");
    public static string RutaResumen => Application.persistentDataPath + "/resumen_partida.json"; // con '/' para que se muestre bien en pantalla

    // Lee config.json. Si falla, NO cierra el juego: guarda el error y devuelve false.
    public static bool CargarConfig()
    {
        Config = null;
        Error = null;

        try
        {
            if (!File.Exists(RutaConfig))
            {
                Error = "No se encontró config.json en StreamingAssets.";
                return false;
            }

            string texto = File.ReadAllText(RutaConfig);
            ConfigData datos = JsonUtility.FromJson<ConfigData>(texto);

            if (datos == null || datos.jugador == null || datos.recursos == null ||
                datos.peligros == null || datos.requisitoJefe == null || datos.jefe == null)
            {
                Error = "config.json está incompleto o tiene un formato inválido.";
                return false;
            }

            Config = datos;
            Debug.Log("config.json cargado. Jugador: " + Config.jugador.nombre);
            return true;
        }
        catch (Exception e)
        {
            Error = "config.json está dañado: " + e.Message;
            Debug.LogWarning(Error);
            return false;
        }
    }

    // Escribe resumen_partida.json y devuelve la ruta donde quedó guardado
    public static string GuardarResumen(ResumenPartida resumen)
    {
        string json = JsonUtility.ToJson(resumen, true);
        File.WriteAllText(RutaResumen, json);
        Debug.Log("Resumen guardado en: " + RutaResumen);
        return RutaResumen;
    }
}
