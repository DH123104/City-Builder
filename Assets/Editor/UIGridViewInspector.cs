using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CBSK 
{
	/// <summary>
	/// Custom editor for UI Grid View.
	/// </summary>
	[CustomEditor (typeof(UIGridView))]
	public class UIGridViewInspector : Editor {

		static string[] presetOptions = new string[]{"Select...", "Occupied Space", "Space Availability", "Always Shown"};
		/// <summary>
		/// Draws the GUI.
		/// </summary>
		override public void OnInspectorGUI() {
			DrawPresetSelector ();
			DrawDefaultInspector ();
		}

		/// <summary>
		/// Draws the preset selector.
		/// </summary>
		virtual protected void DrawPresetSelector() {
			int index = EditorGUILayout.Popup ("Apply Preset", 0, presetOptions);
			UIGridView myTarget = (UIGridView)target;
			switch (index) {
			case 1:
				myTarget.showOnlyWhileMoving = true;
				myTarget.usePlacingSprite = false;
				myTarget.emptyColor = new Color (0, 1.0f, 0, 0.5f);
				myTarget.occupiedColor = new Color (1.0f, 0, 0, 0.5f);
				EditorUtility.SetDirty (myTarget);
				break;
			case 2:
				myTarget.showOnlyWhileMoving = true;
				myTarget.usePlacingSprite = true;
				myTarget.emptyColor = new Color (1.0f, 1.0f, 1.0f, 0.15f);
				myTarget.occupiedColor = new Color (1.0f, 1.0f, 1.0f, 0.15f);
				myTarget.canPlaceColor = new Color (0, 1.0f, 0, 0.5f);
				myTarget.cantPlaceColor = new Color (1.0f, 0, 0, 0.5f);
				EditorUtility.SetDirty (myTarget);
				break;
			case 3:
				myTarget.showOnlyWhileMoving = false;
				myTarget.usePlacingSprite = true;
				myTarget.emptyColor = new Color (1.0f, 1.0f, 1.0f, 0.15f);
				myTarget.occupiedColor = new Color (1.0f, 1.0f, 1.0f, 0.15f);
				myTarget.canPlaceColor = new Color (0, 1.0f, 0, 0.5f);
				myTarget.cantPlaceColor = new Color (1.0f, 0, 0, 0.5f);
				EditorUtility.SetDirty (myTarget);

				break;
			}

		}
	}
}