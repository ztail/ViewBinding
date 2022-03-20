using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Ztail;

namespace ZtailEditor
{
	[Serializable]
	[CustomEditor(typeof(ViewBindings))]
	public class ViewBindingsEditor : Editor
	{
		private SerializedProperty m_Bindings;

		private ReorderableList m_List;

		private void OnEnable()
		{
			m_Bindings = serializedObject.FindProperty("m_Bindings");
			m_List = new ReorderableList(serializedObject, m_Bindings, true, true, true, true)
			{

				drawHeaderCallback = DrawListHeader,
				drawElementCallback = DrawElement,
				drawElementBackgroundCallback = DrawElementBackgroundCallback,
				drawFooterCallback = DrawFooter,
				footerHeight = 20,
			};

			if (ViewBindGameObjectInspectorGUI.lastSelectTarget)
			{
				if (target is ViewBindings viewBindings)
				{
					var index = viewBindings.bindings.FindIndex(x => x.target == ViewBindGameObjectInspectorGUI.lastSelectTarget);

					if (index >= 0)
					{
						m_List.Select(index, true);
						m_List.GrabKeyboardFocus();
						ViewBindGameObjectInspectorGUI.lastSelectTarget = null;
					}
				}
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			m_List.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawListHeader(Rect rect)
		{
			rect.width = rect.width / 2 - 20;
			rect.x += 20;
			GUI.Label(rect, "ID");

			rect.x += rect.width;
			GUI.Label(rect, "Target");
		}

		private void DrawElement(Rect rect, int index, bool selected, bool focused)
		{
			var totalWidth = rect.width;
			rect.height -= 2;
			rect.width = rect.width / 2 - 10;

			var binding = m_Bindings.GetArrayElementAtIndex(index);

			var bindId = binding.FindPropertyRelative("id");
			bindId.stringValue = EditorGUI.TextField(rect, bindId.stringValue);

			rect.x += rect.width + 5;
			rect.width = totalWidth - rect.width - 5;

			var previous = GUI.enabled;
			GUI.enabled = false;

			var bindTarget = binding.FindPropertyRelative("target");
			bindTarget.objectReferenceValue = EditorGUI.ObjectField(rect, bindTarget.objectReferenceValue, typeof(GameObject), true);

			GUI.enabled = previous;
		}

		private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isActive, isActive);
		}

		private void DrawFooter(Rect rect)
		{
			var width = Math.Min(rect.width, 80);
			rect.x = rect.x + rect.width - width;
			rect.width = width;
			if (GUI.Button(rect, "清理"))
			{
				
			}
		}
	}
}
