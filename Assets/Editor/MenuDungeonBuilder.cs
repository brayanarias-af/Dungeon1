using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú: Dungeon > Crear menú principal
/// 1) Configura los sprites como pixel art (Point, sin compresión).
/// 2) Construye Canvas, fondo, caballero, logo y los 4 botones en la escena abierta.
/// </summary>
public static class MenuDungeonBuilder
{
    [MenuItem("Dungeon/Configurar sprites (pixel art)")]
    public static void ConfigurarSprites()
    {
        string carpeta = EncontrarCarpetaSprites();
        if (carpeta == null) return;

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { carpeta }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) continue;

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.maxTextureSize = 4096;
            imp.SaveAndReimport();
        }
        Debug.Log("Sprites del menú configurados como pixel art.");
    }

    [MenuItem("Dungeon/Crear menú principal")]
    public static void CrearMenu()
    {
        string carpeta = EncontrarCarpetaSprites();
        if (carpeta == null) return;
        ConfigurarSprites();

        Sprite fondo    = Cargar(carpeta + "/Fondo_Mazmorra.png");
        Sprite logo     = Cargar(carpeta + "/Logo_Dungeon.png");
        Sprite caballero = Cargar(carpeta + "/Caballero.png");
        if (fondo == null || logo == null) return;

        // ---- Canvas ----
        var canvasGO = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear menú Dungeon");
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        var menu = canvasGO.AddComponent<MenuPrincipal>();

        // ---- EventSystem ----
#if UNITY_2022_2_OR_NEWER
        bool hayEventSystem = Object.FindFirstObjectByType<EventSystem>() != null;
#else
        bool hayEventSystem = Object.FindObjectOfType<EventSystem>() != null;
#endif
        if (!hayEventSystem)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
            Undo.RegisterCreatedObjectUndo(es, "Crear EventSystem");
        }

        // ---- Fondo (pantalla completa) ----
        var rtFondo = CrearImagen("Fondo", canvasGO.transform, fondo, out Image imgFondo);
        imgFondo.raycastTarget = false;
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;

        // ---- Caballero (abajo a la izquierda, recortado por el borde como en el diseño) ----
        if (caballero != null)
        {
            var rtCab = CrearImagen("Caballero", canvasGO.transform, caballero, out Image imgCab);
            imgCab.raycastTarget = false;
            rtCab.anchorMin = rtCab.anchorMax = Vector2.zero;
            rtCab.pivot = Vector2.zero;
            rtCab.sizeDelta = new Vector2(600, 800);
            rtCab.anchoredPosition = new Vector2(-60, -254);
        }

        // ---- Logo (arriba al centro) ----
        var rtLogo = CrearImagen("Logo", canvasGO.transform, logo, out Image imgLogo);
        imgLogo.raycastTarget = false;
        rtLogo.anchorMin = rtLogo.anchorMax = new Vector2(0.5f, 1f);
        rtLogo.pivot = new Vector2(0.5f, 1f);
        rtLogo.sizeDelta = new Vector2(760, 376);
        rtLogo.anchoredPosition = new Vector2(0, -20);

        // ---- Contenedor de botones ----
        var contGO = new GameObject("Botones", typeof(RectTransform), typeof(VerticalLayoutGroup));
        contGO.transform.SetParent(canvasGO.transform, false);
        var rtCont = contGO.GetComponent<RectTransform>();
        rtCont.anchorMin = rtCont.anchorMax = new Vector2(0.5f, 1f);
        rtCont.pivot = new Vector2(0.5f, 1f);
        rtCont.sizeDelta = new Vector2(440, 534);
        rtCont.anchoredPosition = new Vector2(0, -420);
        var vlg = contGO.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 18;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // ---- Botones ----
        string b = carpeta + "/Botones/Boton_";
        CrearBoton(contGO.transform, "Jugar",    Cargar(b + "Jugar_Normal.png"),    Cargar(b + "Jugar_Hover.png"),    menu.Jugar);
        CrearBoton(contGO.transform, "Opciones", Cargar(b + "Opciones_Normal.png"), Cargar(b + "Opciones_Hover.png"), menu.Opciones);
        CrearBoton(contGO.transform, "Creditos", Cargar(b + "Creditos_Normal.png"), Cargar(b + "Creditos_Hover.png"), menu.Creditos);
        CrearBoton(contGO.transform, "Salir",    Cargar(b + "Salir_Normal.png"),    Cargar(b + "Salir_Hover.png"),    menu.Salir);

        Selection.activeGameObject = canvasGO;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Menú Dungeon creado. Recuerda agregar tus escenas en File > Build Settings.");
    }

    // ---------- helpers ----------

    private static string EncontrarCarpetaSprites()
    {
        string[] guids = AssetDatabase.FindAssets("Fondo_Mazmorra t:Texture2D");
        if (guids.Length == 0)
        {
            Debug.LogError("No se encontró Fondo_Mazmorra.png. Copia la carpeta Assets/Sprites/Menu al proyecto.");
            return null;
        }
        string ruta = AssetDatabase.GUIDToAssetPath(guids[0]);
        return System.IO.Path.GetDirectoryName(ruta).Replace("\\", "/");
    }

    private static Sprite Cargar(string ruta)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if (s == null) Debug.LogWarning("No se pudo cargar el sprite: " + ruta);
        return s;
    }

    private static RectTransform CrearImagen(string nombre, Transform padre, Sprite sprite, out Image img)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        img = go.GetComponent<Image>();
        img.sprite = sprite;
        return go.GetComponent<RectTransform>();
    }

    private static void CrearBoton(Transform padre, string nombre, Sprite normal, Sprite hover, UnityAction accion)
    {
        var rt = CrearImagen("Btn" + nombre, padre, normal, out Image img);
        var le = rt.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 440;
        le.preferredHeight = 120;

        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.SpriteSwap;
        btn.spriteState = new SpriteState
        {
            highlightedSprite = hover,
            selectedSprite = hover,
            pressedSprite = hover
        };
        UnityEventTools.AddPersistentListener(btn.onClick, accion);
    }
}
