using UnityEngine;
using System.Collections;

public class TP_Camera : MonoBehaviour {

	public static TP_Camera Instance;
	public Transform TargetLookAt;
	public float Distance = 2f; //5
	public float DistanceMin = 0.5f; //3
	public float DistanceMax = 10f;	
	public float DistanceSmooth = 0.05f;
    public float DistanceResumeSmooth = 1;
	public float X_MouseSensitivity = 5f;
	public float Y_MouseSensitivity = 5f;
	public float MouseWheelSensitivity = 5f;
	public float X_Smooth = 0.05f;
	public float Y_Smooth = 0.1f;	
	public float Y_MinLimit = -40f;
	public float Y_MaxLimit = 80f;
    public float OcclusionDistanceStep = 0.5f;
    public int MaxOcclusionChecks = 10;
	
	public float DeathCameraHeight = 60f;
	public float DeathCameraSpinRate = 10f;
	public float DeathCameraDistance = 4f;

	
	private float mouseX = 0f;
	private float mouseY = 0f;
	private float velX = 0f;
	private float velY = 0f;
	private float velZ = 0f;
	private Vector3 position= Vector3.zero;
	private float velDistance = 0f;
	private float startDistance = 0f;
	private Vector3 desiredPosition = Vector3.zero;
	private float desiredDistance = 0f;
    private float distanceSmooth = 0f;
    private float preOccludedDistance=0;
	
	void Awake()
	{
		Instance = this;		
	}
	
	// Use this for initialization
	void Start () 
	{
		Distance = Mathf.Clamp (Distance,DistanceMin,DistanceMax);
		startDistance = Distance;
		Reset ();
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		if(TargetLookAt == null)
			return;
		
		if(!TP_Animator.Instance.IsDead)
			HandlePlayerInput();
		else
		{			
			desiredDistance = DeathCameraDistance;
			mouseX += Time.deltaTime *DeathCameraSpinRate;
			mouseY = DeathCameraHeight;
		}

        var count = 0;

        do
        {
            CalculateDesiredPosition();
            count++;
        }while(CheckIfOccluded(count));
        
        //ya no se necesita...
        //CheckCameraPoints(TargetLookAt.position, desiredPosition);
		
		UpdatePosition();
	}
	
	void HandlePlayerInput()
	{
		var deadZone = 0.01f;
		
		if(Input.GetMouseButton(1))
		{
			// Se presiono el clic der del mouse
			mouseX += Input.GetAxis ("Mouse X")*X_MouseSensitivity;
			mouseY -= Input.GetAxis ("Mouse Y")*Y_MouseSensitivity;
		}		
		
		// Aqui vamos a limitar la rotación de la camara en Y
		mouseY = Helper.ClampAngle(mouseY,Y_MinLimit, Y_MaxLimit);
		
		if(Input.GetAxis ("Mouse ScrollWheel") < -deadZone || Input.GetAxis ("Mouse ScrollWheel") > deadZone)
		{
			desiredDistance = Mathf.Clamp (Distance - Input.GetAxis ("Mouse ScrollWheel")*MouseWheelSensitivity,
				DistanceMin,DistanceMax);

            preOccludedDistance = desiredDistance;
            distanceSmooth = DistanceSmooth;
		}
	}
	
	void CalculateDesiredPosition()
	{
		//evaluar la distancia
        ResetDesiredDistance();
		Distance = Mathf.SmoothDamp(Distance,desiredDistance, ref velDistance, distanceSmooth);
		
		//calcular posicion deseada
		desiredPosition = CalculatePosition(mouseY,mouseX,Distance);
		
	}
	
	Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
	{
		Vector3 direction = new Vector3(0,0,-distance);
		Quaternion rotation = Quaternion.Euler (rotationX,rotationY,0);
		return TargetLookAt.position + rotation * direction;
	}

    bool CheckIfOccluded(int count)
    {
        var isOccluded = false;

        var nearestDistance = CheckCameraPoints(TargetLookAt.position, desiredPosition);

        if(nearestDistance != -1)
        {
            if (count < MaxOcclusionChecks)
            {
                isOccluded = true;
                Distance -= OcclusionDistanceStep;

                if (Distance < 0.25f)
                    Distance = 0.25f;
            }
            else
                Distance = nearestDistance - Camera.main.nearClipPlane;

            desiredDistance = Distance;
            distanceSmooth = DistanceResumeSmooth;
            
        }

        return isOccluded;
    }

