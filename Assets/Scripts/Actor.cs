using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor
{
    public GameObject obj {get; set;}

    public SpriteRenderer sprite {get; set;}

    public Vector2Int gridPosition {get; set;}

    public Vector2Int gridStartPosition {get; set;}

    public Vector3 movingTo{get;set;}

    public bool isMoving {get;set;}

    public float timeStartedLerp {get; set;}

    public Vector3 lerpStartPosition {get;set;}

    public float lerpLength{get;set;}

    public Animator animator { get; set; }

    public bool facingRight { get; set; }
}