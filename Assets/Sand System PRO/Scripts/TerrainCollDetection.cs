using UnityEngine;
using System.Collections;

[AddComponentMenu("Mesh/SSTerrain/Player")]
public class TerrainCollDetection : MonoBehaviour
{
    public bool haveCollision = false;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "SSTerrain")
            haveCollision = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "SSTerrain")
            haveCollision = false;
    }
}