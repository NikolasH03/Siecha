using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class GrabObject : MonoBehaviour
{
    [SerializeField] Player player;
    public Collider objeto;
    public GameObject mano;
    private void OnTriggerStay(Collider other)
    {
        
        if (other.tag == "objeto")
        {
            if(Input.GetKeyDown(KeyCode.E)) {

                objeto = other;
                player.anim.Play("grab");
                
            }
        }

    }
    public void destruirObjeto()
    {
        objeto.GetComponent<ObjectLogic>().efecto();
        Destroy(objeto.gameObject);
    }
    public void objetoEsHijo ()
    {
        objeto.transform.SetParent(mano.transform);
    }
}
