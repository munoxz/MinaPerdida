using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Queue de eventos: los disparadores ENCOLAN y este procesador los atiende
// uno por uno (Dequeue). Los eventos no se ejecutan directamente.
// Va un objeto con este script en cada escena de juego.
public class ColaEventos : MonoBehaviour
{
    public class EventoJuego
    {
        public string nombre;
        public Action accion;
        public float espera; // segundos antes de atender el siguiente
    }

    private Queue<EventoJuego> cola = new Queue<EventoJuego>();
    private bool procesando = false;

    public int Pendientes => cola.Count;

    public void Encolar(string nombre, Action accion, float espera = 0.5f)
    {
        cola.Enqueue(new EventoJuego { nombre = nombre, accion = accion, espera = espera });
        Debug.Log("Evento encolado: " + nombre + " (pendientes: " + cola.Count + ")");

        if (!procesando) StartCoroutine(Procesar());
    }

    private IEnumerator Procesar()
    {
        procesando = true;
        while (cola.Count > 0)
        {
            EventoJuego evento = cola.Dequeue();
            Debug.Log("Procesando evento: " + evento.nombre);
            evento.accion?.Invoke();
            yield return new WaitForSeconds(evento.espera);
        }
        procesando = false;
    }
}
