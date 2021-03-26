using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBall : MonoBehaviour
{
    public Area area;
    public float Force;
    public float HitReward;
    //public void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.collider.CompareTag("ball")&& !area.isEnding)
    //    {
    //        var xUnit = new Vector3(-1f, 0, 0);
    //        var BatDir = transform.localRotation * xUnit;
    //        var zUnit = new Vector3(0, 0, -1f);
    //        var normal = Vector3.Cross(zUnit,BatDir);
    //        var WorldNormal = transform.TransformVector(normal*100);
    //        var BallRigidbody =collision.collider.attachedRigidbody;
    //        BallRigidbody.AddForce(WorldNormal * Force, ForceMode.Impulse);


    //        var agent=GetComponentInParent<BedmintonAgent>();
    //        agent.AddReward(HitReward);
    //    }
    //}
}
