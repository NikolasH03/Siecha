using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLogic : MonoBehaviour
{

    [SerializeField] Player player;
    [SerializeField] int tipo;

    //1. crece
    //2. aumenta velocidad base
    //3. desaparece el sprint

    void Start()
    {
        player=GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
   public void efecto()
    {
        switch (tipo)
        {
            case 1:
                player.gameObject.transform.localScale = new Vector3(3,3,3);
                break;

            case 2:
               float speed1= player.returnSpeed();
                speed1 += 5;
                player.setSpeed(speed1);
                break;


            default:
                Debug.Log("ningun efecto");
                break;

        }
    }
}
