using UnityEngine;
using Unity.Jobs; // JobHandle을 사용하기 위해 추가
using System.Collections.Generic;
using DOD_ECS.Components;
using DOD_ECS.Systems;

namespace DOD_ECS
{
    // Unity의 라이프사이클(MonoBehaviour)과 순수 C# 기반 ECS를 연결하는 진입점
    public class ECSEntryPoint : MonoBehaviour
    {
        private MovementDataSoA _movementData;
        private MovementSystem _movementSystem;
        
        // 현재 실행 중인 멀티스레드 작업(Job)의 상태를 추적하기 위한 핸들
        private JobHandle _movementJobHandle;
        
        private List<Entity> _activeEntities = new List<Entity>();
        private int _nextEntityId = 1;

        private void Start()
        {
            _movementData = new MovementDataSoA(10000);
            _movementSystem = new MovementSystem();

            // 테스트용 엔티티 생성
            for (int i = 0; i < 5000; i++)
            {
                Entity newEntity = new Entity(_nextEntityId++);
                _activeEntities.Add(newEntity);

                _movementData.Add(
                    newEntity,
                    new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)),
                    new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f))
                );
            }
            
            Debug.Log($"[DOD ECS] Initialized {_movementData.Count} entities using SoA.");
        }

        private void Update()
        {
            // 주의: Update가 호출되는 시점은 이전 프레임의 LateUpdate를 거친 후이므로, 
            // Job은 이미 100% 완료(Complete)된 상태입니다. 
            // 따라서 이곳에서 NativeArray의 길이를 바꾸거나(추가/삭제) 데이터를 읽고 쓰는 것은 메모리 충돌 없이 안전합니다.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_activeEntities.Count > 0)
                {
                    Entity entityToRemove = _activeEntities[0];
                    _activeEntities.RemoveAt(0);

                    Debug.Log($"[DOD ECS] 엔티티(ID: {entityToRemove.Id}) 삭제 요청. 남은 수: {_movementData.Count - 1}");
                    _movementData.Remove(entityToRemove);
                }
            }

            // System에 데이터를 주입하고 워커 스레드에 "예약(Schedule)"만 걸어둡니다.
            // CPU 워커 스레드들이 백그라운드에서 병렬 연산을 시작하며, 
            // 메인 스레드는 작업을 기다리지 않고 즉시 아래로 빠져나가 다른 로직을 처리할 수 있습니다.
            _movementJobHandle = _movementSystem.ScheduleJob(_movementData, Time.deltaTime);
        }

        private void LateUpdate()
        {
            // 렌더링 직전 등 프레임의 마지막 시점에 Job이 끝날 때까지 대기합니다.
            // 메인 스레드가 Update에서 다른 게임 로직을 처리하는 동안 
            // 워커 스레드가 이미 연산을 끝마쳤다면 메인 스레드는 아무런 대기 시간 없이 즉시 통과하게 됩니다. (프레임 이득 극대화)
            _movementJobHandle.Complete();
        }

        private void OnDestroy()
        {
            // 안전장치: 컴포넌트가 파괴될 때 혹시라도 백그라운드에서 돌고 있는 Job이 있다면 
            // 강제로 완료(Complete)시킨 뒤에 NativeArray 메모리를 해제(Dispose)해야 메모리 엑세스 충돌(Crash)이 발생하지 않습니다.
            _movementJobHandle.Complete();
            _movementData?.Dispose();
        }
    }
}
