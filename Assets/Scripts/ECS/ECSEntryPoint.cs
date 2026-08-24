using UnityEngine;
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

        // 생성된 엔티티들을 기억하여 삭제 테스트에 사용하기 위한 리스트
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

                // 엔티티 객체 자체를 Add의 매개변수로 전달
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
            // 삭제 테스트: 스페이스바를 누르면 특정 엔티티를 찾아서 O(1)에 삭제
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_activeEntities.Count > 0)
                {
                    // 가장 앞단에 추가했던 엔티티를 하나 꺼내옴
                    Entity entityToRemove = _activeEntities[0];
                    _activeEntities.RemoveAt(0);

                    Debug.Log($"[DOD ECS] 엔티티(ID: {entityToRemove.Id}) 삭제 요청. 남은 수: {_movementData.Count - 1}");

                    // 인덱스가 아닌 엔티티를 통째로 넘겨서 삭제
                    _movementData.Remove(entityToRemove);
                }
            }

            // 매 프레임마다 System에 데이터를 주입하여 로직 구동
            _movementSystem.Update(_movementData, Time.deltaTime);
        }

        private void OnDestroy()
        {
            // Unity의 Native 메모리는 C# 가비지 컬렉터(GC)가 지워주지 않으므로, 
            // 직접 Dispose()를 호출하여 메모리 누수를 막아야 합니다.
            _movementData?.Dispose();
        }
    }
}
