using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGameCB : MonoBehaviour
{
    public GameObject crateScoreTextGO;
    public GameObject exitDoorGO;
    private int crateCounter;
    private int totalCrateNum;

    private bool isExitOpen;
    // Start is called before the first frame update
    void Start()
    {
        totalCrateNum = 5;
        crateCounter = 0;
        SetScore(crateCounter);

        isExitOpen = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (crateCounter == totalCrateNum && !isExitOpen)
        {
            isExitOpen = true;
            //exitDoorGO.SetActive(true);
            exitDoorGO.GetComponent<Animation>().Play("DoorOpen");
        }
    }

    public void AddToScore()
    {
        crateCounter++;
        SetScore(crateCounter);
    }

    public void SetScore(int val)
    {
        crateScoreTextGO.GetComponent<Text>().text = "Crates Collected: " + val + "/" + totalCrateNum;
    }
}
