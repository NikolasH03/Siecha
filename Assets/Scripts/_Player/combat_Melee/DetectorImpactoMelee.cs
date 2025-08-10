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
            Debug.Log($"Impacto contra: {other.name}");

            player = GameObject.FindGameObjectWithTag("Player").GetComponent<ControladorCombate>();
            enemigo = other.GetComponentInParent<HealthComp>();

            if (enemigo != null)
            {
                int dano = player.EntregarDanoArmaMelee();
                Debug.Log($"Daño entregado: {dano}");
                enemigo.recibeDano(dano);
            }
            else
            {
                Debug.LogWarning("No se encontró HealthComp en el objetivo.");
            }
        }
    }

}

