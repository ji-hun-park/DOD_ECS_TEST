using UnityEngine;

namespace DOD_ECS.Components
{
    // SoA (Structure of Arrays) 방식의 데이터 컨테이너
    // 기존의 Array of Structures(AoS) 방식과 달리 각 필드를 별도의 배열로 관리합니다.
    public class MovementDataSoA
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; }

        // 데이터를 각각 연속된 메모리 배열로 저장하여 캐시 히트율(Cache Hit Rate)을 극대화
        public Vector3[] Positions;
        public Vector3[] Velocities;
        public bool[] IsAlive;

        public MovementDataSoA(int capacity)
        {
            Capacity = capacity;
            Count = 0;

            Positions = new Vector3[capacity];
            Velocities = new Vector3[capacity];
            IsAlive = new bool[capacity];
        }

        // 새로운 데이터(Entity) 추가
        public int Add(Vector3 position, Vector3 velocity)
        {
            if (Count >= Capacity)
            {
                Resize(Capacity * 2);
            }

            int index = Count;
            Positions[index] = position;
            Velocities[index] = velocity;
            IsAlive[index] = true;

            Count++;
            return index; // 부여된 인덱스가 곧 Entity와 연결되는 데이터 매핑 인덱스입니다.
        }

        // 데이터 안전 삭제 (Swap and Pop 방식)
        // 배열 중간의 데이터를 지울 때 뒤의 데이터를 한 칸씩 당기면 O(N)의 오버헤드가 발생합니다.
        // 이를 방지하기 위해 맨 마지막 데이터를 지워진 자리로 옮겨서(덮어쓰기) O(1)에 처리합니다.
        // 메모리 파편화(Fragmentation)가 발생하지 않아 연속된 메모리가 보장되고 페이지 폴트를 방지합니다.
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count) return;

            int lastIndex = Count - 1;

            // 지우려는 데이터가 맨 마지막 데이터가 아니라면, 마지막 데이터를 빈자리로 가져옴(Swap)
            if (index != lastIndex)
            {
                Positions[index] = Positions[lastIndex];
                Velocities[index] = Velocities[lastIndex];
                IsAlive[index] = IsAlive[lastIndex];

                // (참고) 만약 외부에서 Entity ID로 인덱스를 찾고 있다면, 여기서 인덱스 매핑을 갱신해 주어야 합니다.
            }

            // 맨 끝 데이터는 논리적으로 삭제됨 (Pop)
            IsAlive[lastIndex] = false;
            Count--;
        }

        // 배열 꽉 찼을 때 크기 확장
        private void Resize(int newCapacity)
        {
            System.Array.Resize(ref Positions, newCapacity);
            System.Array.Resize(ref Velocities, newCapacity);
            System.Array.Resize(ref IsAlive, newCapacity);
            Capacity = newCapacity;
        }
    }
}

