using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


public class INputchanger : MonoBehaviour
{
    public string[] Joysticks;
    public int valor;
    void Update() 
    {
        Joysticks = Input.GetJoystickNames();
      
        
        controle();
    }

     public void controle()
    {
        for (int i = 0; i < 20 ; i++)
        {
            if (Input.GetKeyDown("joystick " + valor + " button " + i)) {          
                print("controle" + i);
            }
            if (Input.GetKeyDown("joystick 3 button " + i))
            {
                print("Controle Xbox " + i);
            }
            if (Input.GetKeyDown("joystick 4 button " + i))
            {
                print("Controle Generico " + i);
            }
        }
}
}

