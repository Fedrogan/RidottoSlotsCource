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
    [SerializeField] private RectTransform playButtonRT;
    [SerializeField] private Button stopButton;
    [SerializeField] private RectTransform stopButtonRT;

    // поле для задания задержки между началом вращений рилов
    [SerializeField] private float delayStep;
    // кривая начала вращения   
    [SerializeField] private Ease startEase;
    // кривая остановки
    [SerializeField] private Ease stopEase;
    // позиции, в которые должны двигаться рилы при разгоне, вращении и остановке
    [SerializeField] private float boostDistance, linearDistance, stoppingDistance; 
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
        reelsDictionary = new Dictionary<RectTransform, Reel>();
        for (int i = 0; i < reelsRT.Length; i++)
        {
            reelsDictionary.Add(reelsRT[i], reels[i]); // добавление в словарь рилов Reel по ключу RectTransform 
        }
        reelStartPositionY = reelsRT[0].localPosition.y; // получем начальную позицию любого из рилов
    }

    /** Метод ScrollStart срабатывает при нажатии кнопки PLAY и запускает вращения рилов */ 
    public void ScrollStart() 
    {
        playButton.interactable = false; // отключение интерактивности(кликабельности) кнопки PLAY

        playButtonRT.localScale = Vector3.zero;
        stopButtonRT.localScale = Vector3.one;
        for (int i = 0; i < reelsRT.Length; i++)
        {

            var reelRT = reelsRT[i];
            reelRT.DOAnchorPosY(boostDistance, boostDuration) // перемещение якоря рила в позицию boostDistance за время boostDuration
                .SetDelay(i * delayStep) // установка задержки между вращением рилов   
                .SetEase(startEase) // установка кривой начала вращения
                .OnComplete(() => 
                    {
                        ScrollLinear(reelRT);
                        if (reelsDictionary[reelRT].ReelId == reelsRT.Length)
                        {
                            stopButton.interactable = true;
                        }
                    });
        }
    }

    private void ScrollLinear(RectTransform reelRT)
    {
        reelsDictionary[reelRT].ReelState = ReelState.Spin;
        DOTween.Kill(reelRT);
        reelRT.DOAnchorPosY(linearDistance, linearDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => CorrectReelPos(reelRT));
    }

    private void ScrollStop(RectTransform reelRT)
    {
        reelsDictionary[reelRT].ReelState = ReelState.Stopping;
        DOTween.Kill(reelRT);
        var reelCurrentPosY = reelRT.localPosition.y;        
        var slowDownDistance = reelCurrentPosY - symbolHeight * visibleSymbolsOnReel;
        reelRT.DOAnchorPosY(slowDownDistance, stoppingDuration)
            .SetEase(stopEase)
            .OnComplete(() => 
                {
                    reelsDictionary[reelRT].ReelState = ReelState.Stop;
                    PrepareReel(reelRT);
                    if (reelsDictionary[reelRT].ReelId == reelsRT.Length)
                    {
                        stopButtonRT.localScale = Vector3.zero;
                        stopButton.interactable = false;

                        playButtonRT.localScale = Vector3.one;
                        playButton.interactable = true;
                    }
                });

    }

    private void CorrectReelPos (RectTransform reelRT)
    {
        DOTween.Kill(reelRT);
        var currentReelPos = reelRT.localPosition.y;
        var extraDistance = CalculateExtraDistance(currentReelPos);
        var correctionDistance = currentReelPos - extraDistance;
        var correctionDuration = extraDistance / -(linearDistance / linearDuration);
        reelRT.DOAnchorPosY(correctionDistance, correctionDuration)
            .OnComplete(() => ScrollStop(reelRT));
    }

    private float CalculateExtraDistance(float reelCurrentPosY)
    {
        var traveledDistance = reelStartPositionY - reelCurrentPosY;
        var symbolsScrolled = traveledDistance / symbolHeight;
        var integerPart = Mathf.Floor(symbolsScrolled);
        var fractionalPart = symbolsScrolled - integerPart;
        var extraDistance = (1 - fractionalPart) * symbolHeight;
        return extraDistance;
    }

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
    
    
    /** Метод PerpareReel используется для сброса якорей рилов и их символов
     *  на начальную позицию перед началом нового вращения. */
    private void PrepareReel(RectTransform reelRT)
    {
        var prevReelPosY = reelRT.localPosition.y; // получаем текущую позицию рила, необходимую для рассчета пройденной дистанции
        var traveledReelDistance = -(reelStartPositionY + prevReelPosY); // рассчитываем пройденную дистанцию и меняем ей знак, т.к. координаты отрицательные
        reelRT.localPosition = new Vector3(reelRT.localPosition.x, reelStartPositionY); // сброс якорей рила 
        reelsDictionary[reelRT].ResetSymbolsPosition(traveledReelDistance); // см. класс (скрипт) Reel
    }
}

