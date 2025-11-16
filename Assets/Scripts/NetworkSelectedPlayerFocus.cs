using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkSelectedPlayerFocus : MonoBehaviour
{
    public GameObject targetTag;
    private int currentIndex = 0;
    public List<GameObject> objects = new List<GameObject>();

    public float refreshInterval = 5f; // refresh every 10 seconds

    void Start()
    {
        RefreshList();
        SelectedObject();
        StartCoroutine(RefreshRoutine());
    }

    IEnumerator RefreshRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            RefreshList();
            SelectedObject();
        }
    }

    void RefreshList()
    {
        objects.Clear();
        objects.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        objects.AddRange(GameObject.FindGameObjectsWithTag("EnemyPlayer"));
        objects.AddRange(GameObject.FindGameObjectsWithTag("DefaultView"));
    }

    public void SelectedObject()
    {
        if (objects.Count == 0)
        {
            Debug.LogWarning("No objects with tag found.");
            targetTag = null;
            return;
        }

        // Select object in order
        targetTag = objects[currentIndex];
        var cameraFollow = GetComponent<TopDownCameraFollow>();
        if (cameraFollow != null)
            cameraFollow.target = targetTag.transform;

        Debug.Log("Server selected: " + targetTag.name);

        // Increment index and loop back to 0 if needed
        currentIndex++;
        if (currentIndex >= objects.Count)
            currentIndex = 0;
    }
}
