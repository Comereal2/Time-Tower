using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "Object/Modifier")]
public class Modifier : ScriptableObject
{
    public string modifiedVariable = "timeLeft";
    public float modifierValue = 20f;
    public enum ModifierType
    {
        Add, Subtract, Multiply
    }
    public ModifierType modifierType;
    public string modifiedVariableVisibleDescription = "";

    private static readonly Dictionary<string, string> variableDescriptionMap = new()
    {
        { "timeLeft", "Time" },
        { "timeConsumeSpeed", "Time Consume" },
        { "speed", "Movement Speed" },
        { "scoreFromCoins", "Coin Value" },
        { "bonusTimeFromCoins", "Extra Time From Coins" },
        { "bulletSpeed", "Bullet Speed" },
        { "bulletDamage", "Attack Damage" },
        { "numberOfAttacks", "Number of Attacks" },
        { "bulletArch", "Bullet Arch" },
        { "weirdBullets", "Weird Bullets" },
        { "damageResistance", "Damage Resistance" },
        { "bouncyBullets", "Bouncy Bullets" },
        { "bulletDespawnTime", "Bullet Lifespan" },
        { "costModifier", "Cost Modifier" }
    };

    private List<string> boolVariableList = new(){
        "weirdBullets", "bouncyBullets"
    };

    private void Awake()
    {
        if (variableDescriptionMap.TryGetValue(modifiedVariable, out string description))
        {
            modifiedVariableVisibleDescription = description;
        }
        else
        {
            modifiedVariableVisibleDescription = "Unknown Variable";
        }
        modifiedVariableVisibleDescription += ": ";
        if (boolVariableList.Contains(modifiedVariable))
        {
            if(modifierValue == 0)
            {
                modifiedVariableVisibleDescription += "Off";
            }
            else
            {
                modifiedVariableVisibleDescription += "On";
            }
            return;
        }
        switch (modifierType)
        {
            case ModifierType.Add:
                modifiedVariableVisibleDescription += "+";
                break;
            case ModifierType.Subtract:
                modifiedVariableVisibleDescription += "-";
                break;
            case ModifierType.Multiply:
                modifiedVariableVisibleDescription += "*";
                break;
        }
        modifiedVariableVisibleDescription += modifierValue.ToString();
    }
}