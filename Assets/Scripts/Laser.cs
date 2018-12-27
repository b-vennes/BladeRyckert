using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser
{
    public Vector2Int position;

    public bool charged;

    public int charge { get; set; }

    public bool firing;

    public void IncreaseCharge()
    {
        if (!charged)
        {
            charge++;
        }
        
        if (charge == 4)
        {
            charged = true;
        }
    }

    public void FireLaser()
    {
        charge = 0;
        charged = false;
    }

    public Laser()
    {
        charged = false;
        charge = 0;
        firing = false;
    }
}