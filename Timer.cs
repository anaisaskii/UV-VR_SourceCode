using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float timePassed = 0;
    public TextMeshProUGUI TimerTextDisplay;
    public float timeRemaining = 180f;

    private float shapeStartTime;
    public SaveDataToCSV dataManager; // Reference to DataManager
    public CubeManager cubeManager; // Needed to pass cubesChosenSet info

    void Start()
    {
        shapeStartTime = Time.time;
    }

    void Update()
    {
        // Count down timer each second and display as text
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            // If in MRT save data to CSV
            // If not, (in unwrapping scene), progress to the mental rotations test
            if(SceneManager.GetActiveScene().name == "MRTScene")
            {
                cubeManager.SaveDataToCSV();
            }
            else
            {
                SceneManager.LoadScene("MRTScene");
            }
            
        }

        // Seperate time into seconds/minutes for readability
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        TimerTextDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary> Log the time it took to complete a MRT question </summary>
    public void LogShapeTime(string shapeName, string chosenShape, bool isCorrect)
    {
        float shapeCompletionTime = Time.time - shapeStartTime;
        dataManager.AddShapeData(shapeName, shapeCompletionTime, isCorrect, chosenShape);
        shapeStartTime = Time.time; // Reset timer
    }
}