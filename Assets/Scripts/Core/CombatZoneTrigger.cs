using UnityEngine;

public class CombatZoneTrigger : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
                GameFlowManager.Instance.GoToNextSection();
                gameObject.SetActive(false);
        }
    }
}
