using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitWall : MonoBehaviour
{
    public GameObject areaObject;
    public int lastAgentHit;
    public bool net;
    public float TimePaneltyRate;

    bool flag;

    public enum FloorHit
    {
        Service,
        FloorHitUnset,
        FloorAHit,
        FloorBHit
    }

    public FloorHit lastFloorHit;

    Area m_Area;
    BedmintonAgent t1_AgentA;
    BedmintonAgent t1_AgentB;
    BedmintonAgent t2_AgentA;
    BedmintonAgent t2_AgentB;

    //  Use this for initialization
    void Start()
    {
        flag = false;
        m_Area = areaObject.GetComponent<Area>();
        t1_AgentA = m_Area.t1_AgentA;
        t1_AgentB = m_Area.t1_AgentB;

        t2_AgentA = m_Area.t2_AgentA;
        t2_AgentB = m_Area.t2_AgentB;
    }


    void SetEndurancePanelty()
    {
        t1_AgentA.AddReward(-(100 - t1_AgentA.m_endurance) * TimePaneltyRate);
        t1_AgentB.AddReward(-(100 - t1_AgentB.m_endurance) * TimePaneltyRate);
        t2_AgentA.AddReward(-(100 - t2_AgentA.m_endurance) * TimePaneltyRate);
        t2_AgentB.AddReward(-(100 - t2_AgentB.m_endurance) * TimePaneltyRate);
    }

    void showReward()
    {
        Debug.Log(t1_AgentA.name+t1_AgentA.GetCumulativeReward());
        Debug.Log(t1_AgentB.name+t1_AgentB.GetCumulativeReward());
        Debug.Log(t2_AgentA.name+t2_AgentA.GetCumulativeReward());
        Debug.Log(t2_AgentB.name+t2_AgentB.GetCumulativeReward());
    }
    void Reset()
    {
        flag = false;
        showReward();
        t1_AgentA.EndEpisode();
        t1_AgentB.EndEpisode();
        t2_AgentA.EndEpisode();
        t2_AgentB.EndEpisode();

        m_Area.MatchReset();
        lastAgentHit = -1;
        //net = false;
    }

    void t1Wins()
    {
        Debug.Log("T1Wins!!");
        t1_AgentA.AddReward(1);
        t1_AgentB.AddReward(1);
        t2_AgentA.AddReward(-1);
        t2_AgentB.AddReward(-1);
        t1_AgentA.score += 1;
        t1_AgentB.score += 1;
        SetEndurancePanelty();

        Reset();
        m_Area.isEnding = true;
    }

    void t2Wins()
    {
        Debug.Log("T2Wins!!");
        t1_AgentA.AddReward(-1);
        t1_AgentB.AddReward(-1);
        t2_AgentA.AddReward(1);
        t2_AgentB.AddReward(1);
        t2_AgentA.score += 1;
        t2_AgentB.score += 1;
        SetEndurancePanelty();

        Reset();
        m_Area.isEnding = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("iWall"))
        {
            if (collision.gameObject.name == "wallA" || collision.gameObject.name == "wallC" || collision.gameObject.name == "wallD")
            {
                // Agent A hits into wall or agent B hit a winner
                if (lastAgentHit == 0)
                {
                    Debug.Log("t1 hits into wall or t2 hit a winner");
                    t2Wins();
                }
                else
                {
                    Debug.Log("t2 hits long");
                    t1Wins();
                }
            }
            else if (collision.gameObject.name == "wallB" || collision.gameObject.name == "wallE" || collision.gameObject.name == "wallF")
            {
                // t2 hits into wall or t1 hit a winner
                //if (lastAgentHit == 1 || lastFloorHit == FloorHit.FloorBHit)
                if (lastAgentHit == 1)
                {
                    Debug.Log("t2 hits into wall or t1 hit a winner");
                    t1Wins();
                }
                // t1 hits long
                else
                {
                    Debug.Log("t1 hits long");
                    t2Wins();
                }
            }
            else if (collision.gameObject.name == "floorA")
            {
                Debug.Log("hit floor");
                t2Wins();

            }
            else if (collision.gameObject.name == "floorB")
            {
                Debug.Log("hit floor");
                t1Wins();
            }
            //else if (collision.gameObject.name == "net" && !net)
            else if (collision.gameObject.name == "net")
            {
                if (lastAgentHit == 0)
                {
                    Debug.Log("net");
                    t2Wins();
                }
                else if (lastAgentHit == 1)
                {
                    Debug.Log("net");
                    t1Wins();
                }
            }
        }
        else if (collision.gameObject.name == "t1_AgentA" || collision.gameObject.name == "t1_AgentB" || collision.gameObject.name == "t1ABat" || collision.gameObject.name == "t1BBat")
        {
            Invoke("ConvertFlag", 0.5f);
            // t1 double hit
            if (lastAgentHit == 0 && flag)
            {
                Debug.Log("t1 double hit");
                flag = false;
                t2Wins();
            }
            else
            {
                if((lastAgentHit == 1 || lastAgentHit == -1)&& (collision.gameObject.name == "t1ABat" || collision.gameObject.name == "t1BBat"))
                {
                    var agent = collision.gameObject.GetComponentInParent<BedmintonAgent>();
                    if(agent.hitting)
                    {
                        t1_AgentA.AddReward(0.2f);
                        t1_AgentB.AddReward(0.2f);
                    }
                    
                }
                lastAgentHit = 0;
            }
        }
        else if (collision.gameObject.name == "t2_AgentA" || collision.gameObject.name == "t2_AgentB" || collision.gameObject.name == "t2ABat" || collision.gameObject.name == "t2BBat")
        {
            Invoke("ConvertFlag", 0.5f);
            // t2 double hit
            if (lastAgentHit == 1 && flag)
            {
                Debug.Log("t2 double hit");
                flag = false;
                t1Wins();
            }
            else
            {
                if(lastAgentHit==0 || lastAgentHit == -1)
                {

                    t2_AgentA.AddReward(0.2f);
                    t2_AgentB.AddReward(0.2f);
                }
                lastAgentHit = 1;
            }

        }
    }

    void ConvertFlag()
    {
        flag = true;
    }
}
