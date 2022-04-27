using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BedmintonAgent : Agent
{
    public GameObject ball;
    public bool invertX;
    public int score;
    public GameObject myArea;
    public float angle;
    public float scale;
    public int team;
    public GameObject Bat;
    public int id;
    public float moveSpeedX;
    public float moveSpeedZ;
    public float BatRotateSpeed;
    public float BodyRotateSpeed;
    public float m_endurance;
    public float AgentResetLocalY;
    public float testForce;
    public float BatSpringForce;
    public bool hitting;

    Text m_TextComponent;
    Rigidbody m_AgentRb;
    Rigidbody m_BallRb;
    float m_InvertMult;
    EnvironmentParameters m_ResetParams;
    
    bool jumping;
    Area Area;
    float distToMate;
    
    // Looks for the scoreboard based on the name of the gameObjects.
    // Do not modify the names of the Score GameObjects
    const string k_CanvasName = "Canvas";
    const string k_ScoreBoardAName = "ScoreA";
    const string k_ScoreBoardBName = "ScoreB";

    public override void Initialize()
    {
        m_InvertMult = invertX ? -1f : 1f;

        m_AgentRb = GetComponent<Rigidbody>();

        m_BallRb = ball.GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
       

        Area=myArea.GetComponent<Area>();

        SetResetParameters();

    }

    float Normalize(float cur,float min,float max)
    {
        float value = (cur - min) / (max - min);
        return value;
    }

    //向sensor提供观测值，传递给算法
    public override void CollectObservations(VectorSensor sensor)
    {
        //球相对人的参数，前向、右向为正
        sensor.AddObservation(m_InvertMult*(ball.transform.position.x - myArea.transform.position.x));
        sensor.AddObservation(ball.transform.position.y - myArea.transform.position.y);
        sensor.AddObservation(m_InvertMult * (ball.transform.position.z - myArea.transform.position.z));
        sensor.AddObservation(-m_InvertMult*m_BallRb.velocity.x);
        sensor.AddObservation( m_BallRb.velocity.y);
        sensor.AddObservation(m_InvertMult * m_BallRb.velocity.z);

        //拍子旋转角
        sensor.AddObservation(Bat.transform.localRotation.x);

        //角色的参数
        sensor.AddObservation(m_InvertMult * (transform.position.x - myArea.transform.position.x));
        sensor.AddObservation(transform.position.y - myArea.transform.position.y);
        sensor.AddObservation(m_InvertMult * (transform.position.z-myArea.transform.position.z));
        sensor.AddObservation(-m_InvertMult * m_AgentRb.velocity.x);
        sensor.AddObservation(m_AgentRb.velocity.y);
        sensor.AddObservation(m_InvertMult * m_AgentRb.velocity.z);

        //旋转角
        var m_localEularY = transform.localEulerAngles.y;
        if (invertX)
        {
            sensor.AddObservation(m_localEularY);
        }
        else
        {
            if(m_localEularY > 180f)
            {
                sensor.AddObservation(m_localEularY-180f);
            }
            else
            {
                sensor.AddObservation(m_localEularY+180f);
            }
        }

        //Agent[] agents=new Agent[2]{Area.t1_AgentA,Area.t2_AgentA};
        //int m_teamNo=invertX?0:1;

        //for(int i=0;i<2;i++)
        //{
        //    var agent=agents[i];
        //    if(agent!=this)
        //    {

        //        Rigidbody a_RB = agent.GetComponent<Rigidbody>();
        //        sensor.AddObservation(m_InvertMult * (agent.transform.position.x - myArea.transform.position.x));
        //        sensor.AddObservation(agent.transform.position.y - myArea.transform.position.y);
        //        sensor.AddObservation(m_InvertMult * (agent.transform.position.z - myArea.transform.position.z));
        //        sensor.AddObservation(-m_InvertMult * a_RB.velocity.x);
        //        sensor.AddObservation(a_RB.velocity.y);
        //        sensor.AddObservation(m_InvertMult * a_RB.velocity.z);

        //        var a_localEularY= a_RB.transform.localEulerAngles.y;
        //        if (!invertX)
        //        {
        //            sensor.AddObservation(a_localEularY);
        //        }
        //        else
        //        {
        //            if (a_localEularY > 180f)
        //            {
        //                sensor.AddObservation(a_localEularY - 180f);
        //            }
        //            else
        //            {
        //                sensor.AddObservation(a_localEularY + 180f);
        //            }
        //        }

        //    }

        //}

        float BatDisToBall = Mathf.Abs(Vector3.Distance(ball.transform.position, Bat.transform.position)) ;
        if(BatDisToBall<3.5f)
        {
            var reward = 0.04f * (1 - Normalize(BatDisToBall, 0f, 3.5f));
            AddReward(reward);
            Debug.Log(this.name + "BatDisToBallReward:" + reward);
        }

    }

    //处理从策略获得的动作，将其应用到游戏角色中
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        var discreteActions = actionBuffers.DiscreteActions;
        var Axis = discreteActions[3];
        var dirToGo = Vector3.zero;
        var RotToGo = Vector3.zero;
        //移动方向选择
        switch (Axis)
        {
            case 1:
                dirToGo = transform.forward * moveSpeedX;
                m_endurance -= 0.1f;

                break;
            case 2:
                dirToGo = -transform.forward * moveSpeedX;
                m_endurance -= 0.1f;

                break;
            case 3:
                dirToGo = transform.right * moveSpeedZ;
                m_endurance -= 0.1f;

                break;
            case 4:
                dirToGo = -transform.right * moveSpeedZ;
                m_endurance -= 0.1f;

                break;
        }
        float agentDisToBall = Mathf.Abs(Vector3.Distance(transform.position, ball.transform.position));

        //跳跃
        // 0-1
        var Jump = discreteActions[0];
        m_endurance -= 0.3f * Jump;

        //挥拍
        //0-1
        var RotateBat = discreteActions[1];
        m_endurance -= 0.1f * Mathf.Abs(RotateBat);

        //转身
        //0-2
        var RotateBody = discreteActions[2];
        m_endurance -= 0.1f * Mathf.Abs(RotateBody);

        //挥拍动作选择
        switch (RotateBat)
        {
            case 1:
                RotateBat = 1;
                break;
            case 0:
                RotateBat = -1;
                break;
        }

        //转身动作选择
        switch (RotateBody)
        {
            case 1:
                RotToGo = transform.up;
                break;
            case 2:
                RotToGo = -transform.up;
                break;
        }

        var BatJoint = Bat.GetComponent<HingeJoint>();
        var spring = BatJoint.spring;

        //将挥拍动作应用到角色。
        if (!hitting && RotateBat == 1)
        {
            hitting = true;
            spring.targetPosition = 120f;
            spring.spring = BatSpringForce;
            spring.damper = 50f;
            BatJoint.spring = spring;
        }
        else if (RotateBat == -1)
        {
            hitting = false;
            spring.targetPosition = 0f;
            spring.spring = BatSpringForce/2;
            spring.damper = 100f;
            BatJoint.spring = spring;
        }

        //旋转实现
        transform.Rotate(RotToGo, Time.deltaTime * 100f);

        //跳跃实现
        if ( transform.parent.position.y - transform.position.y > -AgentResetLocalY-0.01f )
        {
            jumping = false;
        }

        if (!jumping && Jump==1)
        {
            jumping = true;
            //如果改了这个jump力，那么需要改观测值上下限！
            m_AgentRb.AddForce(new Vector3(0f,4f,0f), ForceMode.VelocityChange);
        }
        m_AgentRb.AddForce(new Vector3(dirToGo.x, 0f , dirToGo.z), ForceMode.VelocityChange);

        //防止越界
        if (invertX && transform.position.x - transform.parent.transform.position.x > m_InvertMult ||
            !invertX && transform.position.x - transform.parent.transform.position.x < m_InvertMult)
        {
            transform.position = new Vector3(m_InvertMult + transform.parent.transform.position.x,
                transform.position.y,
                transform.position.z);
        }
    }

    //跳跃时向跳跃添加ActionMask
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if(jumping)
        {
            actionMask.WriteMask(0, new int[1] { 1 });
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        //continuousActionsOut[0] = Input.GetAxis("Vertical");    // Racket Movement
        //continuousActionsOut[1] = Input.GetKey(KeyCode.Space) ? 1f : 0f;   // Racket Jumping
        //continuousActionsOut[1] = Input.GetAxis("Horizontal");   // Racket Movement
        //continuousActionsOut[2] = Input.GetMouseButton(0)? 1f : -1f;
        //continuousActionsOut[2] = Input.GetAxis("RotateBody");

        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // agent Jumping
        discreteActionsOut[1] = Input.GetMouseButton(0) ? 1 : 0;  // agent Hiting
        if(Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[2] = 1;
        }
        else if(Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[2] = 2;
        }
        else
        {
            discreteActionsOut[2] = 0;
        }

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[3] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[3] = 2;
        }
        else if(Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[3] = 3;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[3] = 4;
        }
        else
        {
            discreteActionsOut[3] = 0;
        }
    }

    public override void OnEpisodeBegin()
    {
        //BatDegree = 0;
        //BodyDegree = 0;
        
        m_endurance =100f;
        // transform.position = new Vector3(-m_InvertMult * Random.Range(6f, 8f), -1.5f, -1.8f) + transform.parent.transform.position;
        if (id==2)
        {
            transform.position = new Vector3(m_InvertMult * 3f, AgentResetLocalY, 0f) + transform.parent.transform.position;
        }
        if(id==1)
        {
            transform.position = new Vector3(m_InvertMult * 7f, AgentResetLocalY, 0f) + transform.parent.transform.position;
        }

        m_AgentRb.velocity = new Vector3(0f, 0f, 0f);

        SetResetParameters();
    }

    public void SetAgent()
    {
        // angle = m_ResetParams.GetWithDefault("angle", 0);
        // gameObject.transform.eulerAngles = new Vector3(
        //     gameObject.transform.eulerAngles.x,
        //     gameObject.transform.eulerAngles.y,
        //     m_InvertMult * angle
        // );
        m_AgentRb.transform.localEulerAngles = new Vector3(
            m_AgentRb.transform.localEulerAngles.x,
            -m_InvertMult*90,
            m_AgentRb.transform.eulerAngles.z
        );
    }

    public void SetBall()
    {
        scale = m_ResetParams.GetWithDefault("scale", .5f);
        ball.transform.localScale = new Vector3(scale, scale, scale);

    }

    public void SetResetParameters()
    {
        SetAgent();
        SetBall();
        
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log(collision.collider.name);
    //}
}
