using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventOut : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {

        if (other.tag == "Player")
        {

            var rb = other.gameObject.GetComponent<Rigidbody>();
            rb.AddForce(-2 * rb.velocity, ForceMode.VelocityChange);
        }
    }

}
