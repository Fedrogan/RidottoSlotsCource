using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ReelsScroll : MonoBehaviour
{
    [SerializeField] private RectTransform[] reelsRT;
    [SerializeField] private Reel[] reels;
    [SerializeField] private Button playButton;
    [SerializeField] private Button stopButton;
    // поля для скрытия и показа кнопок
    [SerializeField] private RectTransform playButtonRT;    
    [SerializeField] private RectTransform stopButtonRT;

    // поле для задания задержки между началом вращений рилов
    [SerializeField] private float delayStep;
    // кривая начала вращения   
    [SerializeField] private Ease startEase;
    // кривая остановки
    [SerializeField] private Ease stopEase;
    // позиции, в которые должны двигаться рилы при разгоне и вращении
    [SerializeField] private float boostDistance, linearDistance; 
    // время, за которое перемещаются рилы в эти позиции
    [SerializeField] private float boostDuration, linearDuration, stoppingDuration;

    // словарь для связи рилов с их RectTransform, нужный для того, чтобы не использовать "дорогой" метод GetComponent()
    private Dictionary<RectTransform, Reel> reelsDictionary;
    // стартовая позиция рилов для возврата якорей перед началом нового вращения
    private float reelStartPositionY;

    [SerializeField] private float symbolHeight;
    [SerializeField] private int visibleSymbolsOnReel;
    
    private void Start()
    {
        stopButton.interactable = false;
        stopButtonRT.localScale = Vector3.zero;
        reelsDictionary = new Dictionary<RectTransform, Reel>();    // создаем новый словарь
        for (int i = 0; i < reelsRT.Length; i++)
        {
            reelsDictionary.Add(reelsRT[i], reels[i]);              // добавление в словарь рилов Reel по ключу RectTransform 
        }
        reelStartPositionY = reelsRT[0].localPosition.y;            // получем начальную позицию любого из рилов
    }      

    /// <summary>
    ///  Метод ScrollStart выполняется при нажатии кнопки PLAY и запускает вращения рилов
    /// </summary>
    public void ScrollStart() 
    {
        playButton.interactable = false; // отключение интерактивности(кликабельности) кнопки PLAY

        playButtonRT.localScale = Vector3.zero;
        stopButtonRT.localScale = Vector3.one;
        for (int i = 0; i < reelsRT.Length; i++)
        {

            var reelRT = reelsRT[i];
            reelRT.DOAnchorPosY(boostDistance, boostDuration)   // перемещение якоря рила в позицию boostDistance за время boostDuration
                .SetDelay(i * delayStep)                        // установка задержки между вращением рилов   
                .SetEase(startEase)                             // установка кривой начала вращения
                .OnComplete(() => 
                    {
                        ScrollLinear(reelRT); // при завершении твина выполняется метод ScrollLinear и в него передается текущий рил

                        // когда запускается 3й рил, кнопка STOP становится кликабельной и можно остановить вращения.
                        // если активировать кнопку сразу после нажатия кнопки Play, то можно "сломать" вращения
                        // (будет явный прокрут еще не запущенных рилов, что может вызвать сомнения у пользователя в честности слота)
                        if (reelsDictionary[reelRT].ReelId == reelsRT.Length)
                        {
                            stopButton.interactable = true;
                        }
                    });
        }
    }

    /// <summary>
    ///  Метод ScrollLinear выполняется для каждого рила по завершении стадии "разгона"
    /// </summary>
    private void ScrollLinear(RectTransform reelRT)
    {
        reelsDictionary[reelRT].ReelState = ReelState.Spin; // переключение состояния рила
        DOTween.Kill(reelRT);                               // убиваем твин у рила, чтобы не было "наложения" твинов
        reelRT.DOAnchorPosY(linearDistance, linearDuration) // перемещение якоря рила в позицию linearDistance за время linearDuration
            .SetEase(Ease.Linear)                           // установка кривой линейного вращения
            .OnComplete(() => CorrectReelPos(reelRT));      // при завершении твина выполняется метод CorrectReelPos
    }

    /// <summary>
    ///  Метод ScrollStop служит для остановки рилов.
    /// </summary>
    private void ScrollStop(RectTransform reelRT)
    {
        reelsDictionary[reelRT].ReelState = ReelState.Stopping;                         // переключение состояния рила
        DOTween.Kill(reelRT);                                                           // убиваем твин у рила, чтобы не было "наложения" твинов
        var reelCurrentPosY = reelRT.localPosition.y;                                   // получаем текущую позицию рила       
        var stoppingDistance = reelCurrentPosY - symbolHeight * visibleSymbolsOnReel;   // считаем позицию остановки
        reelRT.DOAnchorPosY(stoppingDistance, stoppingDuration)                         // перемещение якоря рила в позицию stoppingDistance за время stoppingDuration
            .SetEase(stopEase)                                                          // установка кривой начала вращения
            .OnComplete(() => 
            {
                    reelsDictionary[reelRT].ReelState = ReelState.Stop;                 // переключение состояния рила
                    PrepareReel(reelRT);                                                // готовим рил к следующему вращению
                    if (reelsDictionary[reelRT].ReelId == reelsRT.Length)
                    {
                        stopButtonRT.localScale = Vector3.zero;     // выключение кнопки STOP
                        stopButton.interactable = false;

                        playButtonRT.localScale = Vector3.one;      // активация кнопки PLAY
                        playButton.interactable = true;
                    }
                });

    }

    /// <summary>
    /// Метод CorrectReelPos используется для корректировки расположения рила при нажатии кнопки STOP.
    /// "Подкручивает" рил до позиции, из которой начинается остановка.
    /// Это необходимо для того, чтобы правильно заполнялись финальные экраны и рилы останавливались в нужной
    /// позиции при фиксированной дистанции остановки
    /// </summary>
    private void CorrectReelPos (RectTransform reelRT)
    {
        DOTween.Kill(reelRT);                                                           // убиваем твин у рила, чтобы не было "наложения" твинов
        var currentReelPos = reelRT.localPosition.y;                                    // получаем текущую позицию рила
        var extraDistance = CalculateExtraDistance(currentReelPos);                     // считаем дистанцию для полного "выезда" верхнего символа 
        var correctionDistance = currentReelPos - extraDistance;                        // считаем позицию коррекции
        var correctionDuration = extraDistance / -(linearDistance / linearDuration);    // считаем время коррекции с учетом скорости линейного вращения, чтобы не было рывка
        reelRT.DOAnchorPosY(correctionDistance, correctionDuration)                     // перемещаем якорь рила
            .OnComplete(() => ScrollStop(reelRT));
    }

    /// <summary>
    /// Метод CalculateExtraDistance по текущей позиции рила рассчитывает дистанцию, 
    /// на которую нужно прокрутить рил, чтобы полностью показать верхний символ.
    /// </summary> 
    /// <returns> 
    /// float extraDistance 
    /// </returns>
    private float CalculateExtraDistance(float reelCurrentPosY)
    {
        var traveledDistance = reelStartPositionY - reelCurrentPosY;    // определяем расстояние, которое проехал рил
        var partOfUpperSymbol = traveledDistance % symbolHeight;        // определяем какая часть верхнего символа уже выехала
        var extraDistance = symbolHeight - partOfUpperSymbol;           // определяем оставшуюся часть символа
        
        return extraDistance;
    }

    /// <summary>
    /// Метод ForceScrollStop при нажатии кнопки STOP запускает у всех рилов выполнение методов остановки.
    /// </summary>
    public void ForceScrollStop()
    {
        stopButton.interactable = false; 

        foreach (var reelRT in reelsRT)
        {
            if (reelsDictionary[reelRT].ReelState == ReelState.Spin)
            {
                CorrectReelPos(reelRT);
            }            
        }
    }
        
    /// <summary> 
    /// Метод PerpareReel используется для сброса якорей рилов и их символов
    /// на начальную позицию перед началом нового вращения.
    /// </summary>
    private void PrepareReel(RectTransform reelRT)
    {
        var prevReelPosY = reelRT.localPosition.y;                                      // получаем текущую позицию рила, необходимую для рассчета пройденной дистанции
        var traveledReelDistance = -(reelStartPositionY + prevReelPosY);                // рассчитываем пройденную дистанцию и меняем ей знак, т.к. координаты отрицательные
        reelRT.localPosition = new Vector3(reelRT.localPosition.x, reelStartPositionY); // сброс якорей рила 
        reelsDictionary[reelRT].ResetSymbolsPosition(traveledReelDistance);             // см. класс (скрипт) Reel
    }
}

