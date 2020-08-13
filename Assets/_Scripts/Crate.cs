using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    private MainGameCB mainGameCB;

    // Start is called before the first frame update
    void Start()
    {
        GameObject main = GameObject.Find("MainGame");
        if (main != null)
        {
            mainGameCB = main.GetComponent<MainGameCB>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Name: " + other.name);


        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Crate touched by Player!");
            mainGameCB.AddToScore();
            Destroy(this.gameObject);
        }
    }
}
