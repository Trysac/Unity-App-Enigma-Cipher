using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace EnigmaCipher.App
{
    /// <summary>
    /// Opt-in capture of README screenshots from a built player:
    /// <c>Enigma.exe -screenshot &lt;folder&gt; -scenario typing,help</c>.
    /// Without the -screenshot argument nothing is created, so normal runs and
    /// WebGL builds (which have no command line) are unaffected.
    /// </summary>
    public sealed class ScreenshotScenarios : MonoBehaviour
    {
        const string DefaultScenarios = "start,typing,help,spanish";

        string folder;
        string[] scenarios;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartWhenRequested()
        {
            string target = Argument("-screenshot");
            if (string.IsNullOrEmpty(target))
            {
                return;
            }
            var host = new GameObject(nameof(ScreenshotScenarios)).AddComponent<ScreenshotScenarios>();
            host.folder = target;
            host.scenarios = (Argument("-scenario") ?? DefaultScenarios).Split(',');
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            var view = FindAnyObjectByType<EnigmaAppView>();
            foreach (string scenario in scenarios)
            {
                Apply(view, scenario.Trim());
                // Let layout, transitions and the rotor step animation settle.
                yield return new WaitForSecondsRealtime(0.6f);
                yield return new WaitForEndOfFrame();
                Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(folder, scenario.Trim() + ".png"), shot.EncodeToPNG());
                Destroy(shot);
            }
            Application.Quit();
        }

        static void Apply(EnigmaAppView view, string scenario)
        {
            MachineSession session = view.Session;
            view.ReleaseKey();
            view.SetHelpVisible(false);
            session.SelectModel(0);
            session.TrySetPlugboard(string.Empty, out _);
            view.SyncSettingsFields();
            view.SetInputText(string.Empty);
            view.ProcessText();
            switch (scenario)
            {
                case "start":
                    break;
                case "typing":
                    session.SelectRotor(0, 1);
                    session.SelectRotor(1, 3);
                    session.SetRingLetters("BUL");
                    session.SetStartLetters("BLA");
                    session.TrySetPlugboard("AV BS CG DL FU HZ IN KM OW RX", out _);
                    view.SyncSettingsFields();
                    Type(view, "HELLO WORLD");
                    view.SetInputText("Meet me at the old lighthouse at dawn.");
                    view.ProcessText();
                    break;
                case "help":
                    Type(view, "ENIGMA");
                    view.SetHelpVisible(true);
                    break;
                case "spanish":
                    session.SelectModel(1);
                    session.SetStartLetters("ÑAZ");
                    session.TrySetPlugboard("AÑ BZ", out _);
                    view.SyncSettingsFields();
                    Type(view, "HOLA MUNDO");
                    view.SetInputText("Mañana a las ocho en la estación");
                    view.ProcessText();
                    break;
                default:
                    Debug.LogWarning($"Unknown screenshot scenario '{scenario}'.");
                    break;
            }
        }

        /// <summary>Types a message; the last key stays held so its lamp is lit in the picture.</summary>
        static void Type(EnigmaAppView view, string message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                view.PressKey(message[i], hold: true);
                if (i < message.Length - 1)
                {
                    view.ReleaseKey();
                }
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
