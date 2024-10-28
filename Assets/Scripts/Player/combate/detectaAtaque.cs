using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detectaAtaque : MonoBehaviour
{
    [SerializeField] HealthBar healthBar;
    [SerializeField] Player player;
    [SerializeField] weaponController controlArma;
    void Start()
    {
        healthBar.detectaAtaque = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "hostile")
        {
            healthBar.detectaAtaque = true;
        }
    }

    public void playerDead()
    {
        healthBar.playerDied = true;
            player.canMove = true;   
    }
    public void FinishDamage()
    {

            player.GetComponent<Collider>().enabled = true;
        player.GetComponent<Rigidbody>().isKinematic = false;
        player.canMove = true;
        player.isSprinting=false;



    }
    public void canShoot()
    {
        controlArma.shooting = false;
    }
}
