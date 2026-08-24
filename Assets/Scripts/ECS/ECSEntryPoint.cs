using UnityEngine;
using DOD_ECS.Components;
using DOD_ECS.Systems;

namespace DOD_ECS
{
    // Unity의 라이프사이클(MonoBehaviour)과 순수 C# 기반 ECS를 연결하는 진입점
    public class ECSEntryPoint : MonoBehaviour
    {
        private MovementDataSoA _movementData;
        private MovementSystem _movementSystem;

        private void Start()
        {
            // 1. SoA 데이터 컨테이너 초기화 (충분한 메모리 미리 할당)
            _movementData = new MovementDataSoA(10000);

            // 2. 시스템 초기화
            _movementSystem = new MovementSystem();

            // 3. 테스트용 엔티티 데이터 생성
            for (int i = 0; i < 5000; i++)
            {
                _movementData.Add(
                    new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)),
                    new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f))
                );
            }

            Debug.Log($"[DOD ECS] Initialized {_movementData.Count} entities using SoA.");
        }

        private void Update()
        {
            // 삭제 테스트: 스페이스바를 누르면 0번 인덱스의 엔티티(데이터) 안전 삭제
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_movementData.Count > 0)
                {
                    Debug.Log($"[DOD ECS] 맨 앞 엔티티 삭제 처리(Swap and Pop). 남은 수: {_movementData.Count - 1}");
                    _movementData.RemoveAt(0);
                }
            }

            // 매 프레임마다 System에 데이터를 주입하여 로직 구동
            _movementSystem.Update(_movementData, Time.deltaTime);
        }
    }
}

