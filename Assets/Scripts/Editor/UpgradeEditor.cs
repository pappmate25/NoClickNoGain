#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Upgrade))]
public class UpgradeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        Upgrade upgrade = (Upgrade)target;

        // Create arrays for our equation information
        string[] equationNames = { "Base Value Equation", "Cost Equation", "Effect Equation" };
        string[] equationValues = { upgrade.BaseValueEquation, upgrade.CostEquation, upgrade.EffectEquation };
        bool[] needsParsing = { upgrade.BaseValueEquationDirty(), upgrade.CostEquationDirty(), upgrade.EffectEquationDirty() };

        bool anyEquationEmpty = string.IsNullOrEmpty(upgrade.BaseValueEquation)
                                    || string.IsNullOrEmpty(upgrade.CostEquation)
                                    || string.IsNullOrEmpty(upgrade.EffectEquation);

        bool anyEquationNeedsParsing = upgrade.BaseValueEquationDirty()
                                    || upgrade.CostEquationDirty()
                                    || upgrade.EffectEquationDirty();

        if (anyEquationEmpty)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("One or more of the equations are empty. Please fill them in.", MessageType.Error);
        }
        else if (anyEquationNeedsParsing)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("One or more of the equations have been modified. Click 'Parse Equations' to update.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        if (anyEquationEmpty)
        {
            GUI.backgroundColor = Color.red;
            GUI.enabled = false; // Disable the button if any equation is empty
        }
        else if (anyEquationNeedsParsing)
        {
            GUI.backgroundColor =  Color.yellow;
        }

        if (GUILayout.Button("Parse Equations", GUILayout.Height(30)))
        {
            Undo.RecordObject(upgrade, "Parse Equations");
            upgrade.ParseEquations();
            EditorUtility.SetDirty(upgrade);
        }
        GUI.backgroundColor = Color.white;

        // Display status information
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Equation Status", EditorStyles.boldLabel);

        GUI.enabled = false; // Make the following fields read-only

        // Store original background color
        Color originalColor = GUI.backgroundColor;

        // Loop through all equations and display their status
        for (int i = 0; i < equationNames.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(equationNames[i]);

            if (string.IsNullOrEmpty(equationValues[i]))
            {
                GUI.backgroundColor = Color.red;
                EditorGUILayout.LabelField("Not defined", EditorStyles.miniButtonMid);
            }
            else if (needsParsing[i])
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.LabelField("Modified - Needs parsing", EditorStyles.miniButtonMid);
            }
            else
            {
                GUI.backgroundColor = originalColor;
                EditorGUILayout.LabelField("Up to date", EditorStyles.miniButtonMid);
            }

            // Reset background color
            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndHorizontal();
        }

        GUI.enabled = true;
    }
}
#endif