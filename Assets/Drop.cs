using UnityEngine;
using System.Collections;

public class Drop : MonoBehaviour {

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.position = new Vector3(Random.Range(26.5f, 28.5f), 6f, Random.Range(23f, 26.5f));
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