    float CheckCameraPoints(Vector3 from, Vector3 to)
    {
        var nearestDistance = -1f;

        RaycastHit hitInfo;

        Helper.ClipPlanePoints clipPlanePoints = Helper.ClipPlaneAtNear(to);

        //Dibujar lineas en el editor para hacer mas facil la visualizacion

        Debug.DrawLine(from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, Color.red);
        Debug.DrawLine(from, clipPlanePoints.UpperLeft);
        Debug.DrawLine(from, clipPlanePoints.LowerLeft);
        Debug.DrawLine(from, clipPlanePoints.UpperRight);
        Debug.DrawLine(from, clipPlanePoints.LowerRight);

        Debug.DrawLine(clipPlanePoints.UpperLeft, clipPlanePoints.UpperRight);
        Debug.DrawLine(clipPlanePoints.UpperRight, clipPlanePoints.LowerRight);
        Debug.DrawLine(clipPlanePoints.LowerRight, clipPlanePoints.LowerLeft);
        Debug.DrawLine(clipPlanePoints.LowerLeft, clipPlanePoints.UpperLeft);

        if (Physics.Linecast(from, clipPlanePoints.UpperLeft, out hitInfo) && hitInfo.collider.tag != "Player")
            nearestDistance = hitInfo.distance;

        if (Physics.Linecast(from, clipPlanePoints.LowerLeft, out hitInfo) && hitInfo.collider.tag != "Player")
            if(hitInfo.distance < nearestDistance || nearestDistance == -1)
                nearestDistance = hitInfo.distance;

        if (Physics.Linecast(from, clipPlanePoints.UpperRight, out hitInfo) && hitInfo.collider.tag != "Player")
            if (hitInfo.distance < nearestDistance || nearestDistance == -1)
                nearestDistance = hitInfo.distance;

        if (Physics.Linecast(from, clipPlanePoints.LowerRight, out hitInfo) && hitInfo.collider.tag != "Player")
            if (hitInfo.distance < nearestDistance || nearestDistance == -1)
                nearestDistance = hitInfo.distance;

        if (Physics.Linecast(from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, out hitInfo) && hitInfo.collider.tag != "Player")
            if (hitInfo.distance < nearestDistance || nearestDistance == -1)
                nearestDistance = hitInfo.distance;

        return nearestDistance;
    }

    void ResetDesiredDistance()
    {
        if (desiredDistance < preOccludedDistance)
        {
            var pos = CalculatePosition(mouseY, mouseX, preOccludedDistance);

            var nearestDistance = CheckCameraPoints(TargetLookAt.position,pos);

            if (nearestDistance == -1 || nearestDistance > preOccludedDistance)
            {
                desiredDistance = preOccludedDistance;
            }
        }
    }

	void UpdatePosition()
	{		
		var posX = Mathf.SmoothDamp(position.x,desiredPosition.x,ref velX,X_Smooth);
		var posY = Mathf.SmoothDamp(position.y,desiredPosition.y,ref velY,Y_Smooth);
		var posZ = Mathf.SmoothDamp(position.z,desiredPosition.z,ref velZ,X_Smooth);
		
		position = new Vector3(posX,posY,posZ);
		transform.position = position;
		transform.LookAt(TargetLookAt);
	}
	
	public void Reset()
	{
		mouseX = 0;
		mouseY = 10;
		Distance = startDistance;
		desiredDistance = Distance;
        preOccludedDistance = Distance;
	}	
	
	public static void UseExistingOrCreateNewMainCamera()
	{
		GameObject tempCamera;
		GameObject targetLookAt;
		TP_Camera myCamera;
		
		if (Camera.main != null)
        {
            tempCamera = Camera.main.gameObject;
        }
        else
        {
            tempCamera = new GameObject("Main Camera");
            tempCamera.AddComponent<Camera>();
            tempCamera.tag = "MainCamera";
        }

        tempCamera.AddComponent<TP_Camera>();
        myCamera = tempCamera.GetComponent("TP_Camera") as TP_Camera;

        targetLookAt = GameObject.Find("targetLookAt") as GameObject;

        if (targetLookAt == null) 
        {
            targetLookAt = new GameObject("targetLookAt");
            targetLookAt.transform.position = Vector3.zero;
        }

        myCamera.TargetLookAt = targetLookAt.transform;
	}
}
