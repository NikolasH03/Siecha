using UnityEngine;

public class ArcabuzDisparo : MonoBehaviour
{
    public float rangoDisparo = 5f;
    public float radioImpacto = 3f;
    private ControladorCombate player;
    public LayerMask capaEnemigos;
    public Transform puntoDisparo;

    public void Awake()
    {
        player = GetComponent<ControladorCombate>();
    }
    public void Disparar()
    {
       
        Collider[] enemigosImpactados = Physics.OverlapSphere(puntoDisparo.position + transform.forward * 2f, radioImpacto, capaEnemigos);

        foreach (Collider enemigo in enemigosImpactados)
        {
            HealthComp salud = enemigo.GetComponent<HealthComp>();
            if (salud != null)
            {
                salud.recibeDano(player.EntregarDanoArmaDistancia());
                salud.setRecibiendoDano(true);
            }
        }

    }

    private void OnDrawGizmos()
    {
        if (puntoDisparo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoDisparo.position + transform.forward * 2f, radioImpacto);
        }
    }
}

