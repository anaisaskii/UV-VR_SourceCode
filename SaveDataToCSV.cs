using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveDataToCSV : MonoBehaviour
{
    public CubeManager cubeManager; // needed for saving setCompleted info

    private List<string> shapeDetails = new List<string>();

    private int roundsCompleted = 0;
    private int setCompleted; //which set of shapes was completed last

    //
    public void AddShapeData(string shapeName, float completionTime, bool isCorrect, string chosenShape)
    {
        string correctness = isCorrect ? "Correct" : "Incorrect";
        shapeDetails.Add($"{shapeName},{completionTime:F2},{correctness},{chosenShape}");
    }

    /// <summary> Save the data as a CSV file in the persistent data path </summary>
    // On PC this path is .../AppData/LocalLow/UVVR_anaisaski/...
    // On Android this path is /sdcard/Android/data/com.unity.template.vr/files/...
    public void SaveData(int correctAnswers)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "TestTimes.csv");
        bool append = false;

        //Read file data
        if (File.Exists(filePath)) //Check if the file actually exists
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Split each line at each comma (as it's a CSV) and check for "Rounds Completed"
                    var parts = line.Split(',');
                    if (parts.Length == 2 && parts[0].Trim() == "Rounds Completed")
                    {
                        //If the value can be parsed then set it as the round number
                        if (int.TryParse(parts[1].Trim(), out int rounds))
                        {
                            roundsCompleted = rounds;
                        }
                    }
                }
            }
        }

        // Only append the file if 1 round has been completed
        // If 2 rounds have been completed, create a new file as it will be a new user
        append = (roundsCompleted == 1);

        //Write file data
        using (StreamWriter writer = new StreamWriter(filePath, append: append))
        {
            //If creating a new file...
            if (!append)
            {
                writer.WriteLine("Shape Name,Time (seconds),Correct,Chosen Shape");
                roundsCompleted = 0;
            }

            //Shape name, answer accuracy, completion time...
            foreach (string shapeDetail in shapeDetails)
            {
                writer.WriteLine(shapeDetail);
            }

            writer.WriteLine($"\nCorrect Answers,{correctAnswers}"); //total correct answers
            roundsCompleted++; // increase the rounds completed and write to file
            writer.WriteLine($"Rounds Completed, {roundsCompleted}");
            //Write which set was completed that the same set isn't completed twice
            writer.WriteLine($"Set Completed, {cubeManager.cubesChosenSet}");
            writer.WriteLine();
        }

        //Log where the file is stored to avoid any issues
        Debug.Log($"CSV file saved at: {filePath}");

        // If only one round completed,
        // Rename current file to the current date and time (to avoid duplicates)
        if (roundsCompleted >= 2)
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string newFilePath = Path.Combine(Application.persistentDataPath, $"TestTimes_{timestamp}.csv");
            File.Move(filePath, newFilePath);
            SceneManager.LoadScene("EndScene");
        }
        // If two rounds completed, progress to final scene
        else
        {
            SceneManager.LoadScene("UnwrapScene");
        }
    }

    /// <summary> Check which MRT set was completed previously </summary>
    public int GetSetCompleted()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "TestTimes.csv");

        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Split each line at the comma to check for the phrase 'Set Completed'
                    var parts = line.Split(',');
                    if (parts.Length == 2 && parts[0].Trim() == "Set Completed")
                    {
                        //if found and can be parsed, set the set completed
                        if (int.TryParse(parts[1].Trim(), out int set))
                        {
                            setCompleted = set;
                        }
                    }
                }
            }

            return setCompleted;
        }

        return 3; // If no file found, return 3 to signify to choose a random one
    }
}
