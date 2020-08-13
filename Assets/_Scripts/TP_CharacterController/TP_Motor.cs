using UnityEngine;
using System.Collections;

public class TP_Motor : MonoBehaviour {

	public static TP_Motor Instance;

	public float ForwardSpeed = 5f; //10f
    public float BackwardSpeed = 2f;
    public float StrafingSpeed = 5f;
    public float SlideSpeed = 10f;
    public float JumpSpeed = 10f; //6f
    public float Gravity = 21f;
    public float TerminalVelocity = 20f;
    public float SlideThreshold = 0.6f;
    public float MaxControllableSlideMagnitude = 0.4f;
	public float FatalFallHeight = 7f;

    private Vector3 slideDirection;
	private float startFallHeight;
	private bool isFalling = false;
	
	public bool IsFalling
	{
		get{return isFalling;}
		set
		{
			isFalling = value;
			if(isFalling)
			{
				startFallHeight = transform.position.y;	
			}
			else
			{
				if(startFallHeight -transform.position.y > FatalFallHeight)
					TP_Controller.Instance.Die();
			}
		}
	}

	public Vector3 MoveVector{get;set;}
    public float VerticalVelocity { get; set; }
	public bool IsSliding {get;set;}

	
	void Awake()
	{
		Instance = this;       
	}	
	
	public void UpdateMotor()
    {
        SnapAlignCharacterWithCamera();
        ProcessMotion();
    }
	
	public void ResetMotor()
	{  		
		VerticalVelocity = MoveVector.y;
		MoveVector = Vector3.zero;
	}
	
	void ProcessMotion()
    { 
		//Transformar MoveVector a World Space
		if(!TP_Animator.Instance.IsDead)
			MoveVector = transform.TransformDirection(MoveVector);
		else
		{
			MoveVector = new Vector3(0,MoveVector.y,0);	
		}
        //Normalizar MoveVector
        if(MoveVector.magnitude >1)
            MoveVector = Vector3.Normalize(MoveVector);

        //Multiplicar MoveVector por MoveSpeed
        MoveVector *= MoveSpeed();

        // Aplicar Slide si es aplicable
        ApplySlide();
        
        //Reaplicar VerticalVelocity a MoveVector.y
        MoveVector = new Vector3(MoveVector.x,VerticalVelocity,MoveVector.z);

        //Aplicar gravedad
        ApplyGravity();

        //Mover al Character en World Space
        TP_Controller.CharacterController.Move(MoveVector * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (MoveVector.y > -TerminalVelocity)
        {
            MoveVector = new Vector3(MoveVector.x,MoveVector.y-Gravity * Time.deltaTime,MoveVector.z);
        }

        if(TP_Controller.CharacterController.isGrounded && MoveVector.y < -1)
            MoveVector = new Vector3(MoveVector.x, -1, MoveVector.z);
    }

    void ApplySlide()
    {
        if (!TP_Controller.CharacterController.isGrounded)
            return;
        slideDirection = Vector3.zero;

        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitInfo))
        {
            if (hitInfo.normal.y < SlideThreshold)
            {
                slideDirection = new Vector3(hitInfo.normal.x, -hitInfo.normal.y, hitInfo.normal.z);
				if(!IsSliding)
				{
					TP_Animator.Instance.Slide();
				}
				IsSliding = true;
            }
			else
			{
				IsSliding = false;	
			}
        }

        if (slideDirection.magnitude < MaxControllableSlideMagnitude)
            MoveVector += slideDirection;
        else
        {
            MoveVector = slideDirection;
        }
    }

    public void Jump()
    {
        if (TP_Controller.CharacterController.isGrounded)
            VerticalVelocity = JumpSpeed;
    }
	
	void SnapAlignCharacterWithCamera()
    {
        if (MoveVector.x != 0 || MoveVector.z != 0)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
                                Camera.main.transform.eulerAngles.y,
                                transform.eulerAngles.z);
        }
    }

    float MoveSpeed()
    {
        float moveSpeed = 0;

        switch(TP_Animator.Instance.MoveDirection)
        {
            case TP_Animator.Direction.Stationary:
                moveSpeed = 0;
                break;
            case TP_Animator.Direction.Forward:
                moveSpeed = ForwardSpeed;
                break;
            case TP_Animator.Direction.Backward:
                moveSpeed = BackwardSpeed;
                break;
            case TP_Animator.Direction.Left:
                moveSpeed = StrafingSpeed;
                break;
            case TP_Animator.Direction.Right:
                moveSpeed = StrafingSpeed;
                break;
            case TP_Animator.Direction.LeftForward:
                moveSpeed = ForwardSpeed;
                break;
            case TP_Animator.Direction.RightForward:
                moveSpeed = ForwardSpeed;
                break;
            case TP_Animator.Direction.LeftBackward:
                moveSpeed = BackwardSpeed;
                break;
            case TP_Animator.Direction.RightBackward:
                moveSpeed = BackwardSpeed;
                break;
        }

        if (IsSliding)
            moveSpeed = SlideSpeed;

        return moveSpeed;
    }

}
