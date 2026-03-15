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
            // 카메라가 소실된 경우(씬 전환 등)를 대비해 다시 찾음
            if (mainCam == null) mainCam = Camera.main;

            worldTargetPos = startWorldPos;
            currentOffset = Vector3.zero;
            
            // 기존에 실행 중인 트윈이 있다면 중지 (풀링 재사용 시 중요)
            textMesh.DOKill();
            DOTween.Kill(this); // currentOffset 트윈 식별용

            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
                textMesh.alpha = 1f;
                textMesh.enabled = true;
                
                // 스케일 초기화
                transform.localScale = Vector3.one;
                
                // currentOffset 애니메이션
                DOTween.To(() => currentOffset, x => currentOffset = x, new Vector3(0, 1.5f, 0), 1.0f)
                    .SetEase(Ease.OutCubic)
                    .SetTarget(this) // Kill(this)로 지울 수 있도록 타겟 설정
                    .SetLink(gameObject);
                
                textMesh.DOFade(0, 1.0f)
                    .SetEase(Ease.InQuint)
                    .SetLink(gameObject)
                    .OnComplete(() => {
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
