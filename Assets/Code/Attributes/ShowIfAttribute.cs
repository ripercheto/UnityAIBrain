using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
   public readonly string propertyName;
    public ShowIfAttribute(string show)
    {
        propertyName = show;
    }
}
