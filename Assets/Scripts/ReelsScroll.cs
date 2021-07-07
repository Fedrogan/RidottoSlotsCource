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

    private Dictionary<RectTransform, Reel> reelsDictionary;
    private float reelStartPositionY;
    private float thirdReelPosX;

    private void Start()
    {
        reelsDictionary = new Dictionary<RectTransform, Reel>();
        for (int i = 0; i < reelsRT.Length; i++)
        {
            reelsDictionary.Add(reelsRT[i], reels[i]);
        }
        thirdReelPosX = reelsRT[2].position.x;
        reelStartPositionY = reelsRT[0].localPosition.y;
    }

    public void ScrollStart() 
    {
        playButton.interactable = false;
        for (int i = 0; i < reelsRT.Length; i++)
        {
            var reelRT = reelsRT[i];
            reelRT.DOAnchorPosY(boostDistance, boostDuration)
                .SetDelay(i * delayStep)
                .SetEase(startEase)
                .OnComplete(() => ScrollLinear(reelRT));
        }
    }

    private void ScrollLinear(RectTransform reelRT)
    {
        reelRT.DOAnchorPosY(linearDistance, linearDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => ScrollStop(reelRT));
    }

    private void ScrollStop(RectTransform reelRT)
    {
        reelRT.DOAnchorPosY(stoppingDistance, stoppingDuration)
            .SetEase(stopEase)
            .OnComplete(() => 
                {
                    PrepareReel(reelRT);
                    if (reelRT.position.x == thirdReelPosX)
                    {
                        playButton.interactable = true;
                    }
                });
    }

    private void PrepareReel(RectTransform reelRT)
    {
        var currentReelPosY = reelRT.localPosition.y;
        print(currentReelPosY);
        reelRT.localPosition = new Vector3(reelRT.localPosition.x, reelStartPositionY);
        reelsDictionary[reelRT].ResetSymbolsPosition(currentReelPosY);

    }
}
