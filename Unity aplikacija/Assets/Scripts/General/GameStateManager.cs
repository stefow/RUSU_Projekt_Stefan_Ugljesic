using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
/// Singleton class managing the overall simulation.
public class GameStateManager : MonoBehaviour
{
    [SerializeField]
    private CameraMovement Camera;

    [SerializeField]
    public string TrackName;
    public GameObject testCarPrefab;
    public Dropdown trainTrack;
    public Dropdown selectedModel;
    public Dropdown testTrack;
    public Text carPopulation, numberOfGenerations;
    public EvolutionManager em;
    public UIController UIController{get;set;}
    public Canvas endCanv;
    public InputField modelName;
    public static GameStateManager Instance{get;private set;}
    public Camera cam;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple GameStateManagers in the Scene.");
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "TrainedModels");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string[] files = Directory.GetFiles(folderPath, "*.nn");
        List<string> options = files.Select(Path.GetFileNameWithoutExtension).ToList();
        selectedModel.ClearOptions();
        selectedModel.AddOptions(options);
    }
    public async void startTraining(string trackName)
    {
        em.PopulationSize = Convert.ToInt32(carPopulation.text);
        trackName = trainTrack.options[trainTrack.value].text;
        em.RestartAfter=Convert.ToInt32(numberOfGenerations.text);

        SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
        SceneManager.LoadScene(trackName, LoadSceneMode.Additive);
        TrackName=trackName;

        await Task.Delay(TimeSpan.FromSeconds(0.4));
        TrackManager.Instance.BestCarChanged += OnBestCarChanged;
        EvolutionManager.Instance.StartEvolution();
    }
    public async void startTesting(string trackName)
    {
        trackName = testTrack.options[testTrack.value].text;
        SceneManager.LoadScene(trackName, LoadSceneMode.Additive);
        TrackName = trackName;
        await Task.Delay(TimeSpan.FromSeconds(0.4));
        testCarPrefab.GetComponent<CarTestController>().modelName = selectedModel.options[selectedModel.value].text;
        testCarPrefab.GetComponent<CarTestController>().trackName = trackName;
        Transform spawn = GameObject.Find("TrackManager").GetComponent<TrackManager>().PrototypeCar.transform;
        GameObject car= Instantiate(testCarPrefab,spawn.position,spawn.rotation);
        cam.GetComponent<CameraMovement>().SetTarget(car);
    }
    private CarController bestCarToSave;
    void Update ()
    {
        if(EvolutionManager.Instance.started)
        {
            if (EvolutionManager.Instance.GenerationCount == Convert.ToInt32(numberOfGenerations.text))
            {
                if (EvolutionManager.Instance.AgentsAliveCount == 0)
                {
                    string saveFolder = "TrainedModels";
                    string savePath = Path.Combine(Application.persistentDataPath, saveFolder, modelName.text + ".nn");

                    if (!Directory.Exists(Path.Combine(Application.persistentDataPath, saveFolder)))
                    {
                        Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, saveFolder));
                    }
                        bestCarToSave.Agent.FNN.SaveModel(savePath);
                        Debug.Log(bestCarToSave.Agent.FNN.ToString());
                        GameObject.Find("UI").GetComponent<Canvas>().enabled = false;
                        endCanv.enabled = true;
                        EvolutionManager.Instance.started = false;
                }
            }
        }
    }

    private void OnBestCarChanged(CarController bestCar)
    {
        if (bestCar == null)
        {
            Camera.SetTarget(null);
        }
        else
        {
            Camera.SetTarget(bestCar.gameObject);
            bestCarToSave = bestCar;
        }

        if (UIController != null)
            UIController.SetDisplayTarget(bestCar);
    }
}
