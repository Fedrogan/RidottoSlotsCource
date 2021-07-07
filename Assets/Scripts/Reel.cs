using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    [SerializeField] private RectTransform[] symbolsOnReel;
    private const float exitPosition = 140;
    private float symbolHeigth;
    
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private RectTransform mainCanvasRT;
    private float mainCanvasScale;

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
        var random = Random.Range(0, sprites.Length);
        symbol.GetComponent<Image>().sprite = sprites[random];
    }

    public void ResetSymbolsPosition(float reelCurrentPositionY)
    {
        foreach(var symbol in symbolsOnReel)
        {
            var symbolPos = symbol.localPosition;
            var newPos = Mathf.Round(symbolPos.y + reelCurrentPositionY); 
            symbol.localPosition = new Vector3(symbolPos.x, newPos);
        }
    }

    private void MoveSymbolUp(RectTransform symbolRT)
    {
        symbolHeigth = symbolRT.rect.height;
        var offset = symbolRT.position.y + symbolHeigth * mainCanvasScale * symbolsOnReel.Length;
        var newPos = new Vector3(symbolRT.position.x, offset);
        symbolRT.position = newPos;
    }
}
