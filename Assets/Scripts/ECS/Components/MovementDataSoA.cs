using UnityEngine;
using System.Collections.Generic;

namespace DOD_ECS.Components
{
    // SoA (Structure of Arrays) 방식의 데이터 컨테이너
    public class MovementDataSoA
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; }

        // [추가됨] Entity ID를 통해 현재 배열 인덱스를 찾는 매핑 딕셔너리
        public Dictionary<int, int> EntityToIndexMap;

        // [추가됨] 배열 인덱스를 통해 역으로 Entity ID를 찾는 SoA 배열 (Swap 시 필요)
        public int[] EntityIds;
        
        public Vector3[] Positions;
        public Vector3[] Velocities;

        public MovementDataSoA(int capacity)
        {
            Capacity = capacity;
            Count = 0;
            
            EntityToIndexMap = new Dictionary<int, int>(capacity);
            EntityIds = new int[capacity];
            Positions = new Vector3[capacity];
            Velocities = new Vector3[capacity];
        }

        // 새로운 데이터(Entity) 추가 - 이제 Entity 정보를 직접 받습니다.
        public void Add(Entity entity, Vector3 position, Vector3 velocity)
        {
            if (Count >= Capacity)
            {
                Resize(Capacity * 2);
            }

            int index = Count;
            Positions[index] = position;
            Velocities[index] = velocity;
            EntityIds[index] = entity.Id; // 역매핑 데이터 저장
            
            // 딕셔너리에 매핑 추가: Entity ID -> 인덱스
            EntityToIndexMap[entity.Id] = index;
            
            Count++;
        }

        // 엔티티를 안전하게 삭제 (Swap and Pop 방식 + 매핑 갱신)
        public void Remove(Entity entity)
        {
            // 딕셔너리에서 엔티티가 몇 번 인덱스에 있는지 O(1)로 찾음
            if (!EntityToIndexMap.TryGetValue(entity.Id, out int index)) return;

            int lastIndex = Count - 1;

            // 지우려는 데이터가 맨 마지막 데이터가 아니라면(Swap이 필요하다면)
            if (index != lastIndex)
            {
                Positions[index] = Positions[lastIndex];
                Velocities[index] = Velocities[lastIndex];
                
                // 역방향 매핑 배열에서, 옮겨진 마지막 데이터의 Entity ID를 가져옴
                int lastEntityId = EntityIds[lastIndex];
                EntityIds[index] = lastEntityId; // 식별자 정보도 이동시킴

                // 딕셔너리 매핑 갱신: 마지막 데이터였던 엔티티가 새로운 인덱스(원래 지워진 자리)를 가리키도록 업데이트
                EntityToIndexMap[lastEntityId] = index;
            }

            // 배열에서 삭제된 엔티티의 매핑 정보를 완전히 제거
            EntityToIndexMap.Remove(entity.Id);
            Count--;
        }

        private void Resize(int newCapacity)
        {
            System.Array.Resize(ref EntityIds, newCapacity);
            System.Array.Resize(ref Positions, newCapacity);
            System.Array.Resize(ref Velocities, newCapacity);
            Capacity = newCapacity;
        }
    }
}
