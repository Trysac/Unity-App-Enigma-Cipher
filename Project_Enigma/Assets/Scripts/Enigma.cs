using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enigma : MonoBehaviour
{
    char[] ABC_Ordenado = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'Ñ', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

    bool rotorAB = true;
    int numRetornoRotor = 0;


    public void CifrarMensaje() 
    {
        if (GetComponent<GeneralUI>().GetMensajeUsuario() == null) { return; }
        ProcesoDeCifrado();
    }

    private void ProcesoDeCifrado() 
    {
        string mensaje = GetComponent<GeneralUI>().GetMensajeUsuario();
        char[] caracteresMensaje = mensaje.ToCharArray();

        char[] ABCrotor;
        char[] ABCrotor_2;
        char[] ABCrotor_Espejo;
        bool rotorA = true;
        bool rotorB = false;
        numRetornoRotor = 0;

        string salida = "";

        for (int posicionArregloMensaje = 0; posicionArregloMensaje < caracteresMensaje.Length; posicionArregloMensaje++) 
        {
            if (caracteresMensaje[posicionArregloMensaje].Equals(' '))
            {
                salida += " ";
                continue;
            }

            string letraMensaje = caracteresMensaje[posicionArregloMensaje].ToString();

            int indexInicioCifrado = 0;
            string letraEnTransito = "";

            //Posicion de inicio cifrado, el siguiente paso es ir a los rotores
            for (int z = 0; z < ABC_Ordenado.Length; z++)
            {
                string caracterAbecedario = ABC_Ordenado[z].ToString();

                if (caracterAbecedario.Equals(letraMensaje.ToUpper()))
                {
                    indexInicioCifrado = z;
                }
            }

            bool ProcesoDeIdaYVuelta = false;
            //Toca ir a los rotores
            for (int indexRotor = 0; indexRotor < 17; indexRotor += 2) 
            {
                if (indexRotor > 15 && ProcesoDeIdaYVuelta) { break; }

                //Rotor espejo
                if (indexRotor >=15 && !ProcesoDeIdaYVuelta) 
                {
                    //Obtenemos ABC espejo y normalizamos
                    char[] ABCrotorEspejo = GetComponent<Ordenamiento>().GetABCRotorEspejo().text.ToCharArray();
                    char[] ABCrotorEspejoNormalizado = new char[27];
                    for (int i = 0, x = 0; i < ABCrotorEspejo.Length; i++)
                    {
                        if (ABCrotorEspejo[i].Equals('\n') || ABCrotorEspejo[i].Equals(' ')) { continue; }
                        ABCrotorEspejoNormalizado[x] = ABCrotorEspejo[i];
                        x++;
                    }

                    int nuevoIndexEspejo = 0;
                    letraEnTransito = ABCrotorEspejoNormalizado[indexInicioCifrado].ToString().ToUpper();
                    print("Cambio valores");
                    for (int x = 0; x < ABCrotorEspejoNormalizado.Length; x++) 
                    {
                        string letraABC = ABCrotorEspejoNormalizado[x].ToString().ToUpper();
                        if (letraABC.Equals(letraEnTransito))
                        {
                            nuevoIndexEspejo += x;
                        }
                    }

                    if (indexInicioCifrado != nuevoIndexEspejo) 
                    {
                        if (nuevoIndexEspejo > 26) 
                        {
                            indexInicioCifrado = nuevoIndexEspejo - 26;
                        }
                        else 
                        {
                            indexInicioCifrado = nuevoIndexEspejo;
                        }     
                    }

                    while (indexInicioCifrado > 26) 
                    { 
                        indexInicioCifrado = indexInicioCifrado - 26;
                    }

                    //Volver a empezar pero a la inversa       
                    rotorA = !true;
                    rotorB = !false;
                    indexRotor = -2;
                    ProcesoDeIdaYVuelta = true;
                    print(indexInicioCifrado);
                    print("primera vuelta");
                    continue;
                }

                //Obtenemos el ABC en el rotor y Normalizamos 
                //Los rotores se invierten al regresar
                ABCrotor = GetComponent<Ordenamiento>().GetCorrectField(numRetornoRotor, rotorB).text.ToCharArray();
                char[] ABCrotorNormalizado = new char[27];
                for (int i = 0, x = 0; i < ABCrotor.Length; i++)
                {
                    if (ABCrotor[i].Equals('\n') || ABCrotor[i].Equals(' ')) { continue; }
                    ABCrotorNormalizado[x] = ABCrotor[i];
                    x++;
                }

                ABCrotor_2 = GetComponent<Ordenamiento>().GetCorrectField(numRetornoRotor, rotorA).text.ToCharArray();
                char[] ABCrotor_2_Normalizado = new char[27];
                for (int i = 0, x = 0; i < ABCrotor_2.Length; i++)
                {
                    if (ABCrotor_2[i].Equals('\n') || ABCrotor_2[i].Equals(' ')) { continue; }
                    ABCrotor_2_Normalizado[x] = ABCrotor_2[i];
                    x++;
                }

                letraEnTransito = ABCrotorNormalizado[indexInicioCifrado].ToString().ToUpper();

                for (int y = 0; y < ABCrotor_2_Normalizado.Length; y++) 
                {
                    string letraABC = ABCrotor_2_Normalizado[y].ToString().ToUpper();
                    if (letraABC.Equals(letraEnTransito)) 
                    {
                        indexInicioCifrado = y;
                        break;
                    }
                }

            }
            salida += ABC_Ordenado[indexInicioCifrado].ToString();
        }

        GetComponent<GeneralUI>().SetConsola(salida);
    }

    public void DecifrarMensaje()
    {
        if (GetComponent<GeneralUI>().GetMensajeUsuario() == null) { return; }
        ProcesoDeDecifrado();
    }

    //TODO: Revisar proceso, es posible que el error este en la aplicacion de los indices, es decir, es como seguimos el proceso de forma inversa en el rotor espejo especificamente.
    private void ProcesoDeDecifrado()
    {
        string mensaje = GetComponent<GeneralUI>().GetMensajeUsuario();
        char[] caracteresMensaje = mensaje.ToCharArray();

        char[] ABCrotor;
        char[] ABCrotor_2;
        char[] ABCrotor_Espejo;
        bool rotorA = true;
        bool rotorB = false;
        numRetornoRotor = 0;

        string salida = "";

        for (int posicionArregloMensaje = 0; posicionArregloMensaje < caracteresMensaje.Length; posicionArregloMensaje++) 
        {
            if (caracteresMensaje[posicionArregloMensaje].Equals(' '))
            {
                salida += " ";
                continue;
            }

            string letraMensaje = caracteresMensaje[posicionArregloMensaje].ToString();

            int indexInicioCifrado = 0;
            string letraEnTransito = "";

            //Posicion de inicio cifrado, el siguiente paso es ir a los rotores
            for (int z = 0; z < ABC_Ordenado.Length; z++)
            {
                string caracterAbecedario = ABC_Ordenado[z].ToString();

                if (caracterAbecedario.Equals(letraMensaje.ToUpper()))
                {
                    indexInicioCifrado = z;
                }
            }

            bool ProcesoDeIdaYVuelta = false;
            //Toca ir a los rotores
            for (int indexRotor = 0; indexRotor < 17; indexRotor += 2) 
            {
                if (indexRotor > 15 && ProcesoDeIdaYVuelta) { break; }

                //Rotor espejo
                if (indexRotor >=15 && !ProcesoDeIdaYVuelta) 
                {
                    //Obtenemos ABC espejo y normalizamos
                    char[] ABCrotorEspejo = GetComponent<Ordenamiento>().GetABCRotorEspejo().text.ToCharArray();
                    char[] ABCrotorEspejoNormalizado = new char[27];
                    for (int i = 0, x = 0; i < ABCrotorEspejo.Length; i++)
                    {
                        if (ABCrotorEspejo[i].Equals('\n') || ABCrotorEspejo[i].Equals(' ')) { continue; }
                        ABCrotorEspejoNormalizado[x] = ABCrotorEspejo[i];
                        x++;
                    }

                    int nuevoIndexEspejo = 0;
                    letraEnTransito = ABCrotorEspejoNormalizado[indexInicioCifrado].ToString().ToUpper();
                    for (int x = 0; x < ABCrotorEspejoNormalizado.Length; x++)
                    {
                        string letraABC = ABCrotorEspejoNormalizado[x].ToString().ToUpper();
                        if (letraABC.Equals(letraEnTransito))
                        {
                            nuevoIndexEspejo += x;
                        }
                    }

                    if (indexInicioCifrado != nuevoIndexEspejo)
                    {
                        if (nuevoIndexEspejo > 26)
                        {
                            indexInicioCifrado = nuevoIndexEspejo - 26;
                        }
                        else
                        {
                            indexInicioCifrado = nuevoIndexEspejo;
                        }
                    }

                    //Volver a empezar pero a la inversa
                    rotorA = !true;
                    rotorB = !false;
                    indexRotor = -2;
                    ProcesoDeIdaYVuelta = true;
                    continue;
                }

                //Obtenemos el ABC en el rotor y Normalizamos 
                //Los rotores se invierten al regresar
                ABCrotor = GetComponent<Ordenamiento>().GetCorrectField(numRetornoRotor, rotorA).text.ToCharArray();
                char[] ABCrotorNormalizado = new char[27];
                for (int i = 0, x = 0; i < ABCrotor.Length; i++)
                {
                    if (ABCrotor[i].Equals('\n') || ABCrotor[i].Equals(' ')) { continue; }
                    ABCrotorNormalizado[x] = ABCrotor[i];
                    x++;
                }

                ABCrotor_2 = GetComponent<Ordenamiento>().GetCorrectField(numRetornoRotor, rotorB).text.ToCharArray();
                char[] ABCrotor_2_Normalizado = new char[27];
                for (int i = 0, x = 0; i < ABCrotor_2.Length; i++)
                {
                    if (ABCrotor_2[i].Equals('\n') || ABCrotor_2[i].Equals(' ')) { continue; }
                    ABCrotor_2_Normalizado[x] = ABCrotor_2[i];
                    x++;
                }

                letraEnTransito = ABCrotorNormalizado[indexInicioCifrado].ToString().ToUpper();

                for (int y = 0; y < ABCrotor_2_Normalizado.Length; y++) 
                {
                    string letraABC = ABCrotor_2_Normalizado[y].ToString().ToUpper();
                    if (letraABC.Equals(letraEnTransito)) 
                    {
                        indexInicioCifrado = y;
                        break;
                    }
                }

            }
            salida += ABC_Ordenado[indexInicioCifrado].ToString();
        }

        GetComponent<GeneralUI>().SetConsola(salida);
    }


}
