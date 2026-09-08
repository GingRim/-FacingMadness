using UnityEngine;

public class CharacterFactory : MonoBehaviour
{

    [Header("프리팹")]
    [SerializeField] private GameObject characterPrefab;

    /// <summary>
    /// 플레이어 캐릭터 생성
    /// </summary>
    public CharacterBase CreatePlayerCharacter(CharacterBuildData data, Vector3 position)
    {

        if (data == null)
        {
            Debug.LogError("CharacterBuildData가 없습니다.");
            return null;
        }

        if (characterPrefab == null)
        {
            Debug.LogError("Character Prefab이 없습니다.");
            return null;
        }

        Debug.Log("CreatePlayerCharacter 호출됨");

        GameObject characterObject = ObjectManager.CreateObject(characterPrefab, position);



        if (characterObject == null)
        {
            Debug.LogError("캐릭터 오브젝트 생성 실패");
            return null;
        }

        Debug.Log($"생성된 캐릭터 오브젝트: {characterObject.name}");

        CharacterBase character = characterObject.GetComponent<CharacterBase>();

        if (character == null)
        {
            Debug.LogError("생성된 오브젝트에 CharacterBase가 없습니다.");
            return null;
        }

        character.SetDisplayName(data.characterName);
        character.SetIcon(data.Icon);

        // 캐릭터 데이터를 적용하기 전에 모든 모듈을 먼저 등록한다.
        character.AddAllModuleFromObject(character.gameObject);

        // 플레이어 컨트롤러도 같은 캐릭터를 소유하도록 연결한다.
        AttachPlayerController(character);
        ApplyBuildData(character, data);

        return character;
    }

    /// <summary>
    /// 캐릭터에 생성 데이터를 적용한다.
    /// </summary>
    private void ApplyBuildData(CharacterBase character, CharacterBuildData data)
    {
        ApplyLevel(character, data);
        ApplyStats(character, data);
        ApplyDeck(character, data);

        ApplyCost(character);
        RefreshHP(character);
        RefreshSanity(character);
    }

    /// <summary>
    /// LV 적용
    /// </summary>
    /// <param name="character"></param>
    /// <param name="data"></param>
    private void ApplyLevel(CharacterBase character, CharacterBuildData data)
    {
        LVModules lv = character.GetModule<LVModules>();

        if (lv == null)
            return;

        lv.SetLevel(data.level);
    }

    /// <summary>
    /// 능력치 적용
    /// </summary>
    private void ApplyStats(CharacterBase character, CharacterBuildData data)
    {
        StatModules stat = character.GetModule<StatModules>();
        DerivedStatModule derived = character.GetModule<DerivedStatModule>();

        if (stat == null)
        {
            Debug.LogWarning("StatModules가 없습니다.");
            return;
        }

        stat.SetStat(StatType.Strength, data.strength);

        stat.SetStat(StatType.Agility, data.agility);

        stat.SetStat(StatType.Health, data.health);

        stat.SetStat(StatType.Intelligence, data.intelligence);

        stat.SetStat(StatType.Will, data.will);

        stat.SetDesignatedStatType(data.designatedStatType);
    }

    /// <summary>
    /// 기본 덱 적용
    /// </summary>
    private void ApplyDeck(CharacterBase character, CharacterBuildData data)
    {
        if (data.startDeck == null)
            return;

        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogWarning("DeckModule이 없습니다.");
            return;
        }

        deck.RegisterDeck(data.startDeck);
    }

    /// <summary>
    /// 코스트 적용
    /// </summary>
    /// <param name="character"></param>
    private void ApplyCost(CharacterBase character)
    {
        CostModule cost = character.GetModule<CostModule>();
        DerivedStatModule derived = character.GetModule<DerivedStatModule>();

        if (cost == null || derived == null)
            return;

        cost.InitializeCost(
            derived.GetMaxActionCost(),
            derived.GetMaxAuxiliaryCost(),
            derived.GetMaxReactionCost()
        );

    }

    /// <summary>
    /// 능력치 기반 최대 체력 재설정
    /// </summary>
    private void RefreshHP(CharacterBase character)
    {
        HitpointModules hp = character.GetModule<HitpointModules>();
        DerivedStatModule derived = character.GetModule<DerivedStatModule>();

        if (hp == null)
        {
            Debug.LogError("HitpointModules 없음");
            return;
        }

        if (derived == null)
        {
            Debug.LogError("DerivedStatModule 없음");
            return;
        }

        int maxHP = derived.GetMaxHP();

        hp.InitializeHP(maxHP);

        Debug.Log($"HP 초기화 후: {hp.Current} / {hp.Max}");
    }

    /// <summary>
    /// 능력치 기반 최대 정신력 재설정
    /// </summary>
    private void RefreshSanity(CharacterBase character)
    {
        if (character == null)
            return;

        DerivedStatModule derived = character.GetModule<DerivedStatModule>();

        SanityModule sanity = character.GetModule<SanityModule>();

        if (derived == null || sanity == null)
            return;

        sanity.SetMaxSanity(derived.GetMaxSanity());
        sanity.FillSanity();
    }

    /// <summary>
    /// 플레이어 컨트롤러 연결
    /// </summary>
    private void AttachPlayerController(CharacterBase character)
    {

        PlauerController controller =
        character.GetComponent<PlauerController>();

        if (controller != null)
        {
            controller.Possess(character);
        }
    }

}
