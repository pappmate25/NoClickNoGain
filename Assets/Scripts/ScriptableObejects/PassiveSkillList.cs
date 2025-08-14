using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSkillList", menuName = "SO/Configuration/PassiveSkillList")]
public class PassiveSkillList : ScriptableObject
{
    public PassiveSkill[] PassiveSkills;
}
