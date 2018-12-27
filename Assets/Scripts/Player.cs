using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player : Actor
{
    public bool isHidden {get; set;}
    public bool alive { get; set; }
}