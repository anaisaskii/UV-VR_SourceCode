using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class CubeManager : MonoBehaviour
{
    string CurrentShape;

    [Header("Shapes and Materials")]
    public Material cubeDisplayMat; 
    public Texture2D[] cubeTexturesSet1; // All images of cubes
    public Texture2D[] cubeTexturesSet2; // All images of cubes

    [Header("Renderers")]
    // The cubes that may be the answer
    public Renderer planeRenderer;  // The target cube image
    public Renderer[] answerRenderers;

    [Header("Chosen Set")]
    public int cubesChosenSet = 0;

    [Header("External Scripts")]
    public SaveDataToCSV savedatatocsv;
    public Timer timer;

    private Dictionary<string, List<Texture2D>> textureGroups = new Dictionary<string, List<Texture2D>>();
    private Queue<string> selectedShapesQueue = new Queue<string>();

    private Texture2D currentCorrectTexture;            // Correct shape, correct angle (on main plane)
    private Texture2D currentCorrectDifferentAngle;     // Correct shape, different angle (in answers)
    private Texture2D[] currentAnswerOptions = new Texture2D[4]; // The 4 answer options

    private int correctAnswers = 0;
    private int roundsCompleted = 0;

    void Start()
    {
        // Choose a random number between 0 and 1 to determine which cube set to use
        // (If none can be read from the CSV file)
        if (savedatatocsv.GetSetCompleted() == 3)
        {
            cubesChosenSet = Random.Range(0, 2);
        }
        else
        {
            // Set the previously completed set to be the one read from the file
            int previousSet = savedatatocsv.GetSetCompleted();
            Debug.Log("The previous set was: " + previousSet);
            //reverse
            if (previousSet == 1)
            {
                cubesChosenSet = 0;
            }
            else
            {
                cubesChosenSet = 1;
            }
            Debug.Log("The chosen set is: " + cubesChosenSet);
        }

        OrganizeTextures();
        PrepareShapeQueue();
        ChooseNextShape();
    }

    //read from csv first
    void OrganizeTextures()
    {
        if (cubesChosenSet == 1)
        {
            foreach (Texture2D tex in cubeTexturesSet1)
            {
                //split image where there's a '_'
                string shapeName = tex.name.Split('_')[0];
                if (!textureGroups.ContainsKey(shapeName))
                {
                    textureGroups[shapeName] = new List<Texture2D>();
                }
                textureGroups[shapeName].Add(tex);
            }
        }
        else
        {
            foreach (Texture2D tex in cubeTexturesSet2)
            {
                //split image where there's a '_'
                string shapeName = tex.name.Split('_')[0];
                if (!textureGroups.ContainsKey(shapeName))
                {
                    textureGroups[shapeName] = new List<Texture2D>();
                }
                textureGroups[shapeName].Add(tex);
            }
        }
    }

    void PrepareShapeQueue()
    {
        List<string> allShapes = new List<string>(textureGroups.Keys);
        ShuffleList(allShapes);
        foreach (string shape in allShapes)
        {
            selectedShapesQueue.Enqueue(shape);
        }
    }

    void ChooseNextShape()
    {
        if (selectedShapesQueue.Count == 0)
        {
            Debug.Log("All shapes completed!");
            return;
        }

        CurrentShape = selectedShapesQueue.Dequeue();
        List<Texture2D> shapeTextures = textureGroups[CurrentShape];

        // Pick random angle to display
        currentCorrectTexture = shapeTextures[Random.Range(0, shapeTextures.Count)];

        // Set main plane texture
        planeRenderer.material.mainTexture = currentCorrectTexture;

        // Pick a DIFFERENT angle for the correct answer
        do
        {
            currentCorrectDifferentAngle = shapeTextures[Random.Range(0, shapeTextures.Count)];
        }
        while (currentCorrectDifferentAngle == currentCorrectTexture);

        // Choose 3 incorrect answers
        List<Texture2D> wrongOptions = new List<Texture2D>();

        List<string> otherShapes = new List<string>(textureGroups.Keys);
        otherShapes.Remove(CurrentShape); // so that all answers are different

        ShuffleList(otherShapes);

        for (int i = 0; i < 3; i++)
        {
            string wrongShape = otherShapes[i];
            List<Texture2D> wrongShapeTextures = textureGroups[wrongShape];
            Texture2D wrongTexture = wrongShapeTextures[Random.Range(0, wrongShapeTextures.Count)];
            wrongOptions.Add(wrongTexture);
        }

        // Combine into the 4 options
        List<Texture2D> allOptions = new List<Texture2D>
        {
            currentCorrectDifferentAngle, // the only corect option
            wrongOptions[0],
            wrongOptions[1],
            wrongOptions[2]
        };

        ShuffleList(allOptions);

        // assign textures to answer slots
        for (int i = 0; i < answerRenderers.Length; i++)
        {
            answerRenderers[i].material.mainTexture = allOptions[i];
        }

        currentAnswerOptions = allOptions.ToArray();
    }

    // check the chosen shapes material against the target one
    public void CheckAnswer(int selectedIndex)
    {
        Renderer clickedRenderer = answerRenderers[selectedIndex];
        Texture2D selectedTexture = (Texture2D)clickedRenderer.material.mainTexture;


        bool isCorrect = selectedTexture.name == currentCorrectDifferentAngle.name;
        string chosenShapeName = selectedTexture.name;

        timer.LogShapeTime(CurrentShape, chosenShapeName, isCorrect);

        roundsCompleted += 1;

        if (isCorrect)
        {
            correctAnswers += 1;
        }

        if (roundsCompleted == 6)
        {
            SaveDataToCSV();
        }

        ChooseNextShape();
    }

    public void SaveDataToCSV()
    {
        savedatatocsv.SaveData(correctAnswers);
    }


    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
