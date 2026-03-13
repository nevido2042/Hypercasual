using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MineableRock : MonoBehaviour
{
    private Vector3 originalScale;
    public bool CanBeMined { get; private set; } = true;
    private Collider col;

    void Start()
    {
        originalScale = transform.localScale;
        col = GetComponent<Collider>();
    }

    public void Mine()
    {
        if (!CanBeMined) return;
        CanBeMined = false;
        
        // 콜라이더를 꺼서 채광 중복이나 타겟팅 방지
        if (col != null) col.enabled = false;
        
        // 작아지면서 사라짐 (0.3초)
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            StartCoroutine(RespawnRoutine());
        });
    }

    IEnumerator RespawnRoutine()
    {
        // 1초 대기
        yield return new WaitForSeconds(1.0f);
        
        // 다시 원래 크기로 커지면서 나타남
        if (col != null) col.enabled = true;
        CanBeMined = true;
        transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);
    }
}
