using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIButtons : MonoBehaviour
{
    public Text numberOfCycles,numberOfCars;
    public Slider numberOfCarsSlider;
    public Slider numberOfCyclesSlider;
    void Start()
    {
        
    }
    void Update()
    {
        numberOfCycles.text=numberOfCarsSlider.value.ToString();
        numberOfCars.text=numberOfCyclesSlider.value.ToString();
    }
    public void OpenScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }
    public void enableCanvas(Canvas canv)
    {
        canv.enabled = true;
    }
    public void disableCanvas(Canvas canv)
    {
        canv.enabled=false;
    }
    public void quitAplication()
    {
        Application.Quit();
    }
}
