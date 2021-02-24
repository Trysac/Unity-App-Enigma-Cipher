using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUI : MonoBehaviour
{
    [Header("Bloqueo de opciones")]
    [SerializeField] Image bloque_1;
    [SerializeField] Image bloque_2;

    [Header("Cifrado - Mensaje")]
    [SerializeField] Text mensaje;
    [SerializeField] Text consola;
    [SerializeField] Text warning;

    [Header("Opciones de cifrado")]
    [SerializeField] Text clave;


    string clave_Ordenamiento = " ";
    string mensaje_usuario = " ";

    bool confiRotoresBloqueado = true;
    bool confiClaveBloqueado = true;

    void Start()
    {

    }


    void Update()
    {
        
    }

    public void DesbloquearBloquearConfiguracionRotores()
    {
        if (confiRotoresBloqueado) 
        {
            confiRotoresBloqueado = false;
            bloque_1.enabled = false;
        }
        else 
        {
            confiRotoresBloqueado = true;
            bloque_1.enabled = true;
        }
    }

    public void DesbloquearBloquearConfiguracionClave()
    {
        if (confiClaveBloqueado)
        {
            confiClaveBloqueado = false;
            bloque_2.enabled = false;
        }
        else
        {
            confiClaveBloqueado = true;
            bloque_2.enabled = true;
        }
    }
}
