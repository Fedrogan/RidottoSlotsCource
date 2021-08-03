using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    [SerializeField] GameConfig gameConfig;
    [SerializeField] private RectTransform[] symbolsOnReel;
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
    // поле для определения текущего состояния рила и заполнения финальных экранов, работы с кнопками и т.д.
    private ReelState reelState = ReelState.Stop; 

    internal ReelState ReelState { get => reelState; set => reelState = value; }

    public int ReelId => reelId; // дефолтный геттер для поля reelId

    private void Start()
    {
        // получаем высоту любого из символов, т.к. все одинаковых размеров (можно вынести в константу или в gameConfig)
        symbolHeigth = symbolsOnReel[0].rect.height; 

        // получаем текущий масштаб холста
        // !!! не использовать в Unity методе Awake(),
        // т.к. в Awake по умолчанию присваивается Scale, установленный для
        // референсного (эталонного) разрешения, а затем в Start
        // Scale корректируется с учетом текущего разрешения экрана
        mainCanvasScale = mainCanvasRT.lossyScale.y; 
    }
    
    private void Update()
    {
        foreach (var symbol in symbolsOnReel) // проходим циклом по всем символам на риле
        {
            if (symbol.position.y <= exitPosition * mainCanvasScale) // при выходе символа за позицию-триггер:
            {
                MoveSymbolUp(symbol);                               // он перемещается вверх
                ChangeSymbolSprite(symbol);                         // на нем меняется спрайт (иконка)
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

    /// <summary>
    /// Метод возвращает случайный символ из набора символов в конфиге
    /// </summary>
    /// <returns> Sprite randomSprite</returns>
    private Sprite GetRandomSymbol()
    {        
        var random = Random.Range(0, gameConfig.Symbols.Length);    // получаем рандомное число в диапазоне от 0(включительно) до 10(не включительно)
        var sprite = gameConfig.Symbols[random].SymbolImage;        // меняем иконку символа на рандомную из конфига
        return sprite;
    }

    /// <summary>
    /// Метод возвращает нужный символ финального экрана из набора символов в конфиге
    /// </summary>
    /// <returns> Sprite finalScreenSprite</returns>
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

    /// <summary>
    /// Метод перемещает символ, вышедший за границы маски вверх.
    /// </summary>
    private void MoveSymbolUp(RectTransform symbolRT)
    {
        // рассчитываем Y позицию, в которую нужно переместить символ, чтобы он оказался сверху
        var offset = symbolRT.position.y + symbolHeigth * mainCanvasScale * symbolsOnReel.Length;
        // создаем локальную переменную типа Vector3, т.к. symbolRT.position (Transform.position) имеет тип Vector3                                                                                                                                                                                  
        var newPos = new Vector3(symbolRT.position.x, offset); // x оставляем текущий, а y присваиваем offset
        // перемещаем символ на новую позицию
        symbolRT.position = newPos;
    }

    /// <summary>
    /// Метод ResetSymbolsPosition необходим, чтобы оставить символы в рамках маски при сбросе позиции рилов.
    /// </summary>    
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
