using System;
using System.Collections.Generic;
using System.Text;

namespace EnigmaCipher.Core
{
    /// <summary>
    /// Path of one key press through the machine, kept for visualisation.
    /// Contacts are alphabet indices.
    /// </summary>
    public readonly struct KeyTrace
    {
        public readonly char Input;
        public readonly char Output;
        /// <summary>Contacts in signal order: plugboard in, each rotor forward, reflector, each rotor back, plugboard out.</summary>
        public readonly IReadOnlyList<int> Path;

        public KeyTrace(char input, char output, IReadOnlyList<int> path)
        {
            Input = input;
            Output = output;
            Path = path;
        }
    }

    /// <summary>
    /// Rotor cipher machine in the style of the Enigma I: plugboard, three or
    /// more rotors with turnover notches (including the double step of the
    /// middle rotor), and a reflector. Pressing the same keys from the same
    /// start settings encrypts and decrypts.
    /// </summary>
    public sealed class EnigmaMachine
    {
        readonly Rotor[] rotors; // Left to right, as seen from the operator

        public Alphabet Alphabet { get; }
        public Reflector Reflector { get; }
        public Plugboard Plugboard { get; }
        public IReadOnlyList<Rotor> Rotors => rotors;

        public EnigmaMachine(Alphabet alphabet, Reflector reflector, IList<Rotor> rotorsLeftToRight)
        {
            if (rotorsLeftToRight == null || rotorsLeftToRight.Count == 0)
            {
                throw new ArgumentException("A machine needs at least one rotor.", nameof(rotorsLeftToRight));
            }
            Alphabet = alphabet;
            Reflector = reflector ?? throw new ArgumentNullException(nameof(reflector));
            Plugboard = new Plugboard(alphabet);
            rotors = new Rotor[rotorsLeftToRight.Count];
            rotorsLeftToRight.CopyTo(rotors, 0);
        }

        /// <summary>Letters in the rotor windows, left to right (e.g. "AAA").</summary>
        public string WindowLetters
        {
            get
            {
                var letters = new StringBuilder(rotors.Length);
                foreach (Rotor rotor in rotors)
                {
                    letters.Append(Alphabet[rotor.Position]);
                }
                return letters.ToString();
            }
            set
            {
                if (value == null || value.Length != rotors.Length)
                {
                    throw new ArgumentException($"Expected {rotors.Length} window letters.");
                }
                for (int i = 0; i < rotors.Length; i++)
                {
                    rotors[i].Position = RequireLetter(value[i]);
                }
            }
        }

        /// <summary>Ring settings as letters, left to right ("A" = no offset).</summary>
        public string RingLetters
        {
            get
            {
                var letters = new StringBuilder(rotors.Length);
                foreach (Rotor rotor in rotors)
                {
                    letters.Append(Alphabet[rotor.RingSetting]);
                }
                return letters.ToString();
            }
            set
            {
                if (value == null || value.Length != rotors.Length)
                {
                    throw new ArgumentException($"Expected {rotors.Length} ring letters.");
                }
                for (int i = 0; i < rotors.Length; i++)
                {
                    rotors[i].RingSetting = RequireLetter(value[i]);
                }
            }
        }

        /// <summary>
        /// Turns the rotors before a key closes the circuit: the rightmost rotor
        /// always steps, and a rotor at its notch steps together with the one on
        /// its left (so the middle rotor double-steps).
        /// </summary>
        public void StepRotors()
        {
            int last = rotors.Length - 1;
            var steps = new bool[rotors.Length];
            steps[last] = true;
            // The leftmost rotor has no pawl to its left, so only rotors 1..last carry over.
            for (int i = last; i >= 1; i--)
            {
                if (rotors[i].AtNotch)
                {
                    steps[i - 1] = true;
                    // A pawl pushing on a notch also moves the rotor that owns it,
                    // except the rightmost one, which steps anyway.
                    if (i < last)
                    {
                        steps[i] = true;
                    }
                }
            }
            for (int i = 0; i < rotors.Length; i++)
            {
                if (steps[i])
                {
                    rotors[i].Step();
                }
            }
        }

        /// <summary>Presses one key. Returns null for a character the machine has no key for.</summary>
        public KeyTrace? Press(char key)
        {
            int index = Alphabet.IndexOf(key);
            if (index < 0)
            {
                return null;
            }

            StepRotors();
            var path = new List<int>(rotors.Length * 2 + 3);
            int signal = Plugboard.Swap(index);
            path.Add(signal);
            for (int i = rotors.Length - 1; i >= 0; i--)
            {
                signal = rotors[i].Forward(signal);
                path.Add(signal);
            }
            signal = Reflector.Reflect(signal);
            path.Add(signal);
            for (int i = 0; i < rotors.Length; i++)
            {
                signal = rotors[i].Backward(signal);
                path.Add(signal);
            }
            signal = Plugboard.Swap(signal);
            path.Add(signal);

            return new KeyTrace(Alphabet[index], Alphabet[signal], path);
        }

        /// <summary>
        /// Types a whole message. Characters without a key are copied unchanged
        /// and do not move the rotors; letters come out in upper case.
        /// </summary>
        public string Process(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            var output = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                KeyTrace? trace = Press(c);
                output.Append(trace.HasValue ? trace.Value.Output : c);
            }
            return output.ToString();
        }

        int RequireLetter(char letter)
        {
            int index = Alphabet.IndexOf(letter);
            if (index < 0)
            {
                throw new ArgumentException($"'{letter}' is not a letter of this machine.");
            }
            return index;
        }
    }
}
