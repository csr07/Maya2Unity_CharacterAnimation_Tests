using UnityEngine;
using System.Collections;

public class TP_Animator : MonoBehaviour
{
    public enum Direction
    { 
        Stationary, Forward, Backward, Left, Right,
        LeftForward, RightForward, LeftBackward, RightBackward
    }

    public enum CharacterState
    { 
        Idle, Running, WalkBackwards, StrafingLeft, StrafingRight, Jumping,
        Falling, Landing, Climbing, Sliding, Using, Dead, ActionLocked
    }

    public static TP_Animator Instance;
	
	private CharacterState lastState;
	private Transform climbPoint;
	
	public Vector3 ClimbOffset = Vector3.zero;
	public Vector3 PostClimbOffset = Vector3.zero;
	public float ClimbJumpStartTime = 0.4333f;
	public float ClimbAnchorTime = 0.6f;
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
	private Transform pelvis;	
	//transforms de los meshes poligonales
	private Transform armaduraa;	
	
	
	private Vector3 initialPosition = Vector3.zero;
	private Quaternion initialRotation = Quaternion.identity;
	private GameObject ragdoll;
	
	private float climbTargetOffset = 0f;
	private float ClimbInitialTargetHeight = 0f;
	
    public Direction MoveDirection { get; set; }
    public CharacterState State { get; set; }
	public bool IsDead{get;set;}


    void Awake()
    {
        Instance = this;
		pelvis = transform.Find ("grp_BONES01/bn_spA01") as Transform;
		initialPosition = transform.position;
		initialRotation = transform.rotation;	
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////		
		//inicializar los meshes
		//armaduraa = transform.FindChild("armaduraa0") as Transform;
		
	
    }

    void Update()
    {
        DetermineCurrentState();
        ProcessCurrentState();

        //Debug.Log("Current Character State: " + State.ToString());
    }

    public void DetermineCurrentMoveDirection()
    {
        var forward = false;
        var backward = false;
        var left = false;
        var right = false;

        if (TP_Motor.Instance.MoveVector.z > 0)
            forward = true;
        if (TP_Motor.Instance.MoveVector.z < 0)
            backward = true;
        if (TP_Motor.Instance.MoveVector.x > 0)
            right = true;
        if (TP_Motor.Instance.MoveVector.x < 0)
            left = true;

        if (forward)
        {
            if (left)
                MoveDirection = Direction.LeftForward;
            else if (right)
                MoveDirection = Direction.RightForward;
            else
                MoveDirection = Direction.Forward;            
        }
        else if (backward)
        {
            if (left)
                MoveDirection = Direction.LeftBackward;
            else if (right)
                MoveDirection = Direction.RightBackward;
            else
                MoveDirection = Direction.Backward;
        }
        else if (left)
            MoveDirection = Direction.Left;        
        else if (right)        
            MoveDirection = Direction.Right;       
        else        
            MoveDirection = Direction.Stationary;
    }

    void DetermineCurrentState()
    {
        if (State == CharacterState.Dead)
            return;

        if (!TP_Controller.CharacterController.isGrounded)
        {
            if (State != CharacterState.Falling &&
               State != CharacterState.Jumping &&
               State != CharacterState.Landing)
            { 
                // Deber�a estar cayendo.
				Fall ();
            }
        }

        if (State != CharacterState.Falling &&
            State != CharacterState.Jumping &&
            State != CharacterState.Landing &&
            State != CharacterState.Using &&
            State != CharacterState.Climbing &&
            State != CharacterState.Sliding)
        {
            switch (MoveDirection)
            { 
                case Direction.Stationary:
                    State = CharacterState.Idle;
                    break;
                case Direction.Forward:
                    State = CharacterState.Running;
                    break;
                case Direction.Backward:
                    State = CharacterState.WalkBackwards;
                    break;
                case Direction.Left:
                    State = CharacterState.StrafingLeft;
                    break;
                case Direction.Right:
                    State = CharacterState.StrafingRight;
                    break;
                case Direction.LeftForward:
                    State = CharacterState.Running;
                    break;
                case Direction.RightForward:
                    State = CharacterState.Running;
                    break;
                case Direction.LeftBackward:
                    State = CharacterState.WalkBackwards;
                    break;
                case Direction.RightBackward:
                    State = CharacterState.WalkBackwards;
                    break;                
            }
        }
    }

    void ProcessCurrentState()
    {
        switch (State)
        { 
            case CharacterState.Idle:
				Idle ();
                break;
            case CharacterState.Running:
				Running();
                break;
            case CharacterState.WalkBackwards:
				WalkBackwards();
                break;
            case CharacterState.StrafingLeft:
				StrafingLeft();
                break;
            case CharacterState.StrafingRight:
				StrafingRight();
                break;
            case CharacterState.Jumping:
				Jumping ();
                break;
            case CharacterState.Falling:
				Falling ();
                break;
            case CharacterState.Landing:
				Landing ();
                break;
            case CharacterState.Climbing:
				Climbing ();
                break;
            case CharacterState.Sliding:
				Sliding();
                break;
            case CharacterState.Using:
				Using();
                break;
            case CharacterState.Dead:
				Dead ();
                break;
            case CharacterState.ActionLocked:
                break;
        }
    }
	
	
	#region Character State Methods
	
