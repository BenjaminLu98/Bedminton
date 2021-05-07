using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class Area : MonoBehaviour
{
    public GameObject ball;
    public BedmintonAgent t1_AgentA;
    public BedmintonAgent t1_AgentB;
    public BedmintonAgent t2_AgentA;
    public BedmintonAgent t2_AgentB;
    public bool isEnding;

    Rigidbody m_BallRb;
    public float stepPassed;
    // Use this for initialization
    void Start()
    {
        m_BallRb = ball.GetComponent<Rigidbody>();
        MatchReset();
        isEnding = false;
        stepPassed = 0;
    }

  
    public void MatchReset()
    {
        stepPassed = 0;
        var ballOut = 8f;
        var noise = Random.value-0.5f;
        var flip = Random.Range(0, 2);
        if (flip == 0)
        {
            ball.transform.position = new Vector3(-ballOut + noise * 5, -1.5f, 1.079f+noise*2) + transform.position;
        }
        else
        {
            ball.transform.position = new Vector3(ballOut + noise * 5, -1.5f, -1.079f+noise*2) + transform.position;
        }
        Debug.Log("reset ball velocity");
        m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.localScale = new Vector3(.5f, .5f, .5f);
        ball.GetComponent<HitWall>().lastAgentHit = -1;


    }

    void FixedUpdate()
    {
        var rgV = m_BallRb.velocity;
        m_BallRb.velocity = new Vector3(Mathf.Clamp(rgV.x, -9f, 9f), Mathf.Clamp(rgV.y, -9f, 9f), Mathf.Clamp(rgV.z, -9f, 9f));
        isEnding = false;
        stepPassed += 1;
    }
}