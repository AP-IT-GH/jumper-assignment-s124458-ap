using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using static UnityEngine.GraphicsBuffer;


public class CubeAgent2 : Agent
{
    public GameObject Target;
    public GameObject BonusTarget;
    
    public override void OnEpisodeBegin()
    {
        // agent en obstakel op startlocatie zetten
        this.transform.localPosition = new Vector3(0, 0.75f, 2.25f);
        Target.transform.localPosition = new Vector3(0, 0.75f, -8);
        BonusTarget.transform.localPosition = new Vector3(0, 0.75f, -8);

        // target enabelen en bonustarget disabelen
        Target.SetActive(true);
        BonusTarget.SetActive(false);

        // target naar voor bewegen
        
        float snelheid = UnityEngine.Random.Range(4f, 6f);
        Rigidbody rb_target = Target.GetComponent<Rigidbody>();
        rb_target.velocity = new Vector3(0, 0, snelheid);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent positie
        sensor.AddObservation(this.transform.localPosition);
    }

    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 5;
    int succesvolleSprongen = 0;
    float jumpinput = 0;
    float nietsdoen = 0;
    Vector3 sprong;
    float sprongkracht = 2.0f;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        jumpinput = actionBuffers.ContinuousActions[0];
        nietsdoen = actionBuffers.ContinuousActions[1];
        Rigidbody rb_self = GetComponent<Rigidbody>();
        Rigidbody rb_target = Target.GetComponent<Rigidbody>();
        Rigidbody rb_bonus = BonusTarget.GetComponent<Rigidbody>();
        sprong = new Vector3(0.0f, 5.0f, 0.0f);

        if (jumpinput > 0)
        {
            if (this.transform.localPosition.y < 0.80f)
            {
                rb_self.AddForce(sprong * sprongkracht, ForceMode.Impulse);
                AddReward(-0.01f);
            }
        }

        if (nietsdoen > 0)
        {
            if (this.transform.localPosition.y < 0.80f)
            {
                AddReward(0.01f);
                // voor de rest niets
            }

        }
        // Beloningen
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
        float distanceToBonusTarget = Vector3.Distance(this.transform.localPosition, BonusTarget.transform.localPosition);

        // target aangeraakt
        if (distanceToTarget < 1.32f)
        {
            AddReward(-0.5f);
            succesvolleSprongen = 0;
            EndEpisode();
        }

        // bonustarget aangeraakt. deze verschijnt enkel bij de 2e poging
        if (distanceToBonusTarget < 1.32f)
        {
            SetReward(1f);
            succesvolleSprongen = 0;
            EndEpisode();
            // bonustarget van het spelbord halen
            BonusTarget.transform.localPosition = new Vector3(4, 0.75f, -8f);
            rb_bonus.velocity = new Vector3(0, 0, 0);
        }

        if (Target.transform.localPosition.z > 5)
        {
            AddReward(0.8f);
            // target van het spelbord halen
            Target.transform.localPosition = new Vector3(0, 0.75f, -10f);
            rb_target.velocity = new Vector3(0, 0, 0);
            succesvolleSprongen = succesvolleSprongen + 1;
            float BonusKans = UnityEngine.Random.Range(0f, 2f);
            float snelheid = UnityEngine.Random.Range(4f, 6f);
            // random beslissen of de bonustarget verschijnt
            if (BonusKans >= 1f)
            {
                Target.SetActive(false);
                BonusTarget.SetActive(true);
                BonusTarget.transform.localPosition = new Vector3(0, 0.75f, -8f);
                rb_bonus.velocity = new Vector3(0, 0, snelheid);
            }
            else
            {
                Target.transform.localPosition = new Vector3(0, 0.75f, -8f);
                rb_target.velocity = new Vector3(0, 0, snelheid);
            }

        }

        // bonustarget gemist
        if (BonusTarget.transform.localPosition.z > 5)
        {
            BonusTarget.transform.localPosition = new Vector3(4, 0.75f, -8f);
            rb_bonus.velocity = new Vector3(0, 0, 0);
            AddReward(-0.5f);
            succesvolleSprongen = 0;
            EndEpisode();
        }

        // grote reward bij 2 keer erover springen
        if (succesvolleSprongen == 2)
        {
           SetReward(1.0f);
        
           succesvolleSprongen = 0;
            EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }


}