	void Idle()
	{
		GetComponent<Animation>().CrossFade("Idle");		
	}
	
	void Running()
	{
		GetComponent<Animation>().CrossFade("Running");		
	}
	
	void WalkBackwards()
	{
		GetComponent<Animation>().CrossFade("WalkBackwards");		
	}
	
	void StrafingLeft()
	{
		GetComponent<Animation>().CrossFade("StrafingLeft");		
	}
	
	void StrafingRight()
	{
		GetComponent<Animation>().CrossFade("StrafingRight");		
	}
	
	void Using()
	{
		if(!GetComponent<Animation>().isPlaying)
		{
			State = CharacterState.Idle;
			GetComponent<Animation>().CrossFade("Idle");
		}
	}	
	
	void Jumping()
	{
		if((!GetComponent<Animation>().isPlaying && TP_Controller.CharacterController.isGrounded) ||
			TP_Controller.CharacterController.isGrounded)
		{
			if(lastState == CharacterState.Running)
				GetComponent<Animation>().CrossFade("RunLand");
			else			
				GetComponent<Animation>().CrossFade("JumpLand");
			State = CharacterState.Landing;
		}		
		else if(!GetComponent<Animation>().IsPlaying("Jump"))
		{
			State = CharacterState.Falling;
			GetComponent<Animation>().CrossFade("Falling");
			TP_Motor.Instance.IsFalling = true;
		}
		else
		{
			State = CharacterState.Jumping;
			//determinar si esta cayendo desde muy alto
		}
	}
	
	void Falling()
	{
		if(TP_Controller.CharacterController.isGrounded)
		{
			if(lastState == CharacterState.Running)
				GetComponent<Animation>().CrossFade("RunLand");
			else
				GetComponent<Animation>().CrossFade("JumpLand");
			State = CharacterState.Landing;
		}
	}
	
	void Landing()
	{
		if(lastState == CharacterState.Running)
		{
			if(!GetComponent<Animation>().IsPlaying("RunLand"))	
			{
				State = CharacterState.Running;
				GetComponent<Animation>().Play ("Running");
			}
		}
		else
		{
			if(!GetComponent<Animation>().IsPlaying("JumpLand"))	
			{
				State = CharacterState.Idle;
				GetComponent<Animation>().Play ("Idle");
			}
		}
		TP_Motor.Instance.IsFalling = false;
	}
	
	void Sliding()
	{
		if(!TP_Motor.Instance.IsSliding)
		{
			State = CharacterState.Idle;
			GetComponent<Animation>().CrossFade("Idle");
		}
	}
	
