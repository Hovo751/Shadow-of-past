using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(Frame))]
public class FrameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.BeginChangeCheck();

        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label
        );

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = position.y + EditorGUIUtility.singleLineHeight;

            var hitbox = property.FindPropertyRelative("hitbox");
            var throwhitbox = property.FindPropertyRelative("throwhitbox");

            Draw(ref y, position, property, "cancelLvl");
            Draw(ref y, position, property, "canRotate");

            Draw(ref y, position, property, "addPosX");
            Draw(ref y, position, property, "addPosY");
            Draw(ref y, position, property, "setVelocityHorizontal");
            Draw(ref y, position, property, "setVelocityHbool");
            Draw(ref y, position, property, "setVelocityVertical");
            Draw(ref y, position, property, "setVelocityVbool");
            Draw(ref y, position, property, "setAccelerationHorizontal");
            Draw(ref y, position, property, "setAccelerationHbool");
            Draw(ref y, position, property, "setAccelerationVertical");
            Draw(ref y, position, property, "setAccelerationVbool");
            GUI.backgroundColor = new Color(0.0f, 0.0f, 1.0f, 0.5f);
            Draw(ref y, position, property, "hurtbox");
            GUI.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            Draw(ref y, position, property, "hitbox");

            if (hitbox.arraySize > 0)
            {
                EditorGUI.indentLevel++;
                Draw(ref y, position, property, "hitboxLand");
                Draw(ref y, position, property, "hitboxPush");
                Draw(ref y, position, property, "hitboxPushAirVertical");
                Draw(ref y, position, property, "hitboxPushAirHorizontal");
                Draw(ref y, position, property, "hitboxPushOnBlock");
                Draw(ref y, position, property, "hitboxDamage");
                Draw(ref y, position, property, "chipDamage");
                Draw(ref y, position, property, "hitboxHitStun");
                Draw(ref y, position, property, "hitStop");
                Draw(ref y, position, property, "hitStopOnBlock");
                Draw(ref y, position, property, "hitboxBlockStun");
                Draw(ref y, position, property, "addJuggleScaling");
                Draw(ref y, position, property, "alwaysLaunch");
                Draw(ref y, position, property, "canHitHighOrLow");
                EditorGUI.indentLevel--;
            }

            GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 0.5f);
            Draw(ref y, position, property, "throwbox");
            GUI.backgroundColor = new Color(1.0f, 1.0f, 0.0f, 0.5f);
            Draw(ref y, position, property, "throwhitbox");
            if (throwhitbox.arraySize > 0)
            {
                EditorGUI.indentLevel++;
                Draw(ref y, position, property, "isAirThrow");
                Draw(ref y, position, property, "setOpponentPositionOnThrow");
                Draw(ref y, position, property, "setAnimOnMiss");
                Draw(ref y, position, property, "setAnimFrameOnMiss");
                EditorGUI.indentLevel--;
            }
            GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.5f);
            Draw(ref y, position, property, "collisionBox");

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndChangeCheck();
        EditorGUI.EndProperty();
    }

    void Draw(ref float y, Rect position, SerializedProperty property, string fieldName)
    {
        var field = property.FindPropertyRelative(fieldName);

        float height = EditorGUI.GetPropertyHeight(field, true);

        Rect rect = new Rect(
            position.x,
            y,
            position.width,
            height
        );

        EditorGUI.PropertyField(rect, field, true);
        GUI.backgroundColor = Color.white;

        y += height + EditorGUIUtility.standardVerticalSpacing;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        float height = EditorGUI.GetPropertyHeight(
            property,
            label,
            true
        );
        
        var hitbox = property.FindPropertyRelative("hitbox");
        var throwhitbox = property.FindPropertyRelative("throwhitbox");

        if (hitbox.arraySize <= 0)
        {
            height -= 14 * EditorGUIUtility.standardVerticalSpacing;
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxLand"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxPush"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxPushAirVertical"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxPushAirHorizontal"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxPushOnBlock"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxDamage"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("chipDamage"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxHitStun"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitStop"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitStopOnBlock"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("hitboxBlockStun"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("addJuggleScaling"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("alwaysLaunch"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("canHitHighOrLow"), true);
        }

        if (throwhitbox.arraySize <= 0)
        {
            height -= 4 * EditorGUIUtility.standardVerticalSpacing;
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("isAirThrow"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("setOpponentPositionOnThrow"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("setAnimOnMiss"), true);
            height -= EditorGUI.GetPropertyHeight(property.FindPropertyRelative("setAnimFrameOnMiss"), true);
        }

        return height;
    }
}