using UnityEngine;
using UnityEngine.InputSystem; // New Input System 사용을 위해 추가
using Unity.Jobs;
using System.Collections.Generic;
using DOD_ECS.Components;
using DOD_ECS.Systems;

namespace DOD_ECS
{
    public class ECSEntryPoint : MonoBehaviour
    {
        private MovementDataSoA _movementData;
        private MovementSystem _movementSystem;
        private JobHandle _movementJobHandle;

        // 실제 유니티 Transform과 동기화하기 위한 딕셔너리 (Entity ID -> Transform)
        private Dictionary<int, Transform> _entityToTransform = new Dictionary<int, Transform>();
        private List<Entity> _activeEntities = new List<Entity>();

        private int _nextEntityId = 1;

        private void Start()
        {
            _movementData = new MovementDataSoA(10000);
            _movementSystem = new MovementSystem();

            // 1. 베이킹(Baking) 과정: 씬에 있는 모든 Authoring 오브젝트를 수집하여 순수 데이터(SoA)로 변환
            BakeSceneObjects();
        }

        private void BakeSceneObjects()
        {
            // 씬에 존재하는 모든 EntityAuthoring 컴포넌트를 찾습니다.
            EntityAuthoring[] authoringObjects = FindObjectsOfType<EntityAuthoring>();

            foreach (var auth in authoringObjects)
            {
                if (auth.IsBaked) continue;

                // 새로운 ECS Entity 발급
                Entity newEntity = new Entity(_nextEntityId++);
                auth.BakedEntity = newEntity;
                auth.IsBaked = true;

                _activeEntities.Add(newEntity);

                // 유니티 렌더링 동기화를 위해 Transform 저장
                _entityToTransform[newEntity.Id] = auth.transform;

                // 순수 데이터 공간(SoA)에 GameObject의 Transform 정보 및 Authoring 정보를 베이킹(복사)
                _movementData.Add(newEntity, auth.transform.position, auth.initialVelocity);
            }

            Debug.Log($"[DOD ECS] {authoringObjects.Length}개의 유니티 오브젝트를 ECS 엔티티로 베이킹 완료.");
        }

        private void Update()
        {
            // 구버전 Input 대신 New Input System의 Keyboard를 사용하여 입력을 감지합니다.
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (_activeEntities.Count > 0)
                {
                    Entity entityToRemove = _activeEntities[0];
                    _activeEntities.RemoveAt(0);

                    // 1. 순수 ECS 데이터 삭제 (Swap and Pop)
                    _movementData.Remove(entityToRemove);

                    // 2. 실제 유니티 GameObject 파괴
                    if (_entityToTransform.TryGetValue(entityToRemove.Id, out Transform t))
                    {
                        Destroy(t.gameObject);
                        _entityToTransform.Remove(entityToRemove.Id);
                        Debug.Log($"[DOD ECS] 유니티 오브젝트 및 ECS 엔티티 동시 삭제 완료 (ID: {entityToRemove.Id})");
                    }
                }
            }

            // Job 예약 (백그라운드 계산 시작)
            _movementJobHandle = _movementSystem.ScheduleJob(_movementData, Time.deltaTime);
        }

        private void LateUpdate()
        {
            // Job 완료 대기
            _movementJobHandle.Complete();

            // --- [동기화 과정] ---
            // 워커 스레드(Job)가 계산해놓은 순수 NativeArray 위치 데이터를 
            // 실제 눈에 보이는 유니티 GameObject의 Transform에 적용(Sync)합니다.
            // 메인 스레드에서 수행해야 하므로 Job 완료 이후인 LateUpdate에서 진행합니다.
            int count = _movementData.Count;
            for (int i = 0; i < count; i++)
            {
                int entityId = _movementData.EntityIds[i];
                if (_entityToTransform.TryGetValue(entityId, out Transform t))
                {
                    // SoA 데이터를 GameObject Transform에 복사
                    t.position = _movementData.Positions[i];
                }
            }
        }

        private void OnDestroy()
        {
            _movementJobHandle.Complete();
            _movementData?.Dispose();
        }
    }
}
