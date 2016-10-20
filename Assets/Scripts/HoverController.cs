using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class HoverController : MonoBehaviour {

    public float hoverHeight = 3f;
    public float impulseAmount = 10f;
    public bool triggerImpulse;

    public PID hoverPID;
    public PID angularPID;

    Rigidbody rb;

    public float currentHeight;
    public float currentAngle;
    public float hoverForce;
    public float angularForce;
    public Vector3 currentAngularVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)){
            triggerImpulse = false;
            rb.AddForce(-transform.up * impulseAmount, ForceMode.Impulse);
            rb.AddRelativeTorque(new Vector3(1f, 0f, 0.5f), ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        RaycastHit[] hit = Physics.RaycastAll(transform.position, -transform.up);

        for(int i = 0; i < hit.Length; i++){
            if(hit[i].collider.CompareTag("Terrain")){
                currentHeight = hit[i].distance;
            }
        }

        hoverForce = hoverPID.Update(hoverHeight, currentHeight, Time.fixedDeltaTime);
        rb.AddForce(Vector3.up * hoverForce);

        Quaternion targetRotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f);
        Quaternion q = targetRotation * Quaternion.Inverse(rb.rotation);
        currentAngle = Quaternion.Angle(rb.rotation, targetRotation);
        angularForce = angularPID.Update(0f, currentAngle, Time.fixedDeltaTime);
        Vector3 axis = new Vector3(q.x * angularForce, q.y * angularForce, q.z * angularForce);
        //axis *= f;
        rb.AddRelativeTorque(axis);
        currentAngularVelocity = axis;
        //transform.eulerAngles = new Vector3(Mathf.Clamp(transform.eulerAngles.x, -30f, 30f), transform.eulerAngles.y, Mathf.Clamp(transform.eulerAngles.z, -30f, 30f));


        /*Vector3 targetForward = targetRotation * Vector3.forward;
        Vector3 axis = Vector3.Cross(transform.forward, targetForward).normalized;
        currentAngle = Quaternion.Angle(rb.rotation, targetRotation);
        Vector3 targetAxis = axis * currentAngle;

        angularForce = angularPID.Update(axis.magnitude, rb.angularVelocity.magnitude, Time.fixedDeltaTime);
        rb.AddTorque(axis.normalized);
        currentAngularVelocity = rb.angularVelocity;*/

    }
}
