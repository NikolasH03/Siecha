using UnityEngine;

public class DetectorImpactoMelee : MonoBehaviour
{
    [SerializeField] private string tagEnemigo = "enemy";
    private HealthComp enemigo;
    private ControladorCombate player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagEnemigo))
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorCombate>();

            enemigo = other.GetComponent<HealthComp>();
            enemigo.recibeDano(player.EntregarDanoArmaMelee(enemigo.getBloqueando()));
            enemigo.setRecibiendoDano(true);

            if (enemigo.EstaEsquivando) return;

            if (enemigo.getBloqueando())
            {
                player.Reproducir("golpe_bloqueado");
            }

            else
            {
                player.Reproducir("golpe_impacta");
            }

        }
    }
}

