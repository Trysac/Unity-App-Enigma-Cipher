using System;
using System.Collections.Generic;
using System.Text;
using EnigmaCipher.Core;

namespace EnigmaCipher.App
{
    /// <summary>
    /// Settings and live state behind the simulator screen, kept free of any
    /// rendering so it can be tested on its own. Every settings change rebuilds
    /// the machine at the chosen start positions.
    /// </summary>
    public sealed class MachineSession
    {
        public const int SlotCount = 3;

        readonly int[] rotorChoice = new int[SlotCount];
        readonly int[] ringSetting = new int[SlotCount];
        readonly int[] startPosition = new int[SlotCount];
        readonly StringBuilder typed = new StringBuilder();
        readonly StringBuilder lit = new StringBuilder();

        /// <summary>Raised after the machine was rebuilt from the settings.</summary>
        public event Action SettingsChanged;

        public MachineSession()
        {
            ApplyModelDefaults(0);
            Rebuild();
        }

        public IReadOnlyList<MachineModel> Models => Presets.All;
        public int ModelIndex { get; private set; }
        public MachineModel Model => Models[ModelIndex];
        public Alphabet Alphabet => Model.Alphabet;
        public int ReflectorIndex { get; private set; }

        /// <summary>Normalised cables in use, e.g. "AB CD". Only valid boards are ever applied.</summary>
        public string PlugboardPairs { get; private set; } = string.Empty;
        public int CableCount { get; private set; }
        public int MaxCables => Alphabet.Size / 2;

        /// <summary>The machine typed on by the keyboard; it stays at the position the last key left it.</summary>
        public EnigmaMachine Machine { get; private set; }
        public KeyTrace? LastTrace { get; private set; }

        /// <summary>Keys pressed since the last reset, and the lamps they lit.</summary>
        public string TypedText => typed.ToString();
        public string LitText => lit.ToString();

        public IReadOnlyList<string> KeyRows => KeyboardLayouts.For(Model);

        public int RotorChoice(int slot) => rotorChoice[slot];
        public char RingLetter(int slot) => Alphabet[ringSetting[slot]];
        public char StartLetter(int slot) => Alphabet[startPosition[slot]];

        /// <summary>Switches machine model and restores its default rotors, reflector and settings.</summary>
        public void SelectModel(int index)
        {
            if (index < 0 || index >= Models.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            ApplyModelDefaults(index);
            // Cables may name letters the new alphabet lacks (or not include all it offers).
            if (!TryParsePlugboard(PlugboardPairs, out _, out _))
            {
                PlugboardPairs = string.Empty;
            }
            Rebuild();
        }

        public void SelectReflector(int index)
        {
            if (index < 0 || index >= Model.Reflectors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            ReflectorIndex = index;
            Rebuild();
        }

        /// <summary>
        /// Puts a rotor in a slot. A machine owns one copy of each rotor, so when
        /// the rotor already sits in another slot the two slots swap rotors.
        /// </summary>
        public void SelectRotor(int slot, int rotor)
        {
            if (rotor < 0 || rotor >= Model.Rotors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rotor));
            }
            int other = Array.IndexOf(rotorChoice, rotor);
            if (other >= 0 && other != slot)
            {
                rotorChoice[other] = rotorChoice[slot];
            }
            rotorChoice[slot] = rotor;
            Rebuild();
        }

        public void StepRing(int slot, int delta)
        {
            ringSetting[slot] = Alphabet.Mod(ringSetting[slot] + delta);
            Rebuild();
        }

        public void StepStart(int slot, int delta)
        {
            startPosition[slot] = Alphabet.Mod(startPosition[slot] + delta);
            Rebuild();
        }

        /// <summary>Sets the three start letters at once, e.g. "MCK".</summary>
        public void SetStartLetters(string letters)
        {
            int[] positions = ParseLetters(letters);
            positions.CopyTo(startPosition, 0);
            Rebuild();
        }

        public void SetRingLetters(string letters)
        {
            int[] positions = ParseLetters(letters);
            positions.CopyTo(ringSetting, 0);
            Rebuild();
        }

        /// <summary>
        /// Applies cables such as "AB CD EF". An invalid board leaves the
        /// previous cables in place and returns the reason.
        /// </summary>
        public bool TrySetPlugboard(string spec, out string error)
        {
            if (!TryParsePlugboard(spec, out string pairs, out error))
            {
                return false;
            }
            PlugboardPairs = pairs;
            Rebuild();
            return true;
        }

        /// <summary>Returns the live machine to the start positions and clears the typed tape.</summary>
        public void ResetMachine()
        {
            Rebuild();
        }

