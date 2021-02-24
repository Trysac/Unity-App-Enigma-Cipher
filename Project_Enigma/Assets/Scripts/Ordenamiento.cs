using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ordenamiento : MonoBehaviour
{
    [Header("Rotor Espejo")]
    [SerializeField] Dropdown[] optionsList;

    [Header("Rotores")]
    [SerializeField] Text R1A;
    [SerializeField] Text R1B;
    [SerializeField] Text R2A;
    [SerializeField] Text R2B;
    [SerializeField] Text R3A;
    [SerializeField] Text R3B;
    [SerializeField] Text R4A;
    [SerializeField] Text R4B;
    [SerializeField] Text R5A;
    [SerializeField] Text R5B;
    [SerializeField] Text R6A;
    [SerializeField] Text R6B;
    [SerializeField] Text R7A;
    [SerializeField] Text R7B;
    [SerializeField] Text R8A;
    [SerializeField] Text R8B;

    [Header("Rotor Espejo")]
    [SerializeField] Text rotorEspejo;
    [SerializeField] Text index;

    char[] ABC_Ordenado = {'A','B','C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'Ñ', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    char[] ABC_Invertido = {'Z', 'Y', 'X', 'W', 'V', 'U', 'T', 'S', 'R', 'Q', 'P', 'O', 'Ñ', 'N', 'M', 'L', 'K', 'J', 'I', 'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A'};
    const string ordenado = "A\nB\nC\nD\nE\nF\nG\nH\nI\nJ\nK\nL\nM\nN\nÑ\nO\nP\nQ\nR\nS\nT\nU\nV\nW\nX\nY\nZ";
    const string invertido = "Z\nY\nX\nW\nV\nU\nT\nS\nR\nQ\nP\nO\nÑ\nN\nM\nL\nK\nJ\nI\nH\nG\nF\nE\nD\nC\nB\nA";


    bool rotorAB = true;

    void Start()
    {

    }

    void Update()
    {

    }

    public string OrdenAleatorioRotor() 
    {
        string[] nuevoABC = { " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " ", " "};
        
        for (int i = 0; i < 27; i++) 
        {
            int random = Random.Range(0,27);
            if (nuevoABC[random] == " ") 
            {
                nuevoABC[random] = ABC_Ordenado[i].ToString();
            }
            else 
            {
                i--;
            }
        }

        string ABC = "";

        for (int i = 0; i < 27; i++) 
        {
            if (i < 26) 
            {
                ABC += nuevoABC[i] + "\n";
            }
            else if (i == 26) 
            {
                ABC += nuevoABC[i];
            }
        }

        return ABC;
    }

    public void OrdenarRotoresConfiguracion() 
    {
        int numRotor = 0;
        rotorAB = true;

        foreach (Dropdown option in optionsList) 
        {
            Text field = GetCorrectField(numRotor, rotorAB);
            if (option.value == 0)
            {
                field.text = ordenado;
                CambiarCondicional(rotorAB);
                numRotor++;
            }
            else if (option.value == 1)
            {
                field.text = invertido;
                CambiarCondicional(rotorAB);
                numRotor++;
            }
            else if (option.value == 2)
            {
                field.text = OrdenAleatorioRotor();
                CambiarCondicional(rotorAB);
                numRotor++;
            }
        }
    }

    private Text GetCorrectField(int numRotor, bool rotorAB) 
    {
        if (numRotor == 0 || numRotor == 1) 
        {
            if (rotorAB) 
            {
                return R1A;
            }
            else 
            {
                return R1B;
            }
        }
        else if (numRotor == 2 || numRotor == 3)
        {
            if (rotorAB)
            {
                return R2A;
            }
            else
            {
                return R2B;
            }
        }
        else if (numRotor == 4 || numRotor == 5)
        {
            if (rotorAB)
            {
                return R3A;
            }
            else
            {
                return R3B;
            }
        }
        else if (numRotor == 6 || numRotor == 7)
        {
            if (rotorAB)
            {
                return R4A;
            }
            else
            {
                return R4B;
            }
        }
        else if (numRotor == 8 || numRotor == 9)
        {
            if (rotorAB)
            {
                return R5A;
            }
            else
            {
                return R5B;
            }
        }
        else if (numRotor == 10 || numRotor == 11)
        {
            if (rotorAB)
            {
                return R6A;
            }
            else
            {
                return R6B;
            }
        }
        else if (numRotor == 12 || numRotor == 13)
        {
            if (rotorAB)
            {
                return R7A;
            }
            else
            {
                return R7B;
            }
        }
        else if (numRotor == 14 || numRotor == 15)
        {
            if (rotorAB)
            {
                return R8A;
            }
            else
            {
                return R8B;
            }
        }

        return null;
    }

    public void CambiarCondicional(bool state)
    {
        if (state) 
        {
            rotorAB = false;
        }
        else 
        {
            rotorAB = true;
        }
    }

    public void OrdenarRotoresClave()
    {
        print("Ordenando rotores por clave");
    }
    public void OrdenarRotorEspejo()
    {
        print("Ordenando rotor espejo");
    }

}
