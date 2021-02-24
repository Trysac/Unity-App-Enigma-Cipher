using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ordenamiento : MonoBehaviour
{
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

    void Start()
    {

    }

    void Update()
    {
        
    }


}
