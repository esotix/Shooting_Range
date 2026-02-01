using UnityEngine;
using System.Collections.Generic;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    private int currentScore = 0;
    private int activeTargetsCount = 0;

    void Awake()
    {
        Instance = this;
    }

    public void AddScore(int amount)
    {
        if (amount > 0)
        {
            currentScore += amount;
            Debug.Log($"Score mis à jour : {currentScore}");
        }
    }

    public int GetScore()
    {
        return currentScore;
    }

    public void TargetActivated()
    {
        activeTargetsCount++;
    }

    public void TargetDeactivated()
    {
        activeTargetsCount--;
    }

    public int GetActiveTargetsCount()
    {
        return activeTargetsCount;
    }
}