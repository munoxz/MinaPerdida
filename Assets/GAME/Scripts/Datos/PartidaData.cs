using System;
using System.Collections.Generic;
using UnityEngine;

// Causas de golpes y muertes (se usan en el registro y en las estadísticas)
public static class Causas
{
    public const string Caida = "caida";
    public const string Enemigo = "enemigo";
    public const string Obstaculo = "obstaculo";
    public const string Jefe = "jefe";
}

// Un objeto recogido (para el inventario: List)
[Serializable]
public class RegistroRecoleccion
{
    public string id;
    public string tipo;
    public string escena;
    public float momento;

    public RegistroRecoleccion(string id, string tipo, string escena, float momento)
    {
        this.id = id; this.tipo = tipo; this.escena = escena; this.momento = momento;
    }
}

// Un golpe o una muerte (para el historial: List)
[Serializable]
public class RegistroEvento
{
    public string tipoEvento; // "golpe" o "muerte"
    public string causa;
    public string escena;
    public float tiempo;

    public RegistroEvento(string tipoEvento, string causa, string escena, float tiempo)
    {
        this.tipoEvento = tipoEvento; this.causa = causa; this.escena = escena; this.tiempo = tiempo;
    }
}

// Un checkpoint activado (para el historial de checkpoints: Stack)
[Serializable]
public class CheckpointData
{
    public string id;
    public string escena;
    public Vector2 posicion;
}

// ===== Clases del JSON de salida (resumen_partida.json) =====

[Serializable]
public class EscenaResumen
{
    public string nombre;
    public float tiempo;
    public int puntaje;
    public int objetos;
    public int golpes;
    public int muertes;
}

[Serializable]
public class RecursoCantidad
{
    public string tipo;
    public int cantidad;
}

[Serializable]
public class MuertesResumen
{
    public int total;
    public int caida;
    public int enemigo;
    public int obstaculo;
    public int jefe;
}

[Serializable]
public class ResumenPartida
{
    public string jugador;
    public string resultado;
    public int puntajeTotal;
    public float tiempoTotal;
    public List<EscenaResumen> escenas = new List<EscenaResumen>();
    public List<RecursoCantidad> recursos = new List<RecursoCantidad>();
    public int totalObjetos;
    public int checkpoints;
    public int golpesRecibidos;
    public MuertesResumen muertes = new MuertesResumen();
}
