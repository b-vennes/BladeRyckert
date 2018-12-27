using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : Actor
{
    public bool targetingPlayer {get; set;}

    public bool waitingForOpenSpace {get; set;}

    public List<Vector2Int> patrol {get; set;}

    public int patrolStage {get; set;}

}