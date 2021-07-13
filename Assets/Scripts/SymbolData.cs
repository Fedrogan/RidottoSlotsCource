using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Symbol Data", menuName = "Symbol Data")]
public class SymbolData : ScriptableObject
{
    [SerializeField] private Sprite symbolImage;
    [SerializeField] private float symbolCost;
    [SerializeField] private SymbolType symbolType;

    public Sprite SymbolImage => symbolImage;

    public float SymbolCost => symbolCost;

    internal SymbolType SymbolType => symbolType;
}

