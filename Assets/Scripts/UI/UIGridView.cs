using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace CBSK
{
	/**
 	* Used to show a visible grid in the game.
 	*/
	public class UIGridView : MonoBehaviour {


		[Header ("View")]

		/**
		 * GameObject that holds the visible content (i.e the grid sprites).
		 */ 
		public GameObject visibleContent;

		/**
		 * Prefab to use for the grid view.
		 */ 
		public GameObject gridViewSpritePrefab;

		[Header ("Sprites")]
		/**
         * Grid sprite when empty.
         */
		public Sprite emptySprite;

		/**
         * Grid sprite when occupied. If null the empty sprite will be used.
         */
		public Sprite occupiedSprite;

		/**
         * Grid sprite when tile contains the building being placed. If null the empty sprite will be used.
         */
		public Sprite placingSprite;

		[Header ("Colors")]

		/**
		 * Color to apply to image when grid is empty.
		 */ 
		public Color emptyColor = new Color(0,1.0f,0,0.5f);

		/**
		 * Color to apply to image when grid is occupied.
		 */ 
		public Color occupiedColor = new Color(1.0f,0,0,0.5f);
	

		/**
		 * Color to apply to image when grid contains the building being placed.
		 */ 
		public Color canPlaceColor = new Color(0,1.0f,0,0.5f);

		/**
		 * Color to apply to image when grid contains the building being placed.
		 */ 
		public Color cantPlaceColor = new Color(1.0f,0,0,0.5f);


		[Header ("Options")]

		/**
		 * Should we show the grid always (false) or only when placing/mocving (true).
		 */
		public bool showOnlyWhileMoving = true;

		/**
		 * If true we show a different sprite/color for placing/moving sprites.
		 */ 
		public bool usePlacingSprite = false;

		protected Dictionary<GridPosition, UIGridViewSprite> posToSprite;

		/**
         * Internal initialisation.
         */
		void Awake()
		{
			if (occupiedSprite == null) occupiedSprite = emptySprite;
			if (placingSprite == null) placingSprite = emptySprite;
			posToSprite = new Dictionary<GridPosition, UIGridViewSprite> ();
		}

		/**
         * Initialise the grid view.
         */
		virtual public void UI_Init()
		{
			int gridSize = BuildingModeGrid.GetInstance ().gridSize;
			for (int y = 0; y < gridSize; y++)
			{
				for (int x = 0; x < gridSize; x++) 
				{
					GridPosition pos = new GridPosition (x, y);
					if (!(BuildingModeGrid.GetInstance ().GetObjectAtPosition (pos) is UnusableGrid)) {
						posToSprite.Add (pos, CreateGridViewSprite (pos));
					}
				}
			}
			if (showOnlyWhileMoving) visibleContent.SetActive (false);
		}

		/**
		 * Update the UI view.
		 */ 
		virtual public void UI_Update() {
			if (showOnlyWhileMoving) visibleContent.SetActive (false);
			int gridSize = BuildingModeGrid.GetInstance ().gridSize;
			Building placingObject = null;
			for (int y = 0; y < gridSize; y++) {
				for (int x = 0; x < gridSize; x++) {
					GridPosition pos = new GridPosition (x, y);
					if (posToSprite.ContainsKey(pos)) {
						IGridObject obj = BuildingModeGrid.GetInstance ().GetObjectAtPosition (pos);
						if (obj == null) {
							posToSprite[pos].UpdateView(true, emptySprite, emptyColor);
						}
						else if (obj is Building && ((Building)obj).State == BuildingState.MOVING) {
							placingObject = (Building)obj;
							posToSprite[pos].UpdateView(true, emptySprite, emptyColor);	
						} else {
							posToSprite[pos].UpdateView(true, occupiedSprite, occupiedColor);
						}
					}
				}
			}

			// Check for PLACING vs MOVING
			if (placingObject == null && BuildingManager.ActiveBuilding != null && (BuildingManager.ActiveBuilding.State == BuildingState.PLACING || BuildingManager.ActiveBuilding.State == BuildingState.PLACING_INVALID))
			{
				placingObject = BuildingManager.ActiveBuilding;
			}
				
			// Update the placing object grid positions
			if (usePlacingSprite && placingObject != null) {
				foreach (GridPosition p in placingObject.Shape) {
					if (posToSprite.ContainsKey (placingObject.MovePosition + p)) {
						IGridObject obj = BuildingModeGrid.GetInstance ().GetObjectAtPosition (placingObject.MovePosition + p);
						if (obj == (IGridObject)placingObject || obj == null) {
							posToSprite [placingObject.MovePosition + p].UpdateView (true, placingSprite, canPlaceColor);
						} else {
							posToSprite [placingObject.MovePosition + p].UpdateView (true, placingSprite, cantPlaceColor);
						}
					}
				}
			}

			// Update visibility if we are placing
			if (showOnlyWhileMoving) {
				if (placingObject != null) 
				{ 
					visibleContent.SetActive (true);
				}
				else if (	UIGamePanel.activePanel != null && UIGamePanel.activePanel.panelType == PanelType.PLACE_PATH)
				{
					visibleContent.SetActive (true);
				}
			}
		}

		/**
		 * Create grid view sprite.
		 */ 
		virtual protected UIGridViewSprite CreateGridViewSprite(GridPosition pos) {
			GameObject go = GameObject.Instantiate (gridViewSpritePrefab);
			UIGridViewSprite view = go.GetComponentInChildren<UIGridViewSprite> ();
			if (view == null) {
				Debug.LogError ("The gridViewSpritePrefab must contain a UIGridViewSprite component!");
				return null;
			}
			view.UI_Init (pos);
			view.UpdateView (true, emptySprite, emptyColor);
			go.transform.SetParent (visibleContent.transform, false);
			return view;
		}


	}

}