using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ShapeEdgeRaycast : MonoBehaviour
{
    [Header("Shapes")]
    public GameObject[] shapes;
    public GameObject[] SnowmanPartsInGame;
    public GameObject[] SnowmanParts;
    public GameObject buttonPodium;

    [Header("Shape Edge Materials")]
    public Material litEdge;
    public Material unlitEdge;

    [Header("VR Controls")]
    public Transform controller;
    public InputActionProperty rightTriggerAction;
    public GameObject player;
    public GameObject userGuide;

    [Header ("Video Clips")]
    public VideoPlayer VideoPlayer;
    public VideoClip[] edgeClips;
    public VideoClip[] unwrapClips;

    [Header("User Guide")]
    public bool HideUserGuide = false;

    private GameObject currentUserGuide;
    private GameObject currentShape;

    private int CurrentSnowmanPart = 0;
    private int currentShapeIndex = 0;
    private int currentBoxColliderIndex = 0;
    private int currentEdgeVideo = 0;
    private int completedShapes;

    private MeshRenderer shapeMeshRenderer;
    private BoxCollider[] boxColliders;
    private Material[] shapematerials;

    private bool FinalChallenge = false;

    private List<GameObject> completedSnowmanParts = new List<GameObject>();

    private void Start()
    {
        SpawnNewShape();
        spawnGuide();
    }

    private void Update()
    {
        //Hide the user guide for the challenges
        //unhide if the player needs it
        if (currentUserGuide)
        {
            if (HideUserGuide)
            {
                currentUserGuide.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                currentUserGuide.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        lookAtPlayer();

        if (!FinalChallenge)
        {
            ProcessStandardShapes();
        }
        else
        {
            ProcessSnowmanChallenge();
        }
    }


    void ProcessStandardShapes()
    {
        if (currentBoxColliderIndex >= boxColliders.Length) return;

        if (CheckForEdgeHit())
        {
            HandleEdgeHit();

            if (currentBoxColliderIndex >= boxColliders.Length)
            {
                // All edges completed for current shape
                Destroy(currentShape);

                //play the unwrapping video for the shape!!
                StartCoroutine(PlayVideoThenSpawnNextShape());

                currentEdgeVideo += 1;
                
                Debug.Log(currentEdgeVideo);
            }
            else
            {
                // Move guide for next edge
                spawnGuide();
            }
        }
    }

    IEnumerator PlayVideoThenSpawnNextShape()
    {
        if (completedShapes < unwrapClips.Length)
        {
            // stop looping so that the unwrap video can play once
            VideoPlayer.isLooping = false;
            VideoPlayer.clip = unwrapClips[completedShapes];
            VideoPlayer.Play();

            //while video player loads up 
            while (!VideoPlayer.isPlaying)
            {
                yield return null;  // Wait for one frame until the video starts
            }

            // Wait for video to finish
            while (VideoPlayer.isPlaying)
            {
                yield return null;
            }

            completedShapes++;  // Only increment AFTER the video is done
        }
        else
        {
            Debug.LogWarning("No matching video clip found for shape " + completedShapes);
        }

        currentShapeIndex++;

        if (currentShapeIndex < shapes.Length)
        {
            // Spawn the next shape and guide
            SpawnNewShape();
            spawnGuide();
            VideoPlayer.isLooping = true; //loop the edge video
        }
        //if all primitive shapes done, do final challenge
        else
        {
            FinalChallenge = true;
            HideUserGuide = true;
            StartSnowmanChallenge();
            Instantiate(buttonPodium);
        }
    }

    //spawn in a new shape and update the video player
    void SpawnNewShape()
    {
        if (currentShapeIndex >= shapes.Length) return;

        currentShape = Instantiate(shapes[currentShapeIndex], new Vector3(0.6f, 1.9f, 2.1f), Quaternion.Euler(-90f, 0f, -180f));
        SetupShape(currentShape);

        VideoPlayer.clip = edgeClips[currentEdgeVideo];
    }

    // set up the shape materials and colliders
    void SetupShape(GameObject shape)
    {
        shapeMeshRenderer = shape.GetComponent<MeshRenderer>();
        boxColliders = shape.GetComponentsInChildren<BoxCollider>();
        shapematerials = shapeMeshRenderer.materials;

        ResetEdges();
        currentBoxColliderIndex = 0;
    }

    // reset all the materials on the shape edges
    void ResetEdges()
    {
        shapeMeshRenderer.materials = shapematerials;
    }

    /// <summary>
    /// Cast ray from right controller.
    /// If it hits the currently active box collider, return true
    /// </summary>
    bool CheckForEdgeHit()
    {
        float triggerValue = rightTriggerAction.action.ReadValue<float>();
        if (triggerValue <= 0.1f) return false;

        Ray ray = new Ray(controller.position, controller.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f))
        {
            BoxCollider currentCollider = boxColliders[currentBoxColliderIndex];
            if (currentCollider.bounds.Contains(hit.point))
            {
                return true;
            }
        }
        return false;
    }

    // Logic for when player successfully hits an edge
    void HandleEdgeHit()
    {
        shapematerials[currentBoxColliderIndex] = litEdge;
        shapeMeshRenderer.materials = shapematerials;

        boxColliders[currentBoxColliderIndex].enabled = false;
        currentBoxColliderIndex++;

        if (currentBoxColliderIndex < boxColliders.Length)
        {
            if (!FinalChallenge)
            {
                currentEdgeVideo += 1;
                VideoPlayer.clip = edgeClips[currentEdgeVideo];
                Debug.Log(currentEdgeVideo);
            }
            
        }
    }

    //spawn in the assistive guide
    void spawnGuide()
    {
        if (currentUserGuide)
        {
            Destroy(currentUserGuide);
        }

        if (currentBoxColliderIndex < boxColliders.Length)
        {
            currentUserGuide = Instantiate(userGuide, boxColliders[currentBoxColliderIndex].bounds.center, Quaternion.identity);
            currentUserGuide.transform.SetParent(boxColliders[currentBoxColliderIndex].transform);
        }
    }

    /// <summary>
    /// Ensure blue light guide is always facing player as it's a 2D plane
    /// </summary>
    void lookAtPlayer()
    {
        if (!currentUserGuide) return;

        Vector3 directionToPlayer = player.transform.position - currentUserGuide.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            lookRotation *= Quaternion.Euler(90, 0, 0);
            currentUserGuide.transform.rotation = lookRotation;
        }
    }

    // --- Snowman Challenge Logic ---

    void StartSnowmanChallenge()
    {
        CurrentSnowmanPart = 0;
        SpawnNextSnowmanPart();
    }

    // Logic for the final challenge
    void ProcessSnowmanChallenge()
    {
        if (currentBoxColliderIndex >= boxColliders.Length) return;

        if (CheckForEdgeHit())
        {
            HandleEdgeHit();

            if (currentBoxColliderIndex >= boxColliders.Length)
            {
                if (CurrentSnowmanPart < SnowmanParts.Length - 1)
                {
                    MoveCompletedPartToDesk(currentShape);
                }
                else
                {
                    MoveCompletedPartToDesk(currentShape);
                    StartCoroutine(PlayFinalVideo());
                }
            }
            else
            {
                spawnGuide();
            }
        }
    }

    void SpawnNextSnowmanPart()
    {
        // Loop videos again as they display which part to choose
        VideoPlayer.isLooping = true;
        VideoPlayer.clip = edgeClips[currentEdgeVideo];

        currentShape = Instantiate(SnowmanParts[CurrentSnowmanPart], new Vector3(0.6f, 1.9f, 2.1f), Quaternion.Euler(-90f, 0f, -180f));
        SetupShape(currentShape);
        spawnGuide();
    }

    // Unhide mesh once shape has been unwrapped
    // Progress to next part
    void BuildSnowman()
    {
        SnowmanPartsInGame[CurrentSnowmanPart].GetComponent<MeshRenderer>().enabled = true;

        completedSnowmanParts.Add(currentShape);

        CurrentSnowmanPart++;
        if (CurrentSnowmanPart < SnowmanParts.Length)
        {
            SpawnNextSnowmanPart();
        }
    }

    void MoveCompletedPartToDesk(GameObject part)
    {
        StartCoroutine(LerpToDeskPosition(part, new Vector3(1.4f, 0.8f, 2.27f)));
    }

    /// <summary>
    /// Move the completed challenge object to its assigned position on the table
    /// </summary>
    IEnumerator LerpToDeskPosition(GameObject part, Vector3 targetPosition)
    {
        Vector3 startPos = part.transform.position;
        float duration = 2.0f;
        float elapsed = 0;

        if (CurrentSnowmanPart < 2)
        {
            currentEdgeVideo += 1;
            Debug.Log(currentEdgeVideo);
            VideoPlayer.clip = edgeClips[currentEdgeVideo];
        }

        while (elapsed < duration)
        {
            part.transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        part.transform.position = targetPosition;
        
        Destroy(part);
        //unhide relevant snowman part
        BuildSnowman();

        if (CurrentSnowmanPart == SnowmanParts.Length)
        {
            StartCoroutine(PlayFinalVideo());
        }

        HideUserGuide = true;

    }

    // Return shape to original position if it becomes unnattainable
    public void TeleportShapeBack()
    {
        currentShape.transform.position = new Vector3(0.6f, 1.9f, 2.1f);
    }

    IEnumerator PlayFinalVideo()
    {
        if (completedShapes < unwrapClips.Length)
        {
            //stop looping to unwrap video plays once
            VideoPlayer.isLooping = false;
            VideoPlayer.clip = unwrapClips[completedShapes]; //unwrap video
            VideoPlayer.Play();

            //while video player loads up 
            while (!VideoPlayer.isPlaying)
            {
                yield return null;  // Wait for one frame until the video starts
            }

            // Wait for video to finish
            while (VideoPlayer.isPlaying)
            {
                yield return null;
            }

            SceneManager.LoadScene("MRTScene");
        }
        else
        {
            Debug.LogWarning("No matching video clip found for shape " + completedShapes);
        }
    }
}
