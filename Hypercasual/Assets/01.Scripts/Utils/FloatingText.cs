using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 월드 좌표를 스크린 좌표로 변환하여 캐릭터 머리 위에 텍스트를 표시하는 UI 연출
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        private TMP_Text textMesh;
        private RectTransform rectTransform;
        private Vector3 worldTargetPos;
        private Vector3 currentOffset;
        private Camera mainCam;
        private ReturnToPool returnToPool;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            textMesh = GetComponentInChildren<TMP_Text>();
            mainCam = Camera.main;
            returnToPool = GetComponent<ReturnToPool>();
        }

        public void Setup(Vector3 startWorldPos, string text, Color color)
        {
            worldTargetPos = startWorldPos;
            currentOffset = Vector3.zero;

            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
                textMesh.alpha = 1f; // 풀 사용 시 이전 상태 초기화
                
                // DOTween 연출: 오프셋 값을 위로 이동시키면서 페이드 아웃
                DOTween.To(() => currentOffset, x => currentOffset = x, new Vector3(0, 1.5f, 0), 1.0f)
                    .SetEase(Ease.OutCubic);
                
                textMesh.DOFade(0, 1.0f).SetEase(Ease.InQuint).OnComplete(() => {
                    if (returnToPool != null) returnToPool.Release();
                    else Destroy(gameObject);
                });
            }
            else
            {
                if (returnToPool != null) returnToPool.Release();
                else Destroy(gameObject, 1.0f);
            }
        }

        void LateUpdate()
        {
            if (mainCam == null) return;

            // 월드 좌표 + 애니메이션 오프셋 -> 스크린 좌표 변환
            Vector3 screenPos = mainCam.WorldToScreenPoint(worldTargetPos + currentOffset);
            
            // 화면 뒤에 있는 경우 텍스트 숨김 처리
            if (screenPos.z < 0)
            {
                if (textMesh != null) textMesh.enabled = false;
                return;
            }

            if (textMesh != null) textMesh.enabled = true;
            rectTransform.position = screenPos;
        }
    }
}
