using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TutorialTuple
{
    public int tutorialNumber;
    public int xIndex;
    public int yIndex;
    public bool isPassed = false;

    public TutorialTuple(int tutorialNumber,int xIndex,int yIndex)
    {
        this.tutorialNumber = tutorialNumber;
        this.xIndex = xIndex;
        this.yIndex = yIndex;
        this.isPassed = false;
    }

    public override bool Equals(object obj)
    {
        if (obj is TutorialTuple tuple)
        {
            if (tuple.tutorialNumber == tutorialNumber && tuple.xIndex == xIndex && tuple.yIndex == yIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return base.Equals(obj);
        }
    }

    public override int GetHashCode()
    {
        return tutorialNumber * 10000 + xIndex * 100 + yIndex;
    }
}
