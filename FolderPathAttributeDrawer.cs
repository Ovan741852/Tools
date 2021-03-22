using UnityEngine;
using UnityEditor;

namespace UNONE.Foundation.Utilities
{
    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public class FolderPathAttributeDrawer : PropertyDrawer
    {
		public const float offset = 2;

		public const float buttonWidth = 50;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var pPath = position;
			pPath.width -= buttonWidth + offset;
			GUI.enabled = false;
			EditorGUI.PropertyField(pPath, property, label);
			GUI.enabled = true;

			var pButton = position;
			pButton.x += pPath.width + offset;
			pButton.width = buttonWidth;
			if (GUI.Button(pButton, "Set"))
			{
				var title = $"select {label}..";
				var folder = property.stringValue;
				var folderPath = EditorUtility.OpenFolderPanel(title, folder, string.Empty);
				if (!string.IsNullOrEmpty(folderPath))
					property.stringValue = folderPath.Replace(Application.dataPath, "Assets");
			}

			EditorGUI.EndProperty();
		}
	}
}