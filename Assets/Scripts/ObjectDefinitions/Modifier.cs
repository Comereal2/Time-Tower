using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "Object/Modifier")]
public class Modifier : ScriptableObject
{
    public string modifiedVariable = "timeLeft";
    public float modifierValue = 20f;
    public enum ModifierType
    {
        Add, Subtract, Multiply, Divide
    }
    public ModifierType modifierType;
    public string modifiedVariableVisibleDescription = "";

    private static readonly Dictionary<string, string> variableDescriptionMap = new Dictionary<string, string>
    {
        { "timeLeft", "Time" },
        { "speed", "Movement Speed" }
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
            case ModifierType.Divide:
                modifiedVariableVisibleDescription += "/";
                break;
        }
        modifiedVariableVisibleDescription += modifierValue.ToString();
    }
}