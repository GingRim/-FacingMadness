using System;

/// <summary>
/// 기존 FieldEventPageData 에셋의 GUID와 참조를 보존하는 호환 형식입니다.
/// 선택지, 설명, 조건과 다음 연결은 모두 FieldEventData에서 관리합니다.
/// 새 에셋은 FieldEventData로만 생성합니다.
/// </summary>
[Obsolete("새 이벤트는 FieldEventData 하나로 구성하십시오.")]
public class FieldEventPageData : FieldEventData
{
}
