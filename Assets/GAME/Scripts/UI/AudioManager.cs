using System;
using UnityEngine;

// Reproduce los efectos de sonido del juego. Va en el prefab del HUD (Mina y Criatura).
// Se usa PlayOneShot porque los efectos cortos pueden sonar al tiempo sin cortarse.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource fuente;
    public AudioClip salto, recoger, golpe, muerte, checkpoint, palanca, elevador, golpeJefe, victoria, espada;

    void Awake()
    {
        Instance = this;
    }

    // Uso desde cualquier script:  AudioManager.Sonar(a => a.salto);
    public static void Sonar(Func<AudioManager, AudioClip> elegir)
    {
        if (Instance == null) return;
        AudioClip clip = elegir(Instance);
        if (clip != null) Instance.fuente.PlayOneShot(clip);
    }
}
