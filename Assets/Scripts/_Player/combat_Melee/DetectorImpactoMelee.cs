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
            player.ReproducirVFX(2, 1);
            player.ReproducirSonido(2, 1);

            enemigo = other.GetComponent<HealthComp>();
            enemigo.recibeDano(player.EntregarDañoArmaMelee(enemigo.getBloqueando()));
            enemigo.setRecibiendoDano(true);

        }
    }
}

