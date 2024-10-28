using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleEspanol : StateMachineBehaviour
{
    private Player player;
    private logicaEnemigo enemigo;
    AudioManager audioManager;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
        player = animator.GetComponent<Player>();
        audioManager = GameObject.FindGameObjectWithTag("audio").GetComponent<AudioManager>();
        

    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemigo = GameObject.FindGameObjectWithTag("enemy").GetComponent<logicaEnemigo>();
        if (player == null)
            return;

        if (player.atacandoDebil)
        {
            player.anim.Play("ataque");
            audioManager.playAudio(audioManager.attack);
            player.numero_golpesDebiles++;
            enemigo.tipoDeDaño("ligero");
            //logicaEnemigo.instance.tipoDeDaño("ligero");
        }
        if (player.atacandoFuerte)
        {
            player.anim.Play("ataqueFuerte1");
            audioManager.playAudio(audioManager.heavyAttack);
            player.numero_golpesFuertes++;
            enemigo.tipoDeDaño("fuerte");
            //logicaEnemigo.instance.tipoDeDaño("fuerte");
        }
      
        if (player.anim.GetBool("blocking"))
        {
            player.anim.Play("bloqueando");
           
            
        } 
        if (player.isDashing)
        {
            player.anim.Play("dash");
        }
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
            return;

        player.atacando = false;
        player.atacandoDebil = false;
        player.atacandoFuerte = false;
        player.isDashing = false;

    }


}
