using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestUpgrade))]
public class TestUpgradeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        TestUpgrade testUpgrade = (TestUpgrade)target;

        // Create arrays for our equation information
        string[] equationNames = { "Base Value Equation", "Cost Equation", "Effect Equation" };
        string[] equationValues = { testUpgrade.BaseValueEquation, testUpgrade.CostEquation, testUpgrade.EffectEquation };
        bool[] needsParsing = { testUpgrade.BaseValueEquationDirty(), testUpgrade.CostEquationDirty(), testUpgrade.EffectEquationDirty() };

        bool anyEquationEmpty = string.IsNullOrEmpty(testUpgrade.BaseValueEquation)
                                    || string.IsNullOrEmpty(testUpgrade.CostEquation)
                                    || string.IsNullOrEmpty(testUpgrade.EffectEquation);

        bool anyEquationNeedsParsing = testUpgrade.BaseValueEquationDirty()
                                    || testUpgrade.CostEquationDirty()
                                    || testUpgrade.EffectEquationDirty();

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
            Undo.RecordObject(testUpgrade, "Parse Equations");
            testUpgrade.ParseEquations();
            EditorUtility.SetDirty(testUpgrade);
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
