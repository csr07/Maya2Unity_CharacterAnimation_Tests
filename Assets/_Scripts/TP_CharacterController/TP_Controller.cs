using UnityEngine;
using System.Collections;

public class TP_Controller : MonoBehaviour {

	public static CharacterController CharacterController;
    public static TP_Controller Instance;	
	
	public bool ClimbEnabled {get;set;}
	
    // Awake se llama al crearse la clase
	void Awake()
    {
        CharacterController = GetComponent("CharacterController") as CharacterController;
        Instance = this;
		//nueva linea al crear la camara...
		 TP_Camera.UseExistingOrCreateNewMainCamera();
    }
	
	// Update se llama una vez por frame
	void Update () {
		if (Camera.main == null)
            return;
		
		TP_Motor.Instance.ResetMotor();
		
		if(!TP_Animator.Instance.IsDead &&
			TP_Animator.Instance.State != TP_Animator.CharacterState.Using &&
			(TP_Animator.Instance.State != TP_Animator.CharacterState.Landing ||
			TP_Animator.Instance.GetComponent<Animation>().IsPlaying("RunLand")) &&
			TP_Animator.Instance.State != TP_Animator.CharacterState.Climbing)
		{
			GetLocomotionInput();
			HandleActionInput();
		}
		else if(TP_Animator.Instance.IsDead)
		{
			if(Input.anyKeyDown)
				TP_Animator.Instance.Reset();
		}        

        TP_Motor.Instance.UpdateMotor();
	}
	
	void GetLocomotionInput()
    {		
        var deadZone = 0.1f;
        


        if (Input.GetAxis("Vertical") > deadZone || Input.GetAxis("Vertical") < -deadZone)
            TP_Motor.Instance.MoveVector += new Vector3(0, 0, Input.GetAxis("Vertical"));

        if (Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < -deadZone)
            TP_Motor.Instance.MoveVector += new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        TP_Animator.Instance.DetermineCurrentMoveDirection();
    }

    void HandleActionInput()
    {
        if (Input.GetButton("Jump"))
        {
			if(ClimbEnabled)
				Climb();
			else
            	Jump();
        }
		
		if (Input.GetKeyDown(KeyCode.E))
        {
            Use();
        }
		
		if (Input.GetKeyDown(KeyCode.F1))
        {
            Die();
        }
    }

    public void Jump()
    {
        TP_Motor.Instance.Jump();
		TP_Animator.Instance.Jump();
    }
	
	public void Use()
	{
		TP_Animator.Instance.Use();
	}
	
	public void Climb()
	{
		TP_Animator.Instance.Climb();
	}
	
	public void Die()
	{
		TP_Animator.Instance.Die ();		
	}
	
	//////////////////////////////////////////////////////////////////////////	
	
}
