using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace Hero
{
    /// <summary>
    /// 수갑(Handcuffs)이 실제로 시각적으로 적재되는 구역.
    /// 구역 감지 로직은 포함하지 않으며, 외부(HandcuffsDeliveryZone)에서 아이템을 넘겨받아 쌓기만 수행함.
    /// </summary>
    public class HandcuffsConsumeZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float stackHeight = 0.1f;
        [SerializeField] private Transform stackContainer;
        [SerializeField] private PrisonerQueueManager queueManager;
        [SerializeField] private float distributeInterval = 0.5f;
        [SerializeField] private float stackJumpDuration = 0.3f;
        [SerializeField] private float stackPopDuration = 0.3f;

        private List<Transform> consumedProducts = new List<Transform>();
        private float nextDistributeTime = 0f;
        private bool isDelivererInZone = false; // 플레이어 또는 크루 존재 여부

        [Header("Audio")]
        [SerializeField] private AudioClip stackSound;
        private AudioSource _audioSource;

        public void SetDelivererInZone(bool isInRange) => isDelivererInZone = isInRange;
        public bool HasHandcuffs() => consumedProducts.Count > 0;
        public int HandcuffCount => consumedProducts.Count;
        
        /// <summary>
        /// 죄수가 기다리고 있고 아직 수갑을 받지 못한 상태인지 확인
        /// </summary>
        public bool IsPrisonerWaiting()
        {
            if (queueManager == null || queueManager.IsQueueEmpty) return false;
            
            // 감옥이 꽉 찼으면 더이상 받지 않음
            if (jailController != null && jailController.IsFull) return false;

            Prisoner p = queueManager.GetFrontPrisoner();
            return p != null && !p.IsSatisfied && !p.IsMoving;
        }

        private JailController jailController; // SerializeField 제거

        void Awake()
        {
            if (stackContainer == null)
            {
                stackContainer = new GameObject("ConsumedProducts_Container").transform;
                stackContainer.SetParent(transform);
                stackContainer.localPosition = Vector3.zero;
            }

            // 인스펙터에서 누락되었을 경우 자동으로 찾기
            if (queueManager == null)
            {
                queueManager = FindFirstObjectByType<PrisonerQueueManager>();
            }

            if (jailController == null)
            {
                jailController = FindFirstObjectByType<JailController>();
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            // 3D Sound Settings
            _audioSource.spatialBlend = 1.0f;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.minDistance = 2f;
            _audioSource.maxDistance = 20f;
        }

        void Update()
        {
            // 배달자가 구역 안에 있을 때만 대기열의 죄수에게 아이템 배달
            if (isDelivererInZone && queueManager != null && !queueManager.IsQueueEmpty)
            {
                // 적재된 아이템이 있을 때만 로직 수행
                if (consumedProducts.Count > 0 && Time.time >= nextDistributeTime)
                {
                    Prisoner target = queueManager.GetFrontPrisoner();
                    if (target != null && !target.IsSatisfied && !target.IsMoving)
                    {
                        DistributeToPrisoner(target);
                        nextDistributeTime = Time.time + distributeInterval;
                    }
                }
            }
        }

        private void DistributeToPrisoner(Prisoner prisoner)
        {
            if (consumedProducts.Count == 0) return;

            if (_audioSource != null && stackSound != null) _audioSource.PlayOneShot(stackSound);

            int lastIndex = consumedProducts.Count - 1;
            Transform item = consumedProducts[lastIndex];
            consumedProducts.RemoveAt(lastIndex);

            // 죄수에게 아이템 전달 (Jump 연출)
            prisoner.ReceiveHandcuff(item);
        }

        public Transform TakeHandcuff()
        {
            if (consumedProducts.Count == 0) return null;

            int lastIndex = consumedProducts.Count - 1;
            Transform item = consumedProducts[lastIndex];
            consumedProducts.RemoveAt(lastIndex);

            item.DOKill();
            return item;
        }

        /// <summary>
        /// 외부에서 아이템을 전달받아 적재함
        /// </summary>
        public void ReceiveProduct(Transform item)
        {
            if (_audioSource != null && stackSound != null) _audioSource.PlayOneShot(stackSound);

            consumedProducts.Add(item);
            int index = consumedProducts.Count - 1;

            Vector3 targetLocalPos = new Vector3(0, index * stackHeight, 0);

            item.SetParent(stackContainer);
            item.DOKill();
            item.DOLocalJump(targetLocalPos, 2f, 1, stackJumpDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(item.gameObject)
                .OnComplete(() => {
                    // 적재 완료 Pop 연출
                    if (item != null) 
                    {
                        item.DOKill();
                        item.DOPunchScale(Vector3.one * 0.5f, stackPopDuration, 5, 1f).SetLink(item.gameObject);
                    }
                });
            item.DOLocalRotate(Vector3.zero, stackJumpDuration).SetLink(item.gameObject);
        }
    }
}
