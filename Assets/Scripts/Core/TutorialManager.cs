using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string id;
        public bool alreadyShown;
    }

    [SerializeField] private TutorialStep[] pasosDeTutorial;

    public void MostrarTutorial(string id)
    {
        //var paso = System.Array.Find(pasosDeTutorial, t => t.id == id && !t.alreadyShown);
        //if (paso == null)
        //{
        //    Debug.Log($"Tutorial con ID '{id}' ya mostrado o no encontrado.");
        //    return;
        //}

        //paso.alreadyShown = true;

        // Delegar al MenuManager la apertura del panel
        MenuManager.Instance.AbrirPanelTutorial(1);
    }

    public void ReiniciarTutoriales()
    {
        foreach (var paso in pasosDeTutorial)
            paso.alreadyShown = false;
    }
}
