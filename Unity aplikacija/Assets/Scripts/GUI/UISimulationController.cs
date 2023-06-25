using UnityEngine.UI;
using UnityEngine;
using System;

/// Class for controlling the various ui elements of the simulation
public class UISimulationController : MonoBehaviour
{
    private CarController target;

    /// The Car to fill the GUI data with.
    public CarController Target
    {
        get { return target; }
        set
        {
            if (target != value)
            {
                target = value;
            }
        }
    }

    // GUI element references to be set in Unity Editor.
    [SerializeField]
    private Text[] InputTexts;
    [SerializeField]
    private Text Evaluation;
    [SerializeField]
    private Text GenerationCount;

    void Update()
    {
        if (Target != null)
        {
            //Display controls
            if (Target.CurrentControlInputs != null)
            {
                for (int i = 0; i < InputTexts.Length; i++)
                    InputTexts[i].text = Target.CurrentControlInputs[i].ToString();
            }

            //Display evaluation and generation count
            Evaluation.text = Target.Agent.Genotype.Evaluation.ToString();
            GenerationCount.text = EvolutionManager.Instance.GenerationCount.ToString();
        }
    }
    /// Starts to display the gui elements.
    public void Show()
    {
        gameObject.SetActive(true);
    }
    /// Stops displaying the gui elements.
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
