using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace Hero
{
    /// <summary>
    /// 플레이어 등 뒤에 젬스톤을 쌓고 휘청거리는 연출을 담당
    /// </summary>
    public class PlayerStack : MonoBehaviour, IHandcuffProvider
    {
        [Header("젬스톤 스택")]
        [SerializeField] private Transform stackPoint;        // 젬스톤이 쌓이기 시작할 위치
        [SerializeField] private float verticalSpacing = 0.2f; // 아이템 간 수직 간격
        private List<Transform> stackList = new List<Transform>();
        
        [Header("전면 수갑 스택")]
        [SerializeField] private Transform frontStackPoint;   // 수갑이 쌓일 앞쪽 위치
        [SerializeField] private float frontVerticalSpacing = 0.1f; // 수갑 전용 수직 간격
        private List<Transform> handcuffsStack = new List<Transform>();

        [Header("현금 스택")]
        [SerializeField] private Transform moneyStackPoint;   // 현금이 쌓일 뒤쪽 위치
        [SerializeField] private float moneyVerticalSpacing = 0.07f; // 현금 전용 수직 간격 (기존의 약 1/3)
        private List<Transform> moneyStack = new List<Transform>();

        [Header("Audio")]
        [SerializeField] private AudioClip stackSound;
        private AudioSource _audioSource;

        public int GemstoneCount => stackList.Count;
        public int HandcuffCount => handcuffsStack.Count;
        public int MoneyCount => moneyStack.Count;

        public System.Action OnGemstoneAdded;
        public System.Action OnGemstoneRemoved;
        public System.Action OnHandcuffAdded;
        public System.Action OnHandcuffRemoved;
        public System.Action OnMoneyAdded;
        public System.Action OnMoneyStackChanged; // 하위 호환성 유지
        
        [Header("스택 배치 설정")]
        [SerializeField] private float baseBackOffset = -0.45f;    // 첫 번째 줄(젬스톤)의 Z 위치
        [SerializeField] private float stackLineDistance = 0.35f;  // 줄 사이의 간격
        [SerializeField] private float tiltSensitivity = 2f;      // 이동 시 기울어지는 감도

        private PlayerMovement movement;

        [Header("적재 제한")]
        [SerializeField] private int maxCapacity = 10; // 최대 적재 수량
        [SerializeField] private GameObject maxTextPrefab; // "Max" 텍스트 프리팹

        private float nextMaxTextTime = 0f;
        private const float MAX_TEXT_COOLDOWN = 0.5f;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float stackJumpDuration = 0.25f; // 점프/회전 시간
        [SerializeField] private float stackScaleDuration = 0.2f; // 크기 조절 시간

        private Canvas cachedCanvas;
        private Vector3 lastPosition;
        private Vector3 currentVelocity;
        private Vector2 smoothLean;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        void Start()
        {
            movement = GetComponent<PlayerMovement>();
            lastPosition = transform.position;
            GameObject canvasObj = GameObject.FindWithTag("MainCanvas");
            if (canvasObj != null) cachedCanvas = canvasObj.GetComponent<Canvas>();
            
            if (cachedCanvas == null) cachedCanvas = Object.FindFirstObjectByType<Canvas>();
            
            if (stackPoint == null)
            {
                GameObject go = new GameObject("StackPoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0, 1f, baseBackOffset);
                stackPoint = go.transform;
            }

            if (moneyStackPoint == null)
            {
                GameObject go = new GameObject("MoneyStackPoint");
                go.transform.SetParent(transform);
                // 초기 위치는 젬스톤 위치와 동일하게
                go.transform.localPosition = new Vector3(0, 1f, baseBackOffset);
                moneyStackPoint = go.transform;
            }

            if (frontStackPoint == null)
            {
                GameObject go = new GameObject("FrontStackPoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0, 1f, Mathf.Abs(baseBackOffset)); // 대칭적으로 앞쪽
                frontStackPoint = go.transform;
            }
        }

        void Update()
        {
            HandlePhysicsSway();
        }

        /// <summary>
        /// 실제 위치 변화(가속도)를 바탕으로 관성 기울기 계산
        /// </summary>
        void HandlePhysicsSway()
        {
            if (stackList.Count == 0 && handcuffsStack.Count == 0 && moneyStack.Count == 0) return;

            // 1. 현재 속도 계산
            Vector3 worldVelocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;

            // 2. 캐릭터의 로컬 좌표계 기준 속도 (전진/후진, 좌/우)
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);
            
            // 3. 속도 변화량(가속도)에 따른 기울기 목표값 설정
            currentVelocity = Vector3.Lerp(currentVelocity, localVelocity, Time.deltaTime * 10f);
            Vector3 deltaVel = localVelocity - currentVelocity;

            float targetX = -deltaVel.z * tiltSensitivity;
            float targetZ = deltaVel.x * tiltSensitivity;

            // 4. 기울기 값을 부드럽게 보간
            smoothLean.x = Mathf.Lerp(smoothLean.x, targetX, Time.deltaTime * 5f);
            smoothLean.y = Mathf.Lerp(smoothLean.y, targetZ, Time.deltaTime * 5f);

            Quaternion targetRot = Quaternion.Euler(smoothLean.x, 0, smoothLean.y);

            // 5. 모든 체인 마디에 적용
            foreach (var gem in stackList)
            {
                gem.localRotation = Quaternion.Slerp(gem.localRotation, targetRot, Time.deltaTime * 5f);
            }

            foreach (var hc in handcuffsStack)
            {
                hc.localRotation = Quaternion.Slerp(hc.localRotation, targetRot, Time.deltaTime * 5f);
            }

            foreach (var money in moneyStack)
            {
                money.localRotation = Quaternion.Slerp(money.localRotation, targetRot, Time.deltaTime * 5f);
            }
        }

        private void UpdateStackPositions()
        {
            // 젬스톤 줄은 항상 앞쪽(첫 번째 줄) 고정
            // 현금 줄은 젬스톤이 있을 때만 더 뒤로 밀려남
            float moneyTargetZ = (stackList.Count > 0) ? (baseBackOffset - stackLineDistance) : baseBackOffset;
            Vector3 targetPos = new Vector3(0, 1f, moneyTargetZ);

            if (moneyStackPoint != null)
            {
                moneyStackPoint.localScale = Vector3.one; // 머니스택포인트는 항상 1,1,1 유지
                moneyStackPoint.DOKill();
                moneyStackPoint.DOLocalMove(targetPos, 0.25f)
                    .SetEase(Ease.OutQuad)
                    .SetLink(moneyStackPoint.gameObject);
            }
        }

        public void AddToStack(GameObject gemstoneGO)
        {
            if (stackList.Count >= maxCapacity)
            {
                if (Time.time >= nextMaxTextTime)
                {
                    nextMaxTextTime = Time.time + MAX_TEXT_COOLDOWN;
                    ShowMaxText();
                }
                
                // 스택이 꽉 찼다면 받아온 오브젝트 처리 (파괴 혹은 그냥 둠)
                // 여기서는 바위에서 이미 생성해서 넘겨준 것이므로, 못 받으면 파괴하는 것이 깔끔함
                Destroy(gemstoneGO);
                return;
            }

            Transform parent = stackList.Count == 0 ? stackPoint : stackList[stackList.Count - 1];
            Vector3 targetLocalPos = stackList.Count == 0 ? Vector3.zero : new Vector3(0, verticalSpacing, 0);

            Gemstone gemstone = gemstoneGO.GetComponent<Gemstone>();
            if (gemstone == null) gemstone = gemstoneGO.AddComponent<Gemstone>();

            gemstone.AttachToStack(parent, targetLocalPos, () => {
                stackList.Add(gemstone.transform);
                
                gemstone.transform.localScale = Vector3.zero;
                gemstone.transform.DOKill();
                gemstone.transform.DOScale(1.0f, stackScaleDuration)
                    .SetEase(Ease.OutBack)
                    .SetLink(gemstone.gameObject);

                // 젬스톤이 생겼으므로 현금 줄 위치 체크
                UpdateStackPositions();
                OnGemstoneAdded?.Invoke();
            });
        }

        private void ShowMaxText()
        {
            if (maxTextPrefab != null)
            {
                if (cachedCanvas == null)
                {
                    GameObject canvasObj = GameObject.FindWithTag("MainCanvas");
                    if (canvasObj != null) cachedCanvas = canvasObj.GetComponent<Canvas>();
                    
                    if (cachedCanvas == null) cachedCanvas = Object.FindFirstObjectByType<Canvas>();
                }
                if (cachedCanvas == null) return;

                // 캐릭터 위치에서 약간 위쪽 (너무 높으면 화면 밖으로 나갈 수 있음)
                Vector3 spawnWorldPos = transform.position + Vector3.up * 1.2f;
                GameObject go = ObjectPoolingManager.Instance.Spawn(maxTextPrefab, spawnWorldPos, Quaternion.identity, cachedCanvas.transform);
                
                FloatingText ft = go.GetComponent<FloatingText>();
                if (ft == null) ft = go.AddComponent<FloatingText>();
                
                ft.Setup(spawnWorldPos, "MAX", Color.red);
            }
        }

        public Transform RemoveFromStack()
        {
            if (stackList.Count == 0) return null;

            if (_audioSource != null && stackSound != null) _audioSource.PlayOneShot(stackSound);

            int lastIndex = stackList.Count - 1;
            Transform lastGem = stackList[lastIndex];
            stackList.RemoveAt(lastIndex);

            // 젬스톤이 모두 사라졌는지 체크하여 현금 줄 위치 조정
            UpdateStackPositions();
            OnGemstoneRemoved?.Invoke();
            
            return lastGem;
        }

        public void AddToFrontStack(Transform item)
        {
            if (_audioSource != null && stackSound != null) _audioSource.PlayOneShot(stackSound);

            Transform parent = handcuffsStack.Count == 0 ? frontStackPoint : handcuffsStack[handcuffsStack.Count - 1];
            Vector3 targetLocalPos = handcuffsStack.Count == 0 ? Vector3.zero : new Vector3(0, frontVerticalSpacing, 0);

            item.SetParent(parent);
            item.DOKill();
            item.DOLocalJump(targetLocalPos, 1.5f, 1, stackJumpDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(item.gameObject);
            item.DOLocalRotate(Vector3.zero, stackJumpDuration)
                .SetLink(item.gameObject);
            
            item.localScale = Vector3.zero;
            item.DOScale(1f, stackScaleDuration)
                .SetEase(Ease.OutBack)
                .SetLink(item.gameObject);

            handcuffsStack.Add(item);
            OnHandcuffAdded?.Invoke();
        }

        public Transform RemoveFromFrontStack()
        {
            if (handcuffsStack.Count == 0) return null;

            if (_audioSource != null && stackSound != null) _audioSource.PlayOneShot(stackSound);

            int lastIndex = handcuffsStack.Count - 1;
            Transform item = handcuffsStack[lastIndex];
            handcuffsStack.RemoveAt(lastIndex);
            OnHandcuffRemoved?.Invoke();
            
            return item;
        }

        public void AddToMoneyStack(Transform money)
        {
            if (_audioSource != null && stackSound != null) _audioSource.PlayOneShot(stackSound);

            // 수집되기 전의 월드 스케일을 기억 (보이는 크기 고정)
            Vector3 worldScaleBefore = money.lossyScale;

            // 돈은 젬스톤과 별개로 moneyStackPoint에서 쌓임
            bool isFirst = moneyStack.Count == 0;
            Transform parent = isFirst ? moneyStackPoint : moneyStack[moneyStack.Count - 1];
            Vector3 targetLocalPos = isFirst ? Vector3.zero : new Vector3(0, moneyVerticalSpacing, 0);

            money.SetParent(parent);
            
            // 부모의 월드 스케일에 상관없이 이전의 월드 스케일을 유지하도록 로컬 스케일 역산
            // lossyScale = parent.lossyScale * localScale => localScale = lossyScale / parent.lossyScale
            Vector3 parentLossy = parent.lossyScale;
            Vector3 targetLocalScale = new Vector3(
                worldScaleBefore.x / parentLossy.x,
                worldScaleBefore.y / parentLossy.y,
                worldScaleBefore.z / parentLossy.z
            );

            // 첫 번째 아이템이 기준 크기를 잡으면, 이후 자식(isFirst=false)들은 스택 누적을 막기 위해 (1,1,1) 유지
            // (이미 부모인 첫 번째 아이템이 보정된 크기를 가지고 있으므로)
            if (!isFirst) targetLocalScale = Vector3.one;

            money.DOKill();
            money.DOLocalJump(targetLocalPos, 2f, 1, stackJumpDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(money.gameObject);
            money.DOLocalRotate(Vector3.zero, stackJumpDuration)
                .SetLink(money.gameObject);
            
            moneyStack.Add(money);
            OnMoneyAdded?.Invoke();
            OnMoneyStackChanged?.Invoke();

            // 스케일 연출
            money.localScale = Vector3.zero;
            money.DOScale(targetLocalScale, stackScaleDuration)
                .SetEase(Ease.OutBack)
                .SetLink(money.gameObject);
        }

        public Transform RemoveFromMoneyStack()
        {
            if (moneyStack.Count == 0) return null;

            if (_audioSource != null && stackSound != null) _audioSource.PlayOneShot(stackSound);

            int lastIndex = moneyStack.Count - 1;
            Transform item = moneyStack[lastIndex];
            moneyStack.RemoveAt(lastIndex);
            OnMoneyStackChanged?.Invoke();
            
            return item;
        }

        public bool HasHandcuffs() => handcuffsStack.Count > 0;

        /// <summary>
        /// 최대 적재량을 늘립니다.
        /// </summary>
        public void IncreaseCapacity(int amount)
        {
            maxCapacity += amount;
        }
    }
}