	void Climbing()
	{
		if(GetComponent<Animation>().isPlaying)
		{
			var time = GetComponent<Animation>()["Climb"].time;
			if(time > ClimbJumpStartTime && time < ClimbAnchorTime)
			{
				transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
					Mathf.Lerp(transform.rotation.eulerAngles.y,climbPoint.rotation.eulerAngles.y,
					(time - ClimbJumpStartTime)/(ClimbAnchorTime-ClimbJumpStartTime)),
					transform.rotation.eulerAngles.z);
				
				var climbOffset = transform.TransformDirection(ClimbOffset);
				
				transform.position = Vector3.Lerp (transform.position,
					new Vector3(climbPoint.position.x,transform.position.y,climbPoint.position.z) + climbOffset,
					(time - ClimbJumpStartTime)/(ClimbAnchorTime-ClimbJumpStartTime));
				
			}
			TP_Camera.Instance.TargetLookAt.localPosition = new Vector3(TP_Camera.Instance.TargetLookAt.localPosition.x,
																		pelvis.localPosition.y + climbTargetOffset,
																		TP_Camera.Instance.TargetLookAt.localPosition.z);
		}
		else
		{
			State = CharacterState.Idle;
			GetComponent<Animation>().Play ("Idle");
			var postClimbOffset = transform.TransformDirection(PostClimbOffset);
			transform.position = new Vector3(pelvis.position.x, 
											 climbPoint.position.y + climbPoint.localScale.y / 2,
											 pelvis.position.z) + postClimbOffset;
			
			TP_Camera.Instance.TargetLookAt.localPosition = new Vector3(TP_Camera.Instance.TargetLookAt.localPosition.x,
																		ClimbInitialTargetHeight,
																		TP_Camera.Instance.TargetLookAt.localPosition.z);
		}
	}
	
	void Dead()
	{		
		State = CharacterState.Dead;
	}
	
	#endregion	
	
	#region Start Action Method
	
	public void Use()
	{
		State = CharacterState.Using;
		GetComponent<Animation>().CrossFade("Using");
	}
	
	public void Jump()
	{
		if(!TP_Controller.CharacterController.isGrounded || IsDead || State == CharacterState.Jumping)
			return;
		
		lastState = State;
		State = CharacterState.Jumping;
		GetComponent<Animation>().CrossFade ("Jump");
	}
	
	public void Fall()
	{
		if(TP_Motor.Instance.VerticalVelocity >-5 || IsDead)
			return;
		lastState = State;
		State = CharacterState.Falling;
		
		TP_Motor.Instance.IsFalling = true;
		
		GetComponent<Animation>().CrossFade("Falling");
	}
	
	public void Slide()
	{
		State = CharacterState.Sliding;
		GetComponent<Animation>().CrossFade("Falling");
	}
	
	public void Climb()
	{
		if(!TP_Controller.CharacterController.isGrounded || IsDead || climbPoint==null)
			return;
		
		if(Mathf.Abs (climbPoint.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y) >60)
		{
			TP_Controller.Instance.Jump();
			return;
		}	
		
		State = CharacterState.Climbing;
		GetComponent<Animation>().CrossFade("Climbing");
		
		climbTargetOffset = TP_Camera.Instance.TargetLookAt.localPosition.y - pelvis.localPosition.y;
		ClimbInitialTargetHeight = 	TP_Camera.Instance.TargetLookAt.localPosition.y;
	}
	
	public void Die()
	{
		//inicializar todo 
		//IsDead = true;
		//SetupRagdoll();
		//Dead ();
		
	}
	
	public void Reset()
	{
		// para volver a jugar
		IsDead = false;
		transform.position = initialPosition;
		transform.rotation = initialRotation;
		State = CharacterState.Idle;
		GetComponent<Animation>().Play ("Idle");
		ClearRagdoll();
	}
	
	#endregion
	
	public void SetClimbPoint(Transform climbPoint)
	{
		this.climbPoint = climbPoint;
		TP_Controller.Instance.ClimbEnabled = true;
	}
	
	public void ClearClimbPoint(Transform climbPoint)
	{
		if(this.climbPoint == climbPoint)
		{
			this.climbPoint = null;
			TP_Controller.Instance.ClimbEnabled = false;
		}
	}
	
	void SetupRagdoll()
	{
		
		if(ragdoll == null)
		{
			// instanciar un nuevo ragdoll
			// coincidir con  la posicion y rotacion del personaje
			ragdoll = GameObject.Instantiate(Resources.Load("Ragdoll"),
											 transform.position,
											 transform.rotation)as GameObject;
		}
		
		// buscamos el nodo root del esqueleto, en este caso "root"
		
		var characterPelvis = transform.Find("grp_BONES01/bn_spA01");
		var ragdollPelvis = ragdoll.transform.Find("grp_BONES01/bn_spA01");
		
		// coincidir el esqueleto del ragdoll con el esqueleto del personaje
		MatchChildrenTransform(characterPelvis,ragdollPelvis);	
		
		//Esconder el personaje
		//pelvis.renderer.enabled = false;
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/*var comps = this.GetComponentsInChildren(Transform);

		foreach (var comp in comps) 
		{
   			comp.renderer.enabled = false;
		}*/	
		
		transform.GetComponent<Renderer>().enabled = false;
		
		
		
		
		//Decirle a la camara que mire al ragdoll
		TP_Camera.Instance.TargetLookAt = ragdoll.transform.Find ("root");
	}
	
	void ClearRagdoll()
	{
		//Destruir ragdoll
		if(ragdoll != null)
		{
			GameObject.Destroy (ragdoll);
			ragdoll = null;
		}
			
		//Mostrar al personaje de nuevo		
		//pelvis.renderer.enabled = true;
		
		/*armaduraa.renderer.enabled = true;
		armadurab.renderer.enabled = true;
		armadurac.renderer.enabled = true;
		cuerpo.renderer.enabled = true;
		espind.renderer.enabled = true;
		espini.renderer.enabled = true;
		hombrod.renderer.enabled = true;
		hombroi.renderer.enabled = true;
		ojod.renderer.enabled = true;
		ojoi.renderer.enabled = true;		
		transform.renderer.enabled = true;*/
		
		// decirle a la camara que que mire al TargetLookAt del personaje.
		TP_Camera.Instance.TargetLookAt = transform.Find("targetLookAt");
		
	}
	
	void MatchChildrenTransform(Transform source, Transform target)
	{
		//moverse atravez de la jerarquia para coincidir las rotaciones de los joints
		
		if(source.childCount > 0)
		{
			foreach(Transform sourceTransform in source.transform)
			{
				Transform targetTransform = target.Find (sourceTransform.name);
				
				if(targetTransform!=null)
				{
					MatchChildrenTransform(sourceTransform,targetTransform);
					targetTransform.localPosition = sourceTransform.localPosition;
					targetTransform.localRotation = sourceTransform.localRotation;
				}
			}
		}		
	}
}