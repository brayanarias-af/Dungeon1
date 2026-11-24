using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lógica del menú principal de Dungeon.
/// Cambia los nombres de escena en el Inspector y agrégalas en File > Build Settings.
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Header("Nombres de escena")]
    [SerializeField] private string escenaJuego = "Juego";
    [SerializeField] private string escenaOpciones = "Opciones";
    [SerializeField] private string escenaCreditos = "Creditos";

    public void Jugar()    => CargarEscena(escenaJuego);
    public void Opciones() => CargarEscena(escenaOpciones);
    public void Creditos() => CargarEscena(escenaCreditos);

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void CargarEscena(string nombre)
    {
        if (!Application.CanStreamedLevelBeLoaded(nombre))
        {
            Debug.LogWarning($"La escena '{nombre}' no existe o no está en Build Settings.");
            return;
        }
        SceneManager.LoadScene(nombre);
    }
}
