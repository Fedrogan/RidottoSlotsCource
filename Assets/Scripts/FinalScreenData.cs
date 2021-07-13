using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Final Screen", menuName = "Final Screen")]
public class FinalScreenData : ScriptableObject
{
    [SerializeField] private int[] finalScreen;

    public int[] FinalScreen => finalScreen;
}
