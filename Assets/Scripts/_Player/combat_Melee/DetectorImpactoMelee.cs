using UnityEngine;

public class DetectorImpactoMelee : MonoBehaviour
{
    [SerializeField] private string tagEnemigo = "enemy";
    HealthbarEnemigo enemigo;
    ControladorCombate player;

    private void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorCombate>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagEnemigo))
        {
            player.ReproducirVFX(2, 1);
            player.ReproducirSonido(2, 1);

            enemigo = other.GetComponent<HealthbarEnemigo>();
            enemigo.recibeDaño(player.EntregarDañoArmaMelee());
            enemigo.setRecibiendoDaño(true);

        }
    }
}

