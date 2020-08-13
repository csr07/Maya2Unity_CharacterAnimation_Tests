using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    //private GameObject target = null;
    private Vector3 offset;
    public Transform parentToThisTransf;

    // Start is called before the first frame update
    void Start()
    {
        //target = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        //if (target != null)
        //{
        //    target.transform.position = transform.position + offset;
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PLATFORM OnTriggerStay");
        Debug.Log("collion with: " + other.name);
        //target = other.gameObject;
        //offset = target.transform.position - transform.position;

        other.transform.parent = parentToThisTransf;
        //other.transform.SetParent(transform, true);
    }

    void OnTriggerExit(Collider col)
    {
        Debug.Log("PLATFORM OnTriggerExit");
        col.transform.parent = null;
    }
}
