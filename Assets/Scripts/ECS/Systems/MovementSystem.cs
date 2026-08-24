using UnityEngine;
using DOD_ECS.Components;

namespace DOD_ECS.Systems
{
    public class MovementSystem
    {
        // System은 데이터(SoA)만 넘겨받아 순회하며 일괄 처리합니다.
        public void Update(MovementDataSoA movementData, float deltaTime)
        {
            int count = movementData.Count;

            // 데이터 지역성을 활용한 처리 (메모리가 연속되어 있어 매우 빠름)
            // Swap and Pop 삭제 로직 덕분에 배열의 0 ~ Count-1 구간은 항상 유효(Alive)함이 보장됩니다.
            // 따라서 if (IsAlive) 분기문을 아예 제거할 수 있으며, 
            // 이는 CPU의 분기 예측(Branch Prediction) 미스를 없애 성능(SIMD 최적화 등)을 극대화합니다.
            for (int i = 0; i < count; i++)
            {
                movementData.Positions[i] += movementData.Velocities[i] * deltaTime;
            }
        }
    }
}

