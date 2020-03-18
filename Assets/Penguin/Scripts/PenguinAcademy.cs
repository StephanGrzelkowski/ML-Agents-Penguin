using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PenguinAcademy : Academy
{
    /// <summary>
    ///  fish speed
    /// </summary>
    public float FishSpeed { get; private set; }

    /// <summary>
    /// Gets/sets the feed radius 
    /// </summary>
    public float FeedRadius { get; private set; }


    /// <summary>
    /// Init call to academy
    /// </summary>
    public override void InitializeAcademy()
    {
        FishSpeed = 0f;
        FeedRadius = 0f;

        FloatProperties.RegisterCallback("fish_speed", f =>
        {
            FishSpeed = f;
        });

        //called every time the feed radius changes during the curriculum learning
        FloatProperties.RegisterCallback("feed_radius", f =>
        {
            FeedRadius = f;
        });

    }
}