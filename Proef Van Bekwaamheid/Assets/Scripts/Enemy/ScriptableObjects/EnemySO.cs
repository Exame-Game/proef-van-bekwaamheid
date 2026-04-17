using System;
using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Enemys")]
public class EnemySO : ScriptableObject
{
    public float Speed;
    public float VisionRange;
    public float HearingRange;
    public float HearingThreshHold;
    
}



