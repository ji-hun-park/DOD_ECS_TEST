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
            // 컴파일러의 SIMD 최적화(벡터화)에도 매우 유리한 구조입니다.
            for (int i = 0; i < count; i++)
            {
                if (movementData.IsAlive[i])
                {
                    movementData.Positions[i] += movementData.Velocities[i] * deltaTime;
                }
            }
        }
    }
}

