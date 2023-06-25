using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// Class for controlling the overall GUI.
public class UIController : MonoBehaviour
{
    /// The parent canvas of all UI elements.
    public Canvas Canvas{get;private set;}

    private UISimulationController simulationUI;

    void Awake()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.UIController = this;

        Canvas = GetComponent<Canvas>();
        simulationUI = GetComponentInChildren<UISimulationController>(true);

        simulationUI.Show();
    }

    /// Sets the CarController from which to get the data from to be displayed.
    public void SetDisplayTarget(CarController target)
    {
        simulationUI.Target = target;
    }
    public void OpenScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }
}
