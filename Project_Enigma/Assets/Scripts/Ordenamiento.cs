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


    [Header("Rotor Espejo")]
    [SerializeField] Dropdown Espejo;
    [SerializeField] Text rotorEspejo;
    [SerializeField] Text index;

    char[] ABC_Ordenado = {'A','B','C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'Ñ', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    char[] ABC_Invertido = {'Z', 'Y', 'X', 'W', 'V', 'U', 'T', 'S', 'R', 'Q', 'P', 'O', 'Ñ', 'N', 'M', 'L', 'K', 'J', 'I', 'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A'};
    const string ordenado = "A\nB\nC\nD\nE\nF\nG\nH\nI\nJ\nK\nL\nM\nN\nÑ\nO\nP\nQ\nR\nS\nT\nU\nV\nW\nX\nY\nZ";
    const string invertido = "Z\nY\nX\nW\nV\nU\nT\nS\nR\nQ\nP\nO\nÑ\nN\nM\nL\nK\nJ\nI\nH\nG\nF\nE\nD\nC\nB\nA";


    bool rotorAB = true;

    int numRetornoRotor = 0;

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

        return null;
    }

    private void CambiarCondicional(bool state)
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
        if (GetComponent<GeneralUI>().GetClaveOrdenamiento() == null) { return; }
        OrdenarPorClave();
    }

    private void OrdenarPorClave()
    {
        string clave = GetComponent<GeneralUI>().GetClaveOrdenamiento();
        if (clave == null) { return; }

        char[] caracteresClave = clave.ToCharArray();

        char[] ABCrotor;

        numRetornoRotor = 0;
        rotorAB = true;

        int posicionArregloClave = 0;

        for (int index = 0; index < 16; index++)
        {
            if (index >= caracteresClave.Length) { break; }
            else
            {
                if (caracteresClave[index].Equals(' '))
                {
                    posicionArregloClave++;
                    continue;
                }
            }

            ABCrotor = GetABCrotores().ToCharArray();

            char[] ABCrotorNormalizado = new char[27];
            for (int i = 0, x = 0; i < ABCrotor.Length; i++)
            {
                if (ABCrotor[i].Equals('\n') || ABCrotor[i].Equals(' ')) { continue; }
                ABCrotorNormalizado[x] = ABCrotor[i];
                x++;
            }

            int posicion = 999;

            string caracterActualClave = caracteresClave[posicionArregloClave].ToString();
            for (int i = 0; i < ABCrotorNormalizado.Length; i++)
            {
                string caracterAbecedario = ABCrotorNormalizado[i].ToString();

                if (caracterAbecedario.Equals(caracterActualClave.ToUpper()))
                {
                    posicion = i;
                    break;
                }
            }

            char[] nuevoABCrotorNormalizado = new char[27];
            for (int i = posicion, salida = 0; salida <= 26; i++)
            {
                if (i >= 26)
                {
                    nuevoABCrotorNormalizado[salida] = ABCrotorNormalizado[i];
                    salida++;
                    i = -1;
                }
                else
                {
                    nuevoABCrotorNormalizado[salida] = ABCrotorNormalizado[i];
                    salida++;
                }
            }

            string ABC_Actualizado = "";

            for (int i = 0; i < nuevoABCrotorNormalizado.Length; i++) 
            {
                if (i == nuevoABCrotorNormalizado.Length - 1) 
                {
                    ABC_Actualizado += nuevoABCrotorNormalizado[i];
                }
                else 
                {
                    ABC_Actualizado += nuevoABCrotorNormalizado[i];
                    ABC_Actualizado += "\n";
                }

            }

            Text field = GetCorrectField(numRetornoRotor, rotorAB);
            CambiarCondicional(rotorAB);
            numRetornoRotor++;
            field.text = ABC_Actualizado;

            posicionArregloClave++;
        }
    }

    private string GetABCrotores()
    {
        Text field = GetCorrectField(numRetornoRotor, rotorAB);
        return field.text;
        
    }

    public void OrdenarRotorEspejo()
    {
        if (Espejo.value == 0)
        {
            rotorEspejo.text = ordenado;
        }
        else if (Espejo.value == 1)
        {
            rotorEspejo.text = invertido;
        }
        else if (Espejo.value == 2)
        {
            rotorEspejo.text = OrdenAleatorioRotor();
        }
    }

}
