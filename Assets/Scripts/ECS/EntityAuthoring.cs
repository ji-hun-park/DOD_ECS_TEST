using UnityEngine;

namespace DOD_ECS
{
    // 유니티의 인스펙터(GameObject)에서 값을 세팅할 수 있는 일반 MonoBehaviour 스크립트입니다.
    // ECS 세계로 넘어가기 전, 유니티 데이터를 들고 있는 '저작(Authoring)' 역할을 수행합니다.
    public class EntityAuthoring : MonoBehaviour
    {
        [Tooltip("오브젝트의 초기 이동 방향과 속도")]
        public Vector3 initialVelocity = new Vector3(1f, 0f, 1f);

        // 베이킹(변환)이 완료된 후 발급된 순수 ECS Entity 식별자를 보관합니다.
        public Entity BakedEntity { get; set; }

        // 이미 베이킹 되었는지 여부
        public bool IsBaked { get; set; } = false;
    }
}

