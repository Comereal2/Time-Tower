using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "Object/Modifier")]
public class Modifier : ScriptableObject
{
    public string modifiedVariable = "timeLeft";
    public float modifierValue = 20f;
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
        modifiedVariableVisibleDescription += modifierValue > 0 ? "+" : "-";
        modifiedVariableVisibleDescription += ((int)modifierValue).ToString();
    }
}