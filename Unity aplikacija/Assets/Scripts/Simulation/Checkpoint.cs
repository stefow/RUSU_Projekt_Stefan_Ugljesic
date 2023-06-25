using UnityEngine;
using System.Collections;
/// Class representing a checkpoint of a race track.
public class Checkpoint : MonoBehaviour
{
    /// The radius in Unity units in which this checkpoint can be captured.
    public float CaptureRadius = 3;
    private SpriteRenderer spriteRenderer;

    /// The reward value earned by capturing this checkpoint.
    public float RewardValue{get;set;}

    /// The distance in Unity units to the previous checkpoint on the track.
    public float DistanceToPrevious{get;set;}

    /// The accumulated distance in Unity units from the first to this checkpoint.
    public float AccumulatedDistance{get;set;}

    /// The accumulated reward earned for capturing all checkpoints from the first to this one.
    public float AccumulatedReward{get;set;}

    /// Whether or not this checkpoint is being drawn to screen.
    public bool IsVisible
    {
        get { return spriteRenderer.enabled; }
        set { spriteRenderer.enabled = value; }
    }

    // Constructors
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    /// Calculates the reward earned for the given distance to this checkpoint.
    public float GetRewardValue(float currentDistance)
    {
        //Calculate how close the distance is to capturing this checkpoint, relative to the distance from the previous checkpoint
        float completePerc = (DistanceToPrevious - currentDistance) / DistanceToPrevious; 

        //Reward according to capture percentage
        if (completePerc < 0)
            return 0;
        else return completePerc * RewardValue;
    }
}
