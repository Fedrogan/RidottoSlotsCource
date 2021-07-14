using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    [SerializeField] GameConfig gameConfig;
    [SerializeField] private RectTransform[] symbolsOnReel;
    private const float exitPosition = 140;
    private float symbolHeigth;
    
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private RectTransform mainCanvasRT;
    private float mainCanvasScale;
    private int currentSymbolIndex = 0;
    private int currentFinalSet = 0;

    [SerializeField] int reelId;
    private ReelState reelState = ReelState.Stop;

    public int ReelId => reelId;

    internal ReelState ReelState { get => reelState; set => reelState = value; }

    private void Start()
    {
        mainCanvasScale = mainCanvasRT.lossyScale.y;
    }

    private void Update()
    {
        foreach (var symbol in symbolsOnReel)
        {
            if (symbol.position.y <= exitPosition * mainCanvasScale)
            {
                MoveSymbolUp(symbol);
                ChangeSymbolSprite(symbol);
            }
        }
    }

    private void ChangeSymbolSprite(RectTransform symbol)
    {
        if (ReelState == ReelState.Stopping || ReelState == ReelState.ForceStopping)
        {
            symbol.GetComponent<Image>().sprite = GetFinalScreenSymbol();            
        }
        else
        {
            symbol.GetComponent<Image>().sprite = GetRandomSymbol();
        }
    }

    public void ResetSymbolsPosition(float reelCurrentPositionY)
    {
        currentSymbolIndex = 0;
        if (currentFinalSet < gameConfig.FinalScreens.Length - 1)
        {
            currentFinalSet++;
        }
        else
        {
            currentFinalSet = 0;
        }

        foreach(var symbol in symbolsOnReel)
        {
            var symbolPos = symbol.localPosition;
            var newPos = Mathf.Round(symbolPos.y + reelCurrentPositionY); 
            symbol.localPosition = new Vector3(symbolPos.x, newPos);
        }
    }

    private Sprite GetRandomSymbol()
    {
        var random = Random.Range(0, gameConfig.Symbols.Length);
        var sprite = gameConfig.Symbols[random].SymbolImage;
        return sprite;
    }

    private Sprite GetFinalScreenSymbol() 
    {
        var finalScreenSymbolIndex = currentSymbolIndex + (reelId - 1) * gameConfig.VisibleSymbolsOnReel;
        var currentFinalScreen = gameConfig.FinalScreens[currentFinalSet].FinalScreen;
        if (finalScreenSymbolIndex >= currentFinalScreen.Length)
        {
            finalScreenSymbolIndex = 0;
        }
        var newSymbol = gameConfig.Symbols[currentFinalScreen[finalScreenSymbolIndex]];
        currentSymbolIndex++;
        return newSymbol.SymbolImage;
    }

    private void MoveSymbolUp(RectTransform symbolRT)
    {
        symbolHeigth = symbolRT.rect.height;
        var offset = symbolRT.position.y + symbolHeigth * mainCanvasScale * symbolsOnReel.Length;
        var newPos = new Vector3(symbolRT.position.x, offset);
        symbolRT.position = newPos;
    }
}
