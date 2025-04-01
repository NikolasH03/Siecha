using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : StateMachineBehaviour
{
    private ControladorCombate player;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
        player = animator.GetComponent<ControladorCombate>();

    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        
        if (player == null)
            return;

        if (player.numeroGolpesLigeros==1)
        {
            player.anim.Play("ataque");
            ControladorSonido.instance.playAudio(ControladorSonido.instance.attack);
            player.numeroGolpesLigeros++;
            player.tipoAtaque = "ligero";

        }
        if (player.numeroGolpesFuertes==1)
        {
            player.anim.Play("ataqueFuerte1");
            ControladorSonido.instance.playAudio(ControladorSonido.instance.heavyAttack);
            player.numeroGolpesFuertes++;
            player.tipoAtaque = "fuerte";
            
        }

        if (player.anim.GetBool("blocking"))
        {
            player.anim.Play("bloqueando");

        }

        if (player.anim.GetBool("dashing"))
        {
            player.anim.Play("dash");
            //player.anim.SetBool("dashing", false);
            player.GetComponent<Collider>().enabled = false;
            player.GetComponent<Rigidbody>().isKinematic = true;

        }

        if (player.PuedeUsarCapoeira()  && Input.GetKeyDown(KeyCode.T))
        {
            player.anim.Play("Capoeira");
        }
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
            return;

        player.setAtacando(false);
        

    }


}
