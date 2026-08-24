using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using DOD_ECS.Components;

namespace DOD_ECS.Systems
{
    // [BurstCompile] 속성을 달아주면 LLVM 기반 컴파일러가 이 구조체를 기계어로 변환하며, 
    // 이때 CPU의 SIMD(Single Instruction Multiple Data) 명령어를 사용하여 벡터화 연산을 최적화합니다.
    [BurstCompile(CompileSynchronously = true)]
    public struct MovementJob : IJobParallelFor
    {
        public float DeltaTime;

        // [ReadOnly]를 붙이면 여러 스레드가 동시에 읽어도 안전함을 보장하여 성능이 오릅니다.
        [ReadOnly]
        public NativeArray<Vector3> Velocities;

        // 읽기/쓰기가 동시에 일어나는 배열
        public NativeArray<Vector3> Positions;

        // 멀티코어 환경에서 워커 스레드들에 의해 병렬로 호출되는 부분
        public void Execute(int index)
        {
            // Vector3 연산이 Burst에 의해 SIMD로 자동 최적화됩니다.
            Positions[index] += Velocities[index] * DeltaTime;
        }
    }

    public class MovementSystem
    {
        public void Update(MovementDataSoA movementData, float deltaTime)
        {
            if (movementData.Count == 0) return;

            // 1. Job 구조체에 Native 데이터 연결
            MovementJob job = new MovementJob
            {
                DeltaTime = deltaTime,
                Velocities = movementData.Velocities,
                Positions = movementData.Positions
            };

            // 2. 멀티스레드 스케줄링 (총 반복 횟수: Count, 한 스레드가 처리할 묶음(배치) 크기: 64)
            JobHandle handle = job.Schedule(movementData.Count, 64);

            // 3. 작업이 끝날 때까지 메인 스레드 대기 
            // (실제 실무에서는 Update 초반에 Schedule 하고 LateUpdate에서 Complete 하여 대기 시간을 최소화합니다)
            handle.Complete();
        }
    }
}
