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
        this.isPassed = isPassed;
    }

    public override bool Equals(object obj)
    {
        if (obj is TutorialTuple)
        {
            if (((TutorialTuple)obj).tutorialNumber == tutorialNumber && ((TutorialTuple)obj).xIndex == xIndex && ((TutorialTuple)obj).yIndex == yIndex)
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

}
