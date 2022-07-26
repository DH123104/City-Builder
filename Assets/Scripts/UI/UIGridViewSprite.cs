using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace CBSK
{
	/**
 	* Used to show a visible grid in the game.
 	*/
	[RequireComponent (typeof(SpriteRenderer))]
	public class UIGridViewSprite : MonoBehaviour {


		/**
		 * Cached Sprite Renderer refernce.
		 */
		protected SpriteRenderer mySpriteRenderer;

		/**
		 * Grid position represennted by this item.
		 */ 
		protected GridPosition position;


		/**
         * Initialise the grid view.
         */
		virtual public void UI_Init(GridPosition pos)
		{
			mySpriteRenderer = GetComponent<SpriteRenderer> ();
			position = pos;
			transform.position = BuildingModeGrid.GetInstance ().GridPositionToWorldPosition (pos);
			transform.Translate (0, 0, -transform.position.z);
			mySpriteRenderer.enabled = false;
		}

		/**
		 * Update the grid view.
		 */
		virtual public void UpdateView(bool show, Sprite sprite, Color color) {
			mySpriteRenderer.sprite = sprite;
			mySpriteRenderer.color = color;
			mySpriteRenderer.enabled = show;
		}

	}
}