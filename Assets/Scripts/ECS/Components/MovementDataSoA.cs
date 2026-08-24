using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using System;

namespace DOD_ECS.Components
{
    // NativeArray를 사용하므로 IDisposable을 구현하여 메모리를 해제할 수 있어야 합니다.
    public class MovementDataSoA : IDisposable
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public Dictionary<int, int> EntityToIndexMap;

        // C# 관리형(Managed) 배열이 아닌, Job System과 Burst Compiler가 접근할 수 있는 비관리형(Native) 메모리로 변경합니다.
        public NativeArray<int> EntityIds;
        public NativeArray<Vector3> Positions;
        public NativeArray<Vector3> Velocities;

        public MovementDataSoA(int capacity)
        {
            Capacity = capacity;
            Count = 0;
            
            EntityToIndexMap = new Dictionary<int, int>(capacity);
            
            // Allocator.Persistent: 프로그램이 끝날 때까지 유지되는 메모리 타입 (직접 해제 필요)
            EntityIds = new NativeArray<int>(capacity, Allocator.Persistent);
            Positions = new NativeArray<Vector3>(capacity, Allocator.Persistent);
            Velocities = new NativeArray<Vector3>(capacity, Allocator.Persistent);
        }

        public void Add(Entity entity, Vector3 position, Vector3 velocity)
        {
            if (Count >= Capacity)
            {
                Resize(Capacity * 2);
            }

            int index = Count;
            Positions[index] = position;
            Velocities[index] = velocity;
            EntityIds[index] = entity.Id;
            
            EntityToIndexMap[entity.Id] = index;
            Count++;
        }

        public void Remove(Entity entity)
        {
            if (!EntityToIndexMap.TryGetValue(entity.Id, out int index)) return;

            int lastIndex = Count - 1;

            if (index != lastIndex)
            {
                Positions[index] = Positions[lastIndex];
                Velocities[index] = Velocities[lastIndex];
                
                int lastEntityId = EntityIds[lastIndex];
                EntityIds[index] = lastEntityId;
                EntityToIndexMap[lastEntityId] = index;
            }

            EntityToIndexMap.Remove(entity.Id);
            Count--;
        }

        private void Resize(int newCapacity)
        {
            // 새 Native 메모리 할당
            var newEntityIds = new NativeArray<int>(newCapacity, Allocator.Persistent);
            var newPositions = new NativeArray<Vector3>(newCapacity, Allocator.Persistent);
            var newVelocities = new NativeArray<Vector3>(newCapacity, Allocator.Persistent);

            // 기존 데이터 복사 (NativeArray.Copy는 내부적으로 Memcpy를 사용하여 C# 배열 복사보다 훨씬 빠릅니다)
            NativeArray<int>.Copy(EntityIds, newEntityIds, Count);
            NativeArray<Vector3>.Copy(Positions, newPositions, Count);
            NativeArray<Vector3>.Copy(Velocities, newVelocities, Count);

            // 기존 메모리 해제
            EntityIds.Dispose();
            Positions.Dispose();
            Velocities.Dispose();

            // 참조 교체
            EntityIds = newEntityIds;
            Positions = newPositions;
            Velocities = newVelocities;
            
            Capacity = newCapacity;
        }

        // 메모리 해제 로직 (Job System 사용 시 필수)
        public void Dispose()
        {
            if (EntityIds.IsCreated) EntityIds.Dispose();
            if (Positions.IsCreated) Positions.Dispose();
            if (Velocities.IsCreated) Velocities.Dispose();
        }
    }
}
