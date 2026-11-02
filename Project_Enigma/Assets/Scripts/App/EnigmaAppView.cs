using System.Collections.Generic;
using EnigmaCipher.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace EnigmaCipher.App
{
    /// <summary>
    /// Binds the UI Toolkit screen (EnigmaApp.uxml) to a <see cref="MachineSession"/>.
    /// All cipher logic lives in the session and the core; this class only
    /// builds the repeated widgets, forwards input and redraws.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class EnigmaAppView : MonoBehaviour
    {
        static readonly string[] SlotNames = { "Left", "Middle", "Right" };
        const float StepAnimationSeconds = 0.14f;
        const float MinimumLampSeconds = 0.2f;
        const int TapeLength = 34;

        readonly Dictionary<char, VisualElement> lamps = new Dictionary<char, VisualElement>();
        readonly Dictionary<char, VisualElement> keys = new Dictionary<char, VisualElement>();
        readonly SlotWidgets[] slots = new SlotWidgets[MachineSession.SlotCount];
        readonly WindowWidgets[] windows = new WindowWidgets[MachineSession.SlotCount];

        VisualElement root;
        DropdownField modelField;
        DropdownField reflectorField;
        TextField plugboardField;
        Label plugboardStatus;
        Label cableCount;
        VisualElement lampboard;
        VisualElement keyboard;
        Label tapeTyped;
        Label tapeLit;
        Label signalPath;
        TextField inputText;
        TextField outputText;
        Label textStatus;
        VisualElement helpOverlay;

        MachineModel builtBoardsFor;
        VisualElement litLamp;
        VisualElement pressedKey;
        bool keyHeld;
        bool heldByPhysicalKey;
        float lampLitAt;

        public MachineSession Session { get; } = new MachineSession();

        sealed class SlotWidgets
        {
            public DropdownField Rotor;
            public Label Ring;
            public Label Start;
        }

        sealed class WindowWidgets
        {
            public Label Letter;
            public Label Name;
            public char Shown;
            public float StepTime = -1f;
        }

        void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            modelField = root.Q<DropdownField>("model");
            reflectorField = root.Q<DropdownField>("reflector");
            plugboardField = root.Q<TextField>("plugboard");
            plugboardStatus = root.Q<Label>("plugboard-status");
            cableCount = root.Q<Label>("cable-count");
            lampboard = root.Q("lampboard");
            keyboard = root.Q("keyboard");
            tapeTyped = root.Q<Label>("tape-typed");
            tapeLit = root.Q<Label>("tape-lit");
            signalPath = root.Q<Label>("signal-path");
            inputText = root.Q<TextField>("input-text");
            outputText = root.Q<TextField>("output-text");
            textStatus = root.Q<Label>("text-status");
            helpOverlay = root.Q("help-overlay");

            BuildSettings();
            BuildWindows();
            BindTextMode();

            // Buttons must not keep focus: the space bar would click them instead of typing.
            root.Query<Button>().ForEach(button => button.focusable = false);
            root.RegisterCallback<PointerDownEvent>(BlurOnBackgroundClick, TrickleDown.TrickleDown);
            root.Q<Button>("help-button").clicked += () => SetHelpVisible(true);
            root.Q<Button>("help-close").clicked += () => SetHelpVisible(false);

            Session.SettingsChanged += Redraw;
            SyncSettingsFields();
        }

        void OnDisable()
        {
            Session.SettingsChanged -= Redraw;
        }

        void Update()
        {
            ReadPhysicalKeyboard();
            if (litLamp != null && !keyHeld && Time.unscaledTime - lampLitAt >= MinimumLampSeconds)
            {
                ClearLamp();
            }
            AnimateWindows();
        }

        // ---- Public surface (also used by the screenshot scenarios) ----

        /// <summary>Presses a key like the operator would; with `hold` the lamp stays lit until <see cref="ReleaseKey"/>.</summary>
        public void PressKey(char key, bool hold)
        {
            KeyTrace? trace = Session.Press(key);
            ClearLamp();
            if (trace.HasValue)
            {
                if (lamps.TryGetValue(trace.Value.Output, out litLamp))
                {
                    litLamp.AddToClassList("lamp--on");
                }
                if (keys.TryGetValue(trace.Value.Input, out pressedKey))
                {
                    pressedKey.AddToClassList("key--pressed");
                }
                lampLitAt = Time.unscaledTime;
            }
            keyHeld = hold && trace.HasValue;
            heldByPhysicalKey = false;
            RedrawMachineState();
        }

        public void ReleaseKey()
        {
            keyHeld = false;
            heldByPhysicalKey = false;
            pressedKey?.RemoveFromClassList("key--pressed");
            pressedKey = null;
        }

        public void SetHelpVisible(bool visible)
        {
            helpOverlay.EnableInClassList("overlay--hidden", !visible);
        }

        public void SetInputText(string text) => inputText.value = text;

        public void ProcessText()
        {
            outputText.value = Session.ProcessText(inputText.value);
            textStatus.text = string.Empty;
        }

        /// <summary>Copies every setting from the session into the input widgets.</summary>
        public void SyncSettingsFields()
        {
            modelField.choices = new List<string>(ModelNames());
            modelField.SetValueWithoutNotify(Session.Model.Name);
            plugboardField.SetValueWithoutNotify(Session.PlugboardPairs);
            plugboardStatus.text = "Pairs of letters, e.g. AB CD EF";
            plugboardStatus.RemoveFromClassList("status--error");
            Redraw();
        }

        // ---- Building ----

        void BuildSettings()
        {
            modelField.RegisterValueChangedCallback(e =>
            {
                Session.SelectModel(modelField.index);
                SyncSettingsFields();
            });
            reflectorField.RegisterValueChangedCallback(e => Session.SelectReflector(reflectorField.index));

            VisualElement container = root.Q("rotor-slots");
            for (int slot = 0; slot < MachineSession.SlotCount; slot++)
            {
                int captured = slot;
                var column = new VisualElement();
                column.AddToClassList("slot");
                column.Add(NewLabel(SlotNames[slot], "slot__name"));

                var rotor = new DropdownField();
                rotor.AddToClassList("slot__rotor");
                rotor.RegisterValueChangedCallback(e => Session.SelectRotor(captured, rotor.index));
                column.Add(rotor);

                column.Add(NewLabel("Ring", "slot__caption"));
                Label ring = AddStepper(column, delta => Session.StepRing(captured, delta));
                column.Add(NewLabel("Start", "slot__caption"));
                Label start = AddStepper(column, delta => Session.StepStart(captured, delta));

                slots[slot] = new SlotWidgets { Rotor = rotor, Ring = ring, Start = start };
                container.Add(column);
            }

            plugboardField.RegisterValueChangedCallback(e => ApplyPlugboard(e.newValue));
            root.Q<Button>("reset-machine").clicked += Session.ResetMachine;
        }

        static Label AddStepper(VisualElement parent, System.Action<int> step)
        {
            var row = new VisualElement();
            row.AddToClassList("stepper");
            var down = new Button(() => step(-1)) { text = "−" };
            var value = NewLabel(string.Empty, "stepper__value");
            var up = new Button(() => step(1)) { text = "+" };
            down.AddToClassList("stepper__button");
            up.AddToClassList("stepper__button");
            row.Add(down);
            row.Add(value);
            row.Add(up);
            parent.Add(row);
            return value;
        }

        void BuildWindows()
        {
            VisualElement container = root.Q("windows");
            for (int slot = 0; slot < MachineSession.SlotCount; slot++)
            {
                var column = new VisualElement();
                column.AddToClassList("window");
                var frame = new VisualElement();
                frame.AddToClassList("window__frame");
                var letter = NewLabel(string.Empty, "window__letter");
                frame.Add(letter);
                column.Add(frame);
                var name = NewLabel(string.Empty, "window__name");
                column.Add(name);
                windows[slot] = new WindowWidgets { Letter = letter, Name = name };
                container.Add(column);
            }
        }

        /// <summary>Lamps and keys follow the model's layout, so they are rebuilt when it changes.</summary>
        void BuildBoards()
        {
            builtBoardsFor = Session.Model;
            lamps.Clear();
            keys.Clear();
            lampboard.Clear();
            keyboard.Clear();
            litLamp = null;
            pressedKey = null;
            foreach (string row in Session.KeyRows)
            {
                var lampRow = new VisualElement();
                var keyRow = new VisualElement();
                lampRow.AddToClassList("board__row");
                keyRow.AddToClassList("board__row");
                foreach (char letter in row)
                {
                    bool space = letter == ' ';
                    var lamp = NewLabel(space ? "space" : letter.ToString(), "lamp");
                    lamp.EnableInClassList("lamp--wide", space);
                    lamps[letter] = lamp;
                    lampRow.Add(lamp);

                    var key = NewLabel(space ? "space" : letter.ToString(), "key");
                    key.EnableInClassList("key--wide", space);
                    char captured = letter;
                    key.RegisterCallback<PointerDownEvent>(e =>
                    {
                        key.CapturePointer(e.pointerId);
                        PressKey(captured, true);
                    });
                    key.RegisterCallback<PointerUpEvent>(e =>
                    {
                        key.ReleasePointer(e.pointerId);
                        ReleaseKey();
                    });
                    keys[letter] = key;
                    keyRow.Add(key);
                }
                lampboard.Add(lampRow);
                keyboard.Add(keyRow);
            }
        }

        void BindTextMode()
        {
            inputText.verticalScrollerVisibility = ScrollerVisibility.Auto;
            outputText.verticalScrollerVisibility = ScrollerVisibility.Auto;
            root.Q<Button>("process-text").clicked += ProcessText;
            root.Q<Button>("swap-text").clicked += () =>
            {
                inputText.value = outputText.value;
                outputText.value = string.Empty;
                textStatus.text = string.Empty;
            };
            root.Q<Button>("reset-text").clicked += () =>
            {
                inputText.value = string.Empty;
                outputText.value = string.Empty;
                textStatus.text = string.Empty;
                Session.ResetMachine();
            };
            root.Q<Button>("copy-output").clicked += () =>
            {
                GUIUtility.systemCopyBuffer = outputText.value;
                // A WebGL page cannot write the system clipboard from Unity without a plugin.
                textStatus.text = Application.platform == RuntimePlatform.WebGLPlayer
                    ? "Copied inside the app only"
                    : "Copied to the clipboard";
            };
        }

        // ---- Input ----

        void ApplyPlugboard(string spec)
        {
            bool ok = Session.TrySetPlugboard(spec, out string error);
            plugboardStatus.EnableInClassList("status--error", !ok);
            plugboardStatus.text = ok
                ? (Session.CableCount == 0 ? "No cables: letters go straight through" : $"Using {Session.PlugboardPairs}")
                : $"{error} Keeping {(Session.CableCount == 0 ? "no cables" : Session.PlugboardPairs)}.";
            RedrawPlugCount();
        }

        void ReadPhysicalKeyboard()
        {
            if (IsTypingInTextField() || !helpOverlay.ClassListContains("overlay--hidden"))
            {
                return;
            }
            foreach (char typed in Input.inputString)
            {
                char key = char.ToUpperInvariant(typed);
                if (Session.Alphabet.Contains(key) || key == ' ')
                {
                    PressKey(key, true);
                    heldByPhysicalKey = true;
                }
            }
            // Text input gives no key-up per character, so the lamp goes out when every key is up.
            if (heldByPhysicalKey && !Input.anyKey)
            {
                ReleaseKey();
            }
        }

        // A focused text field or dropdown owns the keyboard (space would also
        // reopen a dropdown), so the machine only listens when neither has focus.
        bool IsTypingInTextField()
        {
            for (var element = root.panel?.focusController?.focusedElement as VisualElement; element != null; element = element.parent)
            {
                if (element is TextField || element is DropdownField)
                {
                    return true;
                }
            }
            return false;
        }

        void BlurOnBackgroundClick(PointerDownEvent e)
        {
            if (e.target is VisualElement target && target.focusable)
            {
                return;
            }
            (root.panel?.focusController?.focusedElement as VisualElement)?.Blur();
        }

        // ---- Drawing ----

        void Redraw()
        {
            if (builtBoardsFor != Session.Model)
            {
                BuildBoards();
            }
            ClearLamp();
            ReleaseKey();

            var reflectorNames = new List<string>();
            foreach (KeyValuePair<string, string> reflector in Session.Model.Reflectors)
            {
                reflectorNames.Add(reflector.Key);
            }
            reflectorField.choices = reflectorNames;
            reflectorField.SetValueWithoutNotify(Session.Machine.Reflector.Name);

            var rotorNames = new List<string>();
            foreach (RotorSpec rotor in Session.Model.Rotors)
            {
                rotorNames.Add(rotor.Name);
            }
            for (int slot = 0; slot < MachineSession.SlotCount; slot++)
            {
                slots[slot].Rotor.choices = rotorNames;
                slots[slot].Rotor.SetValueWithoutNotify(rotorNames[Session.RotorChoice(slot)]);
                slots[slot].Ring.text = MachineSession.Show(Session.RingLetter(slot));
                slots[slot].Start.text = MachineSession.Show(Session.StartLetter(slot));
                windows[slot].Name.text = $"Rotor {Session.Machine.Rotors[slot].Name}";
            }
            RedrawPlugCount();
            RedrawMachineState();
        }

        void RedrawPlugCount()
        {
            cableCount.text = $"{Session.CableCount} / {Session.MaxCables} cables";
        }

        void RedrawMachineState()
        {
            string letters = Session.Machine.WindowLetters;
            for (int slot = 0; slot < MachineSession.SlotCount; slot++)
            {
                WindowWidgets window = windows[slot];
                if (window.Shown != letters[slot])
                {
                    // Animate only real steps, not a jump caused by a settings change.
                    window.StepTime = Session.LastTrace.HasValue ? Time.unscaledTime : -1f;
                    window.Shown = letters[slot];
                    window.Letter.text = MachineSession.Show(letters[slot]);
                }
            }

            tapeTyped.text = $"Keys    {Tail(Session.TypedText)}";
            tapeLit.text = $"Lamps  {Tail(Session.LitText)}";
            signalPath.text = Session.LastTrace.HasValue
                ? Session.DescribeTrace(Session.LastTrace.Value)
                : "Press a key to see how the current travels through the machine.";
        }

        void AnimateWindows()
        {
            foreach (WindowWidgets window in windows)
            {
                if (window.StepTime < 0f)
                {
                    continue;
                }
                float t = Mathf.Clamp01((Time.unscaledTime - window.StepTime) / StepAnimationSeconds);
                float eased = 1f - (1f - t) * (1f - t);
                window.Letter.style.translate = new Translate(0f, Length.Percent(-45f * (1f - eased)));
                window.Letter.style.opacity = 0.35f + 0.65f * eased;
                if (t >= 1f)
                {
                    window.StepTime = -1f;
                }
            }
        }

        void ClearLamp()
        {
            litLamp?.RemoveFromClassList("lamp--on");
            litLamp = null;
        }

        static string Tail(string text)
        {
            string shown = text.Length > TapeLength ? "…" + text.Substring(text.Length - TapeLength) : text;
            return shown.Length == 0 ? "-" : shown;
        }

        IEnumerable<string> ModelNames()
        {
            foreach (MachineModel model in Session.Models)
            {
                yield return model.Name;
            }
        }

        static Label NewLabel(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }
    }
}
