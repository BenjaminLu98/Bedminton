﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HitWall : MonoBehaviour
{
    public GameObject areaObject;
    public int lastAgentHit;
    public bool net;
    public float TimePaneltyRate;

    bool t1flag;
    bool t2flag;

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
        t1flag = false;
        t2flag = false;
        m_Area = areaObject.GetComponent<Area>();
        t1_AgentA = m_Area.t1_AgentA;
        t1_AgentB = m_Area.t1_AgentB;


        t2_AgentA = m_Area.t2_AgentA;
        t2_AgentB = m_Area.t2_AgentB;
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
        t1flag = false;
        t2flag = false;
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



        t1_AgentA.score += 1;
        t1_AgentB.score += 1;


        Reset();
        m_Area.isEnding = true;
    }

    void t2Wins()
    {
        Debug.Log("T2Wins!!");
       
        t2_AgentA.score += 1;
        t2_AgentB.score += 1;

        Reset();
        m_Area.isEnding = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("iWall"))
        {
            if (collision.gameObject.name == "wallA" || collision.gameObject.name == "wallC" || collision.gameObject.name == "wallD")
            {
                // Agent A hits into wall 
                if (lastAgentHit == 0)
                {
                    Debug.Log("t1 hits into wall");
                    t1_AgentA.AddReward(-0.2f);
                    t1_AgentB.AddReward(-0.2f);
                    t2Wins();
                    
                }
                //避免重置后再次和墙相撞
                else if (lastAgentHit == 1)
                {
                    Debug.Log("t2 hits long");
                    t2_AgentA.AddReward(-0.4f);
                    t2_AgentB.AddReward(-0.4f);
                    t1Wins();
                    
                }
            }
            else if (collision.gameObject.name == "wallB" || collision.gameObject.name == "wallE" || collision.gameObject.name == "wallF")
            {
                if (lastAgentHit == 1)
                {
                    Debug.Log("t2 hits into wall");
                    t2_AgentA.AddReward(-0.2f);
                    t2_AgentB.AddReward(-0.2f);
                    t1Wins();
                    

                }
                //避免重置后再次和墙相撞
                else if(lastAgentHit == 0)
                {
                    
                    Debug.Log("t1 hits long");
                    t1_AgentA.AddReward(-0.4f);
                    t1_AgentB.AddReward(-0.4f);
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
        else if (collision.gameObject.name == "t1_AgentA" || collision.gameObject.name == "t1ABat"|| collision.gameObject.name == "t1_AgentB" || collision.gameObject.name == "t1BBat")
        {
            t2flag = false;
            Invoke("Convertt1Flag", 0.5f);
            // t1 double hit
            if (lastAgentHit == 0 && t1flag)
            {
                Debug.Log("t1 double hit");

                t2Wins();
            }
            else
            {
                //t2最后击球或t1发球时，t1球拍碰到球
                if((lastAgentHit == 1 || lastAgentHit == -1)&& (collision.gameObject.name == "t1ABat"|| collision.gameObject.name == "t1BBat"))
                {
                    t1_AgentA.AddReward(0.3f);
                    t1_AgentB.AddReward(0.3f);
                }
                lastAgentHit = 0;
            }
        }
        else if (collision.gameObject.name == "t2_AgentA" || collision.gameObject.name == "t2ABat"|| collision.gameObject.name == "t2_AgentB" || collision.gameObject.name == "t2BBat")
        {
            t1flag = false;
            Invoke("Convertt2Flag", 0.5f);
            // t2 double hit
            if (lastAgentHit == 1 && t2flag)
            {

                Debug.Log("t2 double hit");
                
                t1Wins();
            }
            else
            {
                //t1最后击球或t2发球时，t2球拍碰到球
                if ((lastAgentHit == 0 || lastAgentHit == -1) && (collision.gameObject.name == "t2ABat"|| collision.gameObject.name == "t2BBat"))
                {

                    t2_AgentA.AddReward(0.3f);
                    t2_AgentB.AddReward(0.3f);
                }
                lastAgentHit = 1;

            }

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "over")
        {
            switch (lastAgentHit)
            {
                case 0:
                    Debug.LogWarning("t1AB击球过网奖励");
                    t1_AgentA.AddReward(0.5f);
                    t1_AgentB.AddReward(0.5f);

                    break;
                case 1:
                    Debug.LogWarning("t2AB击球过网奖励");
                    t2_AgentA.AddReward(0.5f);
                    t2_AgentB.AddReward(0.5f);

                    break;

            }
        }
    }


    void Convertt2Flag()
    {
        if(lastAgentHit!=-1)
        {
            t2flag = true;
        }
       
    }

    void Convertt1Flag()
    {

        if (lastAgentHit != -1)
        {
            t1flag = true;
        }
    }
}
