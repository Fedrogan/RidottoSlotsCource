﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    [SerializeField] GameConfig gameConfig;
    [SerializeField] private RectTransform[] symbolsOnReel;
    // массив иконок (10, J, A и т.д.)
    [SerializeField] private Sprite[] sprites;
    // RectTransform основного холста
    [SerializeField] private RectTransform mainCanvasRT;

    [SerializeField] private int reelID;

    // позиция-триггер, при выходе за которую символы перемещаются наверх
    private const float exitPosition = 140;
    // высота символов
    private float symbolHeigth;
    // масштаб холста, который нужно учитывать для корректной работы в разных разрешениях экрана
    private float mainCanvasScale;
    private int currentSymbolIndex = 0;
    private int currentFinalSet = 0;

    [SerializeField] int reelId;
    private ReelState reelState = ReelState.Stop;

    public int ReelId => reelId;

    internal ReelState ReelState { get => reelState; set => reelState = value; }

    public int ReelID => reelID; // дефолтный геттер для поля reelID

    private void Start()
    {
        symbolHeigth = symbolsOnReel[0].rect.height; // получаем высоту любого из символов (т.к. все одинаковых размеров)

        mainCanvasScale = mainCanvasRT.lossyScale.y; // получаем текущий масштаб холста
                                                     // !!! не использовать в Unity методе Awake(),
                                                     // т.к. в Awake по умолчанию присваивается Scale, установленный для
                                                     // референсного (эталонного) разрешения, а затем в Start
                                                     // Scale корректируется с учетом текущего разрешения экрана
    }

    /** Стандартный метод Unity, который выполняется каждый кадр*/
    private void Update()
    {
        foreach (var symbol in symbolsOnReel) // проходим циклом по всем символам на риле
        {
            if (symbol.position.y <= exitPosition * mainCanvasScale) // при выходе символа за позицию-триггер:
            {
                MoveSymbolUp(symbol); // он перемещается вверх
                ChangeSymbolSprite(symbol); // на нем меняется спрайт (иконка)
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
        // рассчитываем Y позицию, в которую нужно переместить символ, чтобы он оказался сверху
        var offset = symbolRT.position.y + symbolHeigth * mainCanvasScale * symbolsOnReel.Length;
        // создаем локальную переменную типа Vector3, т.к. symbolRT.position (Transform.position) имеет тип Vector3                                                                                                                                                                                  
        var newPos = new Vector3(symbolRT.position.x, offset); // x оставляем текущий, а y присваиваем offset
        // перемещаем символ на новую позицию
        symbolRT.position = newPos;
    }

    /** Метод ResetSymbolsPosition необходим, чтобы оставить символы в рамках маски при сбросе позиции рилов.
     *  Принимает пройденную рилом дистанцию и отнимает ее от текущей позиции символов*/
    public void ResetSymbolsPosition(float reelTraveledDistance)
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
        foreach (var symbol in symbolsOnReel)
        {
            var symbolPos = symbol.localPosition; // получаем текущую позицию символа
            var correction = Mathf.Round(symbolPos.y - reelTraveledDistance); 
            var correctedfPos = new Vector3(symbolPos.x, correction);
            symbol.localPosition = correctedfPos;
        }
    }
}