        /// <summary>
        /// Presses one key on the live machine. A space on a machine without a
        /// space key only separates words on the tape and does not move the rotors.
        /// </summary>
        public KeyTrace? Press(char key)
        {
            KeyTrace? trace = Machine.Press(key);
            if (trace.HasValue)
            {
                LastTrace = trace;
                typed.Append(trace.Value.Input);
                lit.Append(trace.Value.Output);
            }
            else if (key == ' ' && typed.Length > 0 && typed[typed.Length - 1] != ' ')
            {
                typed.Append(' ');
                lit.Append(' ');
            }
            return trace;
        }

        /// <summary>
        /// Runs a whole message through a fresh copy of the machine at the start
        /// positions. Enigma is its own inverse, so this both encrypts and decrypts;
        /// the live machine is not touched.
        /// </summary>
        public string ProcessText(string text) => CreateMachine().Process(text);

        /// <summary>Signal path of a key press, e.g. "A → plug A → III: B → ... → lamp N".</summary>
        public string DescribeTrace(KeyTrace trace)
        {
            IReadOnlyList<Rotor> rotors = Machine.Rotors;
            int last = rotors.Count - 1;
            var steps = new List<string>
            {
                $"key {Show(trace.Input)}",
                $"plug {Show(Alphabet[trace.Path[0]])}",
            };
            for (int i = last, p = 1; i >= 0; i--, p++)
            {
                steps.Add($"{rotors[i].Name}: {Show(Alphabet[trace.Path[p]])}");
            }
            steps.Add($"reflector {Machine.Reflector.Name}: {Show(Alphabet[trace.Path[rotors.Count + 1]])}");
            for (int i = 0, p = rotors.Count + 2; i <= last; i++, p++)
            {
                steps.Add($"{rotors[i].Name}: {Show(Alphabet[trace.Path[p]])}");
            }
            steps.Add($"plug {Show(Alphabet[trace.Path[trace.Path.Count - 1]])}");
            steps.Add($"lamp {Show(trace.Output)}");
            return string.Join("  →  ", steps);
        }

        /// <summary>Visible form of a letter; the Spanish machine's space key is shown as "␣".</summary>
        public static string Show(char letter) => letter == ' ' ? "␣" : letter.ToString();

        void ApplyModelDefaults(int model)
        {
            ModelIndex = model;
            ReflectorIndex = 0;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                rotorChoice[slot] = slot;
                ringSetting[slot] = 0;
                startPosition[slot] = 0;
            }
        }

        void Rebuild()
        {
            Machine = CreateMachine();
            CableCount = Machine.Plugboard.Pairs.Count;
            LastTrace = null;
            typed.Clear();
            lit.Clear();
            SettingsChanged?.Invoke();
        }

        EnigmaMachine CreateMachine()
        {
            EnigmaMachine machine = Model.Build(ReflectorIndex, rotorChoice);
            for (int slot = 0; slot < SlotCount; slot++)
            {
                machine.Rotors[slot].RingSetting = ringSetting[slot];
                machine.Rotors[slot].Position = startPosition[slot];
            }
            machine.Plugboard.SetPairs(PlugboardPairs);
            return machine;
        }

        bool TryParsePlugboard(string spec, out string pairs, out string error)
        {
            var board = new Plugboard(Alphabet);
            try
            {
                board.SetPairs(spec);
            }
            catch (ArgumentException e)
            {
                pairs = null;
                error = e.Message;
                return false;
            }
            pairs = string.Join(" ", board.Pairs);
            error = null;
            return true;
        }

        int[] ParseLetters(string letters)
        {
            if (letters == null || letters.Length != SlotCount)
            {
                throw new ArgumentException($"Expected {SlotCount} letters.", nameof(letters));
            }
            var positions = new int[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                positions[i] = Alphabet.IndexOf(letters[i]);
                if (positions[i] < 0)
                {
                    throw new ArgumentException($"'{letters[i]}' is not a letter of this machine.", nameof(letters));
                }
            }
            return positions;
        }
    }

    /// <summary>Key arrangement of each model's keyboard and lampboard.</summary>
    public static class KeyboardLayouts
    {
        /// <summary>The German QWERTZ arrangement of the historical Enigma I.</summary>
        public static readonly IReadOnlyList<string> EnigmaI = new[] { "QWERTZUIO", "ASDFGHJK", "PYXCVBNML" };

        /// <summary>Alphabetical rows for the Spanish machine, with Ñ and a space bar.</summary>
        public static readonly IReadOnlyList<string> Spanish = new[] { "ABCDEFGHI", "JKLMNÑOPQ", "RSTUVWXYZ", " " };

        public static IReadOnlyList<string> For(MachineModel model) =>
            model == Presets.Spanish ? Spanish : EnigmaI;
    }
}
