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



    Text m_TextComponent;
    Rigidbody m_AgentRb;
    Rigidbody m_BallRb;
    float m_InvertMult;
    EnvironmentParameters m_ResetParams;
    
    Area Area;
    float BatDegree;
    float BodyDegree;

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


    //需要：添加队友、对手信息、添加z轴信息
    public override void CollectObservations(VectorSensor sensor)
    {
        //agent离球网的距离
        sensor.AddObservation(m_InvertMult * (transform.position.x - myArea.transform.position.x));
        //球场原点至agent的距离（高度)e
        sensor.AddObservation(transform.position.y - myArea.transform.position.y);
        
        sensor.AddObservation(m_InvertMult * transform.position.z);
        //agent当前x轴方向速度
        sensor.AddObservation(m_InvertMult * m_AgentRb.velocity.x);
        //agent当前y轴方向速度（纵向）
        sensor.AddObservation(m_AgentRb.velocity.y);
         
        sensor.AddObservation(m_InvertMult * m_AgentRb.velocity.z);
        
        
        Agent[,] agents=new Agent[2,2]{{Area.t1_AgentA,Area.t1_AgentB},{Area.t2_AgentA,Area.t2_AgentB}};
        int m_teamNo=invertX?0:1;
        int op_teamNo=invertX?1:0;
        for(int i=0;i<2;i++)
        { 
            var agent=agents[m_teamNo,i];           
            if(agent!=this)
            {
                Rigidbody a_RB=agent.GetComponent<Rigidbody>();
                sensor.AddObservation(m_InvertMult * (agent.transform.position.x - myArea.transform.position.x));

                sensor.AddObservation(agent.transform.position.y - myArea.transform.position.y);

                sensor.AddObservation(m_InvertMult * agent.transform.position.z);

                sensor.AddObservation(m_InvertMult * a_RB.velocity.x);

                sensor.AddObservation(a_RB.velocity.y);
                
                sensor.AddObservation(m_InvertMult * a_RB.velocity.z);
            }
        }
        for(int i=0;i<2;i++)
        {
            var agent=agents[1-m_teamNo,i];  
            Rigidbody a_RB=agent.GetComponent<Rigidbody>();
            sensor.AddObservation(m_InvertMult * (agent.transform.position.x - myArea.transform.position.x));

            sensor.AddObservation(agent.transform.position.y - myArea.transform.position.y);

            sensor.AddObservation(m_InvertMult * agent.transform.position.z);

            sensor.AddObservation(m_InvertMult * a_RB.velocity.x);

            sensor.AddObservation(a_RB.velocity.y);
            
            sensor.AddObservation(m_InvertMult * a_RB.velocity.z);
        }
        

        //球离球网的距离
        sensor.AddObservation(m_InvertMult * (ball.transform.position.x - myArea.transform.position.x));
        //球场原点至球的距离（高度)
        sensor.AddObservation(ball.transform.position.y - myArea.transform.position.y);
        //球当前x轴方向速度
        sensor.AddObservation(m_InvertMult * m_BallRb.velocity.x);
        //球当前y轴方向速度（纵向）
        sensor.AddObservation(m_BallRb.velocity.y);

        sensor.AddObservation(m_InvertMult * m_BallRb.velocity.z);

        //agent的rotation中的一维
        sensor.AddObservation(m_InvertMult * m_AgentRb.transform.localEulerAngles.z);
    }


    //需要：添加z轴信息v
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;
        //var moveX = Mathf.Clamp(continuousActions[0], -1f, 1f) * -m_InvertMult;
        //var moveY = Mathf.Clamp(continuousActions[1], -1f, 1f);
        //var moveZ = Mathf.Clamp(continuousActions[2], -1f, 1f) * m_InvertMult;
        //var RotateBat = Mathf.Clamp(continuousActions[3], -1f, 1f);
        //var RotateBody = Mathf.Clamp(continuousActions[4], -1f, 1f);
        var moveX= Mathf.Clamp(continuousActions[0], -1f, 1f) * -m_InvertMult;
        var moveZ = Mathf.Clamp(continuousActions[1], -1f, 1f) * m_InvertMult;

        // 0-1
        var Jump = discreteActions[0];
        //-1-1
        var RotateBat = discreteActions[1];
        //-1-1
        var RotateBody = discreteActions[2];
        m_endurance = Mathf.Clamp( m_endurance - (Mathf.Abs(moveX) + Mathf.Abs(Jump) + Mathf.Abs(moveZ) + Mathf.Abs(RotateBat) + Mathf.Abs(RotateBody)) * 0.1f,0f,100f);

        if (m_endurance == 0)
        {
            moveX = 0;
            moveZ = 0;
            RotateBat = -1;
            RotateBody = 0;
        }

        if (transform.position.y - transform.parent.position.y > AgentResetLocalY + 0.1)
        {
            Jump = 0;

        }

        if (Jump ==1 && transform.position.y - transform.parent.transform.position.y < -1.5f)
        {
            m_AgentRb.velocity = new Vector3(m_AgentRb.velocity.x, 7f, m_AgentRb.velocity.z);

        }
        m_AgentRb.velocity = new Vector3(moveX * moveSpeedX, m_AgentRb.velocity.y, moveZ * moveSpeedZ);


        //BatDegree += RotateBat * 3f;
        //BatDegree = Mathf.Clamp(BatDegree, 0f, 90f);
        //Vector3 Up = new Vector3(0, 1, 0);
        //Vector3 Normal = Quaternion.AngleAxis(BatDegree, new Vector3(0,0,1))* Up;
        //Debug.DrawLine(Bat.transform.position, Bat.transform.position + Normal);
        //Bat.GetComponent<Rigidbody>().AddForce(Normal * 0.001f);

        BatDegree += RotateBat * BatRotateSpeed;
        BatDegree = Mathf.Clamp(BatDegree, 45f, 120f);
        Bat.transform.localEulerAngles = new Vector3(BatDegree, 0, 0);

        //Rigidbody Bat_RigidBody = Bat.GetComponent<Rigidbody>();
        //Vector3 Bat_Up = new Vector3(0,0,0);
        //Bat_RigidBody.velocity = new Vector3(0, 1, 0);


        BodyDegree = m_AgentRb.transform.localEulerAngles.y + BodyRotateSpeed * RotateBody;
        m_AgentRb.transform.localEulerAngles = new Vector3(m_AgentRb.transform.localEulerAngles.x, BodyDegree, m_AgentRb.transform.localEulerAngles.z);

        //防止越界!!!!!!!??????
        if (invertX && transform.position.x - transform.parent.transform.position.x > m_InvertMult ||
            !invertX && transform.position.x - transform.parent.transform.position.x < m_InvertMult)
        {
            transform.position = new Vector3(m_InvertMult + transform.parent.transform.position.x,
                transform.position.y,
                transform.position.z);
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        continuousActionsOut[0] = Input.GetAxis("Vertical");    // Racket Movement
        //continuousActionsOut[1] = Input.GetKey(KeyCode.Space) ? 1f : 0f;   // Racket Jumping
        continuousActionsOut[1] = Input.GetAxis("Horizontal");   // Racket Movement
        //continuousActionsOut[2] = Input.GetMouseButton(0)? 1f : -1f;
        //continuousActionsOut[2] = Input.GetAxis("RotateBody");

        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // agent Jumping
        discreteActionsOut[1] = Input.GetMouseButton(0) ? 1 : -1;  // agent Hiting
        if(Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[2] = 1;
        }
        else if(Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[2] = -1;
        }
        else
        {
            discreteActionsOut[2] = 0;
        }

    }

    public override void OnEpisodeBegin()
    {
        BatDegree = 0;
        BodyDegree = 0;

        m_endurance =100f;
        // transform.position = new Vector3(-m_InvertMult * Random.Range(6f, 8f), -1.5f, -1.8f) + transform.parent.transform.position;
        if (id==1)
        {
            transform.position = new Vector3(m_InvertMult * Random.Range(2f, 4f), AgentResetLocalY, 0f) + transform.parent.transform.position;
        }
        if(id==2)
        {
            transform.position = new Vector3(m_InvertMult * Random.Range(6f, 8f), AgentResetLocalY, 0f) + transform.parent.transform.position;
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


}
