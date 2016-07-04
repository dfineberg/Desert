using UnityEngine;
using System.Collections;
using ThirdPersonCharacter = UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter;

public class FollowCamera : MonoBehaviour {

	[Header("General stuff")]
	public float targetDistance;
	public float zoomSpeed;
	public float offsetH;
	public float offsetV;
	public float followSpeed;
	public float resetTime;
	public string axisH, axisV;
	public Transform target;


	[Header("Yaw controls")]
	public float yawSpeed;
	public LayerMask obstaclesLayerMask;


	[Header("Pitch controls")]
	public float pitchSpeed;
	public float minAngle, maxAngle;
	public float pitchDamping;
	public float minGroundHeight;
	public float regularFOV, maxFOV;
	public LayerMask terrainMask;


	[Header("Character controller")]
	public string playerAxisH;
	public string playerAxisV;
	public ThirdPersonCharacter characterController;



	Camera camera;
	Vector3 targetPosition;

	float angleY = 0f;
	float angleX = 0f;
	float distance;
	float lastInputTime = 0f;


	bool shouldReset{
		get{
			return (Time.time - lastInputTime > resetTime);
		}
	}

	Vector3 angleDirection{
		get{
			return Quaternion.Euler (angleX, angleY, 0f) * Vector3.back * distance;
		}
	}


	void Start()
	{
		if (target == null)
			return;

		distance = targetDistance;
		targetPosition = target.position - (target.forward * distance);
		transform.position = targetPosition;

		camera = GetComponent<Camera> ();
	}

	void Update()
	{
		Vector3 walkDirection = new Vector3 (Input.GetAxis (playerAxisH), 0f, Input.GetAxis (playerAxisV));
		walkDirection = Quaternion.Euler (angleX, angleY, 0f) * walkDirection;

		if(characterController){
			characterController.Move (walkDirection, false, false);
		}
	}

	void LateUpdate()
	{
		UpdatePosition ();
	}

	void UpdatePosition()
	{
		if (target == null)
			return;

		float yCache = angleY;
		float xCache = angleX;

		float inputH = Input.GetAxis (axisH) * yawSpeed * Time.deltaTime;
		float inputV = Input.GetAxis (axisV) * pitchSpeed * Time.deltaTime;
		float newTargetDistance = targetDistance;


		if(inputH != 0f || inputV != 0f){
			lastInputTime = Time.time;
		}


#region yaw_controls

		float maxWhiskerAngle = 60f;
		int numWhiskers = 10;
		float whiskerAngle = maxWhiskerAngle / numWhiskers;
		float thisAngle = whiskerAngle;
		bool hitObstacle = false;

		angleY += inputH;

		while(thisAngle <= maxWhiskerAngle){
			RaycastHit[] hit = new RaycastHit[2];

			for(int i = 0; i < 2; i++){ // one set to the left of the camera and one set to the right
				float thisAngleY = angleY + (i == 0 ? thisAngle : -thisAngle);
				Vector3 thisDirection = Quaternion.Euler (angleX, thisAngleY, 0f) * Vector3.back; // calculate a rotation a few degrees round from the camera's relative direction
				Debug.DrawRay (target.position, thisDirection * targetDistance, Color.red);

				if(Physics.Raycast(target.position, thisDirection, out hit[i], targetDistance, obstaclesLayerMask)){ // if the whisker hits something...
					hitObstacle = true;
					float compensateProp = thisAngle / maxWhiskerAngle; // how far out from the centre to the peripheral whiskers are we
					float thisTargetDistance = Mathf.Lerp (hit[i].distance - 0.2f, targetDistance, compensateProp);

					if (thisTargetDistance < newTargetDistance){ // if the left and right whisker both hit something, go with the obstacle that's closer
						newTargetDistance = thisTargetDistance;
					}
				}
			}

			if (hitObstacle) // stop casting whiskers if we've found an obstacle
				break;

			thisAngle += whiskerAngle;
		}

#endregion

#region pitch_controls

		if(inputV != 0f){
			float pitchAdjust = 0f;
			float angleDiff = pitchDamping;

			if(angleX < minAngle + pitchDamping){ // if camera is approaching minimum pitch
				angleDiff = Mathf.Abs(minAngle - angleX);
			}else if(angleX > maxAngle - pitchDamping){ // if camera is approaching maximum pitch
				angleDiff = Mathf.Abs(maxAngle - angleX);
			}

			float prop = angleDiff / pitchDamping; // no damping when this = 1f
			pitchAdjust = Mathf.Lerp(inputV > 0f ? 0.2f : -0.2f, inputV, prop); // cut the input value according to our damping amount (none when prop is 1f)
			angleX += pitchAdjust;
		}

		if (angleX < -360){
			angleX += 360;
		} else if (angleX > 360){
			angleX -= 360;
		}

		angleX = Mathf.Clamp (angleX, minAngle, maxAngle);

		RaycastHit finalHit;
		Ray finalRay = new Ray(target.position, new Vector3(transform.position.x, transform.position.y - minGroundHeight, transform.position.z) - target.position);
		Debug.DrawRay(finalRay.origin, finalRay.direction.normalized * newTargetDistance, Color.blue);

		if(Physics.Raycast(finalRay, out finalHit, newTargetDistance, terrainMask)){ // one final raycast to check for terrain occluding the player
			newTargetDistance = finalHit.distance - 0.2f;
		}

		if(angleX >= 0f){
			camera.fieldOfView = regularFOV;
		}else{
			camera.fieldOfView = Mathf.Lerp(regularFOV, maxFOV, angleX / minAngle); // widen the camera's FOV if the camera is pointing upwards
		}

#endregion

		distance = Mathf.Lerp (distance, newTargetDistance, zoomSpeed * Time.deltaTime);
		Vector3 direction = Vector3.back * distance;
		Vector3 targetPos = target.position + (Quaternion.Euler(angleX, angleY, 0f) * direction); // target position assuming no obstacles between camera and target


		transform.position = targetPos;
		transform.LookAt (target.position);
	}
}
