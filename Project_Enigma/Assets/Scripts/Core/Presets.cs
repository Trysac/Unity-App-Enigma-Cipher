using System.Collections.Generic;

namespace EnigmaCipher.Core
{
    /// <summary>Name, wiring and turnover letters of a rotor model.</summary>
    public readonly struct RotorSpec
    {
        public readonly string Name;
        public readonly string Wiring;
        public readonly string Notches;

        public RotorSpec(string name, string wiring, string notches)
        {
            Name = name;
            Wiring = wiring;
            Notches = notches;
        }
    }

    /// <summary>A set of rotors and reflectors that work on the same alphabet.</summary>
    public sealed class MachineModel
    {
        public string Name { get; }
        public Alphabet Alphabet { get; }
        public IReadOnlyList<RotorSpec> Rotors { get; }
        /// <summary>Reflector names and wirings.</summary>
        public IReadOnlyList<KeyValuePair<string, string>> Reflectors { get; }

        public MachineModel(string name, Alphabet alphabet, RotorSpec[] rotors, KeyValuePair<string, string>[] reflectors)
        {
            Name = name;
            Alphabet = alphabet;
            Rotors = rotors;
            Reflectors = reflectors;
        }

        public Rotor CreateRotor(int index) =>
            new Rotor(Rotors[index].Name, Alphabet, Rotors[index].Wiring, Rotors[index].Notches);

        public Reflector CreateReflector(int index) =>
            new Reflector(Reflectors[index].Key, Alphabet, Reflectors[index].Value);

        /// <summary>Machine with the given rotors (left to right) and reflector, all at A.</summary>
        public EnigmaMachine Build(int reflector, params int[] rotorsLeftToRight)
        {
            var rotors = new List<Rotor>();
            foreach (int index in rotorsLeftToRight)
            {
                rotors.Add(CreateRotor(index));
            }
            return new EnigmaMachine(Alphabet, CreateReflector(reflector), rotors);
        }
    }

    public static class Presets
    {
        /// <summary>
        /// The Wehrmacht and Luftwaffe Enigma I with rotors I-V and reflectors B and C,
        /// using the historical wirings.
        /// </summary>
        public static readonly MachineModel EnigmaI = new MachineModel(
            "Enigma I",
            Alphabet.Latin,
            new[]
            {
                new RotorSpec("I", "EKMFLGDQVZNTOWYHXUSPAIBRCJ", "Q"),
                new RotorSpec("II", "AJDKSIRUXBLHWTMCQGZNPYFVOE", "E"),
                new RotorSpec("III", "BDFHJLCPRTXVZNYEIWGAKMUSQO", "V"),
                new RotorSpec("IV", "ESOVPZJAYQUIRHXLNFTGKDCMWB", "J"),
                new RotorSpec("V", "VZBRGITYUPSDNHLXAWMJQOFECK", "Z"),
            },
            new[]
            {
                new KeyValuePair<string, string>("B", "YRUHQSLDPXNGOKMIEBFZCWVJAT"),
                new KeyValuePair<string, string>("C", "FVPJIAOYEDRZXWGCTKUQSBNMHL"),
            });

        /// <summary>
        /// A fictional machine for Spanish text: 27 letters plus the space.
        /// Wirings were generated once from a fixed seed (1918, the year of
        /// Scherbius's rotor machine patent) and are fixed here.
        /// </summary>
        public static readonly MachineModel Spanish = new MachineModel(
            "Enigma ES",
            Alphabet.Spanish,
            new[]
            {
                new RotorSpec("I", "QD ZJBAMTXÑNOEVLHWSUFKYIPGRC", "H"),
                new RotorSpec("II", "YPJELCIMQZRBVSN OGÑKFDUXTWHA", "J"),
                new RotorSpec("III", "WCKVNALXZEITYBDRJPQOÑSFH UGM", "L"),
                new RotorSpec("IV", "LVRUBWHI KZSYÑQTJGDCEPXAMFON", "S"),
                new RotorSpec("V", "BC FSEXUVZAQGDOWJHPLIYÑRMKTN", "O"),
            },
            new[]
            {
                new KeyValuePair<string, string>("ES", "ZCBMTVRKL HIDYXPOSGQEWFUÑNAJ"),
            });

        public static readonly IReadOnlyList<MachineModel> All = new[] { EnigmaI, Spanish };
    }
}
