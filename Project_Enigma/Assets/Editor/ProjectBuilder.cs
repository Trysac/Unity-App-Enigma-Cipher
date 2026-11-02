using System;
using System.IO;
using EnigmaCipher.App;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace EnigmaCipher.EditorTools
{
    /// <summary>
    /// Generates the scene, panel settings and app icon from code, and builds
    /// the players, so every binary asset can be recreated from text.
    /// Batch mode:
    /// <code>
    /// Unity -batchmode -quit -projectPath Project_Enigma -executeMethod EnigmaCipher.EditorTools.ProjectBuilder.SetUpProject
    /// Unity -batchmode -quit -projectPath Project_Enigma -executeMethod EnigmaCipher.EditorTools.ProjectBuilder.BuildWindows -buildOutput &lt;folder&gt;
    /// Unity -batchmode -quit -projectPath Project_Enigma -executeMethod EnigmaCipher.EditorTools.ProjectBuilder.BuildWebGL -buildOutput &lt;folder&gt;
    /// </code>
    /// </summary>
    public static class ProjectBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string LayoutPath = "Assets/UI/EnigmaApp.uxml";
        const string ThemePath = "Assets/UI/EnigmaTheme.tss";
        const string PanelSettingsPath = "Assets/UI/EnigmaPanelSettings.asset";
        const string IconPath = "Assets/Art/AppIcon.png";
        static readonly Color CaseColor = new Color32(0x17, 0x12, 0x0e, 0xff);

        [MenuItem("Enigma/Regenerate Scene, Icon and Player Settings")]
        public static void SetUpProject()
        {
            Texture2D icon = CreateIcon();
            PanelSettings panel = CreatePanelSettings();
            CreateScene(panel);
            ConfigurePlayer(icon);
            AssetDatabase.SaveAssets();
            Debug.Log("Enigma project set up.");
        }

        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Enigma.exe");

        public static void BuildWebGL() => Build(BuildTarget.WebGL, string.Empty);

        static PanelSettings CreatePanelSettings()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            // Designed at 1280x720; "shrink" keeps everything visible on other aspect ratios.
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1280, 720);
            panel.screenMatchMode = PanelScreenMatchMode.Shrink;
            panel.clearColor = true;
            panel.colorClearValue = CaseColor;
            EditorUtility.SetDirty(panel);
            return panel;
        }

        static void CreateScene(PanelSettings panel)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = CaseColor;
            camera.cullingMask = 0;
            camera.tag = "MainCamera";

            var ui = new GameObject("Enigma UI");
            var document = ui.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LayoutPath);
            ui.AddComponent<EnigmaAppView>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void ConfigurePlayer(Texture2D icon)
        {
            PlayerSettings.productName = "Enigma";
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            int[] sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Any);
            var standaloneIcons = new Texture2D[sizes.Length];
            Array.Fill(standaloneIcons, icon);
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, standaloneIcons, IconKind.Any);

            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            // Static hosts such as GitHub Pages do not send Content-Encoding for .br/.gz files.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
        }

        // ---- Icon: a brass rotor with 26 contacts around a lit lamp ----

        static Texture2D CreateIcon()
        {
            const int size = 1024;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = IconPixel(x + 0.5f - size / 2f, y + 0.5f - size / 2f, size / 2f);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
            File.WriteAllBytes(IconPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        }

        /// <summary>Colour at a point given relative to the centre; `half` is half the icon size.</summary>
        static Color IconPixel(float x, float y, float half)
        {
            float scale = half / 512f;
            float r = Mathf.Sqrt(x * x + y * y) / scale;
            float angle = Mathf.Atan2(y, x);
            float aa = 1.5f / scale;

            // Rounded walnut tile.
            float corner = RoundedSquareDistance(x / scale, y / scale, 470f, 170f);
            var color = Color.Lerp(new Color32(0x30, 0x22, 0x16, 0xff), new Color32(0x12, 0x0d, 0x09, 0xff), (y / half + 1f) / 2f);
            color.a = Coverage(-corner, aa);

            // Brass rotor: a ring with 26 teeth on its rim.
            const int contacts = 26;
            float sector = angle / (2f * Mathf.PI) * contacts;
            float tooth = Mathf.Abs(sector - Mathf.Round(sector)); // 0 at a tooth centre, 0.5 between teeth
            float outer = 380f + (tooth < 0.28f ? 34f : 0f);
            float ring = Mathf.Min(r - 268f, outer - r);
            var brass = Color.Lerp(new Color32(0xe6, 0xc4, 0x7c, 0xff), new Color32(0x8a, 0x66, 0x30, 0xff), (1f - y / half) / 2f);
            color = Over(color, brass, Coverage(ring, aa));

            // Engraved contact dots on the ring.
            float dotAngle = (Mathf.Round(sector) / contacts) * 2f * Mathf.PI;
            float dx = x / scale - Mathf.Cos(dotAngle) * 324f;
            float dy = y / scale - Mathf.Sin(dotAngle) * 324f;
            color = Over(color, new Color32(0x4a, 0x34, 0x16, 0xff), Coverage(13f - Mathf.Sqrt(dx * dx + dy * dy), aa));

            // Dark well and the glowing lamp inside it.
            color = Over(color, new Color32(0x1a, 0x13, 0x0d, 0xff), Coverage(262f - r, aa));
            float glow = Mathf.Clamp01(1f - r / 250f);
            color = Over(color, new Color32(0xff, 0xa9, 0x3a, 0xff), glow * glow * 0.85f);
            color = Over(color, new Color32(0xff, 0xd8, 0x80, 0xff), Coverage(150f - r, aa));
            color = Over(color, new Color32(0xff, 0xf6, 0xdc, 0xff), Mathf.Clamp01(1f - r / 110f));
            return color;
        }

        static float RoundedSquareDistance(float x, float y, float halfSize, float radius)
        {
            float qx = Mathf.Abs(x) - (halfSize - radius);
            float qy = Mathf.Abs(y) - (halfSize - radius);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        /// <summary>Anti-aliased coverage of a shape from its signed distance (positive inside).</summary>
        static float Coverage(float inside, float width) => Mathf.Clamp01(inside / width + 0.5f);

        static Color Over(Color below, Color above, float amount)
        {
            Color mixed = Color.Lerp(below, above, amount);
            mixed.a = below.a;
            return mixed;
        }

        // ---- Builds ----

        static void Build(BuildTarget target, string fileName)
        {
            string folder = Argument("-buildOutput");
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentException("Pass -buildOutput <folder> outside the repository.");
            }
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = target,
                locationPathName = string.IsNullOrEmpty(fileName) ? folder : Path.Combine(folder, fileName),
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"Build {target}: {summary.result}, {summary.totalSize / (1024 * 1024f):0.0} MB, {summary.totalErrors} errors.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
