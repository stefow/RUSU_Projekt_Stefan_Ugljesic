using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CarTestController : MonoBehaviour
{
    public CarMovement Movement { get; private set; }
    public string modelName="";
    public string trackName = "";
    public double[] CurrentControlInputs { get { return Movement.CurrentInputs; } }
    private Sensor[] sensors;
    NeuralNetwork nn;
    void Awake()
    {
        Movement = GetComponent<CarMovement>();
        sensors = GetComponentsInChildren<Sensor>();
    }
    void Start()
    {
        string loadFolder = "TrainedModels";
        string loadPath = Path.Combine(Application.persistentDataPath, loadFolder, modelName + ".nn");
        nn = NeuralNetwork.LoadModel(loadPath);
        Movement.HitWall += Die;
        GameObject.Find("TrackNameText").GetComponent<TextMeshProUGUI>().text = trackName;
        GameObject.Find("TestModelNameText").GetComponent<TextMeshProUGUI>().text = modelName;
        GameObject.Find("TestModelCanvas").GetComponent<Canvas>().enabled = true;
    }
    public void Restart()
    {
        Movement.enabled = true;
        foreach (Sensor s in sensors)
            s.Show();

        this.enabled = true;
    }
    void FixedUpdate()
    {
        double[] sensorOutput = new double[sensors.Length];
        for (int i = 0; i < sensors.Length; i++)
            sensorOutput[i] = sensors[i].Output;

        double[] controlInputs = nn.ProcessInputs(sensorOutput);
        Movement.SetInputs(controlInputs);

    }
    private void Die()
    {
        GameObject.Find("TestModelEndText").GetComponent<Text>().text = "Testing model finished!";
        
        this.enabled = false;
        Movement.Stop();
        Movement.enabled = false;

        foreach (Sensor s in sensors)
            s.Hide();
    }
}
