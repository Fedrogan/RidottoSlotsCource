using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ReelsScroll : MonoBehaviour
{
    [SerializeField] private RectTransform[] reelsRT;
    [SerializeField] private Reel[] reels;
    [SerializeField] private float delayStep;
    [SerializeField] private Ease startEase;
    [SerializeField] private Ease stopEase;
    [SerializeField] private float boostDistance, linearDistance, stoppingDistance;
    [SerializeField] private float boostDuration, linearDuration, stoppingDuration;
    [SerializeField] private Button playButton;
    [SerializeField] private RectTransform playButtonRT;
    [SerializeField] private Button stopButton;
    [SerializeField] private RectTransform stopButtonRT;

    private Dictionary<RectTransform, Reel> reelsDictionary;
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
            reelsDictionary.Add(reelsRT[i], reels[i]);
        }
        reelStartPositionY = reelsRT[0].localPosition.y;
    }

    public void ScrollStart() 
    {
        playButton.interactable = false;
        playButtonRT.localScale = Vector3.zero;
        stopButtonRT.localScale = Vector3.one;
        for (int i = 0; i < reelsRT.Length; i++)
        {

            var reelRT = reelsRT[i];
            reelRT.DOAnchorPosY(boostDistance, boostDuration)
                .SetDelay(i * delayStep)
                .SetEase(startEase)
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

    private void PrepareReel(RectTransform reelRT)
    {
        var currentReelPosY = reelRT.localPosition.y;
        print(currentReelPosY);
        reelRT.localPosition = new Vector3(reelRT.localPosition.x, reelStartPositionY);
        reelsDictionary[reelRT].ResetSymbolsPosition(currentReelPosY);

    }
}
