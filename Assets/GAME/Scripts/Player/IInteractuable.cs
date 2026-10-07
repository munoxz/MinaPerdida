// Todo lo que el jugador puede accionar con el raycast frontal (palancas, interruptores...)
public interface IInteractuable
{
    void Interactuar(PlayerController jugador);
    string TextoAyuda { get; } // ej: "Presiona E para usar la palanca"
}
