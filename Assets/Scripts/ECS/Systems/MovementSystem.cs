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
        // Update 대신 JobHandle을 반환하는 ScheduleJob으로 변경합니다.
        // dependency 매개변수를 추가하면 여러 시스템의 Job들을 꼬리물기(체이닝) 방식으로 연결할 수 있습니다.
        public JobHandle ScheduleJob(MovementDataSoA movementData, float deltaTime, JobHandle dependency = default)
        {
            if (movementData.Count == 0) return dependency;

            // 1. Job 구조체에 Native 데이터 연결
            MovementJob job = new MovementJob
            {
                DeltaTime = deltaTime,
                Velocities = movementData.Velocities,
                Positions = movementData.Positions
            };

            // 2. 멀티스레드 스케줄링 후 핸들(JobHandle)을 반환 (메인 스레드는 즉시 다음 코드로 넘어감)
            return job.Schedule(movementData.Count, 64, dependency);
        }
    }
}
