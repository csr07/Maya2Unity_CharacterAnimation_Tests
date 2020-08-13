using UnityEngine;
using System.Collections;

public class ClimbingVolume : MonoBehaviour {

	// Use this for initialization
	void OnTriggerEnter()
	{
		TP_Animator.Instance.SetClimbPoint(transform);
	}
	
	void OnTriggerExit()
	{
		TP_Animator.Instance.ClearClimbPoint(transform);
	}
}
