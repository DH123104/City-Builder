using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/**
 * The grid used in building mode.
 */
namespace CBSK
{
    public class BuildingModeGrid : AbstractGrid
    {
		[Header ("Grid Shape")]
		public int gridUnusuableHeight;           // Height (from bottom AND top) that cannot be used (used to make the grid a square instead of a diamond).
		public int gridUnusuableWidth;            // Width from (left AND right) that cannot be used (used to make the grid a square instead of a diamond).

		[Header ("World Size")]
        public float widthOfGridInWorldUnits;       // Width of the grid in world units (not grid units)
        public float heightOfGridInWorldUnits;      // Height of the grid in world units (not grid units)

		[Header ("World Position")]
		public Vector2 positionOffset;				// Offset to apply to the building view.

		[Header ("View")]
		public GameObject gameView;					// View for the grid.

#if UNITY_EDITOR
		[Header ("Editor")]
		public bool showGridInEditor;
		private int cachedUnsuableHeight;
		private int cachedUnsuableWidth;
#endif

        protected static BuildingModeGrid instance; // Static reference to this grid.

        protected bool initialised = false;         // Has this grid been initialised.

        /**
         * Get the instance of the grid class or create if one has not yet been created.
         * 
         * @returns An instance of the grid class.
         */
        public static BuildingModeGrid GetInstance()
        {
            return instance;
        }

        /**
         * If there is already a grid destroy self, else initialise and assign to the static reference.
         */
        void Awake()
        {
            if (instance == null)
            {
                if (!initialised) Init();
                instance = (BuildingModeGrid)this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
			
		void Start() {
			PostInit ();
		}

        /**
         * Initialise the grid.
         */
        virtual protected void Init()
        {
            initialised = true;
            grid = new IGridObject[gridSize, gridSize];
            FillUnusableGrid();
            gridObjects = new Dictionary<IGridObject, List<GridPosition>>();
        }

		override public void AddObjectAtPosition(IGridObject gridObject, GridPosition position)
		{
			base.AddObjectAtPosition (gridObject, position);
			if (gameView != null) gameView.SendMessage ("UI_Update", SendMessageOptions.DontRequireReceiver);
		}

		override public void RemoveObject(IGridObject gridObject)
		{
			base.RemoveObject (gridObject);
			if (gameView != null) gameView.SendMessage ("UI_Update", SendMessageOptions.DontRequireReceiver);
		}

		virtual protected void PostInit() {
			BuildingManager.GetInstance ().gameView.transform.position = new Vector3 (positionOffset.x, positionOffset.y, BuildingManager.GetInstance ().gameView.transform.position.z);
			if (gameView != null) gameView.SendMessage ("UI_Init", SendMessageOptions.DontRequireReceiver);
		}

        override public Vector3 GridPositionToWorldPosition(GridPosition position)
        {
            return GridPositionToWorldPosition(position, GridPosition.DefaultShape);
        }

        override public Vector3 GridPositionToWorldPosition(GridPosition position, GridPosition[] shape)
        {
            float x = (widthOfGridInWorldUnits / 2) * (position.x - position.y);
            float y = (heightOfGridInWorldUnits / 2) * (position.x + position.y);
            float sz = 9999.0f;
            float lz = -9999.0f;
            float tsz;
            // TODO Clean up and fix to cater for even odder shapes
            foreach (GridPosition pos in shape)
            {
                tsz = ((position.y + pos.y) * (heightOfGridInWorldUnits / 2)) + ((position.x + pos.x) * (heightOfGridInWorldUnits / 2)) - 2;
                if (sz >= tsz) sz = tsz;
                if (lz <= tsz) lz = tsz;
            }

            return new Vector3(x, y, ((lz + sz) / 2.0f));

        }

        override public GridPosition WorldPositionToGridPosition(Vector3 position)
        {
            int tx = Mathf.RoundToInt(((position.x / (widthOfGridInWorldUnits / 2)) + (position.y / (heightOfGridInWorldUnits / 2))) / 2);
            int ty = Mathf.RoundToInt(((position.y / (heightOfGridInWorldUnits / 2)) - (position.x / (widthOfGridInWorldUnits / 2))) / 2);
            return new GridPosition(tx, ty);
        }

        /**
         * Get the building at the given position. If the space is empty or the object
         * at the position is not a building return null.
         */
        public Building GetBuildingAtPosition(GridPosition position)
        {
            IGridObject result;
            result = GetObjectAtPosition(position);
            if (result is Building) return (Building)result;
            return null;
        }

		/**
		 * Trigger a grid view update.
		 */ 
		public void UpdateGameView() 
		{
			if (gameView != null) gameView.SendMessage ("UI_Update", SendMessageOptions.DontRequireReceiver);
		}

        /**
         * Fill up the grid so the diamon becomes a rectangle.
         */
        private void FillUnusableGrid()
        {

            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    // Fill Bottom
                    if (x + y < gridUnusuableHeight)
                    {
                        grid[x, y] = new UnusableGrid();
                    }
                    // Fill Top
                    if (x + y > (gridSize * 2) - gridUnusuableHeight)
                    {
                        grid[x, y] = new UnusableGrid();
                    }
                    // Fill Left
                    if (x - y > gridSize - gridUnusuableWidth)
                    {
                        grid[x, y] = new UnusableGrid();
                    }
                    // Fill Right
                    if (y - x > gridSize - gridUnusuableWidth)
                    {
                        grid[x, y] = new UnusableGrid();
                    }
                }
            }

            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {

                }
            }
            // Fill Left
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (x + y > (gridSize * 2) - gridUnusuableHeight)
                    {
                        grid[x, y] = new UnusableGrid();
                    }
                }
            }
            // Fill Right
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    if (x + y > (gridSize * 2) - gridUnusuableHeight)
                    {
                        grid[x, y] = new UnusableGrid();
                    }
                }
            }
        }

#if UNITY_EDITOR

		void OnDrawGizmos() {
			if (!showGridInEditor) return;
			if (grid == null || grid.GetLength(0) != gridSize || grid.GetLength(1)  != gridSize || 
				cachedUnsuableHeight != gridUnusuableHeight || cachedUnsuableWidth != gridUnusuableWidth) {
				grid = new IGridObject[gridSize, gridSize];
				FillUnusableGrid ();
				cachedUnsuableHeight = gridUnusuableHeight;
				cachedUnsuableWidth = gridUnusuableWidth;
			}
			for (int y = 0; y < gridSize; y++)
			{
				for (int x = 0; x < gridSize; x++) {
					if (!(GetObjectAtPosition(new GridPosition(x,y)) is UnusableGrid)) {
						Vector3 pos = GridPositionToWorldPosition(new GridPosition(x,y), GridPosition.DefaultShape) + (Vector3) positionOffset;
						Vector3 posA = pos + new Vector3(0, heightOfGridInWorldUnits / 2.0f, 0);
						Vector3 posB = pos + new Vector3(widthOfGridInWorldUnits / 2.0f,0, 0);
						Vector3 posC = pos + new Vector3(0, -heightOfGridInWorldUnits / 2.0f, 0);
						Vector3 posD = pos + new Vector3(-widthOfGridInWorldUnits / 2.0f, 0, 0);
						posA.z = transform.position.z;
						posB.z = transform.position.z;
						posC.z = transform.position.z;
						posD.z = transform.position.z;
						Gizmos.DrawLine(posA, posB);
						Gizmos.DrawLine(posB, posC);
						Gizmos.DrawLine(posC, posD);
						Gizmos.DrawLine(posD, posA);
					}
				}
			}
		}
#endif
    }

}
