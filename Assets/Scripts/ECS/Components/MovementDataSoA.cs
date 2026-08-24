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

