using System;
using System.Collections.Generic;

// Clases que representan el archivo config.json.
// IMPORTANTE: los nombres de las variables deben ser IGUALES a los del JSON,
// porque JsonUtility los busca por nombre.

[Serializable]
public class ConfigData
{
    public JugadorConfig jugador;
    public List<RequisitoData> requisitoJefe;
    public List<RecursoData> recursos;
    public List<PeligroData> peligros;
    public JefeConfig jefe;

    // Busca un recurso por su id (ej: "hierro")
    public RecursoData BuscarRecurso(string id)
    {
        foreach (RecursoData r in recursos)
            if (r.id == id) return r;
        return null;
    }

    // Busca un peligro por su id (ej: "murcielago")
    public PeligroData BuscarPeligro(string id)
    {
        foreach (PeligroData p in peligros)
            if (p.id == id) return p;
        return null;
    }
}

[Serializable]
public class JugadorConfig
{
    public string nombre;
    public int vidas;
    public float velocidad;
    public float fuerzaSalto;
    public float invulnerabilidad;
}

[Serializable]
public class RequisitoData
{
    public string tipo;
    public int cantidad;
}

[Serializable]
public class RecursoData
{
    public string id;
    public string tipo;
    public int puntos;
    public string efecto;
    public float valor;
    public float duracion;
}

[Serializable]
public class PeligroData
{
    public string id;
    public string tipo;
    public int dano;
    public float velocidad;
}

[Serializable]
public class JefeConfig
{
    public int vida;
    public List<float> umbralesFase;
    public List<float> velocidadPorFase;
    public List<int> danoPorFase;
    public int puntosVictoria;
}
