using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class Booster : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	// the UI.Image component
    private Image mImage;

    // the RectTransform component
    private RectTransform mRectXform;

    // reset position
    private Vector3 mStartPosition;

    // Board component
    private Board mBoard;

    // the Tile to apply the booster effect
    private Tile mTileTarget;

    // the one active Booster GameObject
    public static GameObject activeBooster;

    // UI.Text component for instructions
    public Text instructionsText;

    // text instructions 
    public string instructions = "drag over game piece to remove";

    // is the Booster enabled? (has the button been clicked once?)
    public bool isEnabled;

    // is this Booster intended to draggable (currently the only implemented behavior)
    public bool isDraggable = true;

    // has the Booster been locked (for use with another manager script)
    public bool isLocked;

    // useful for UI elements that may be colliding with drag event / add a CanvasGroup and add to List
    public List<CanvasGroup> canvasGroups;

    // actions to invoke when the drag is complete
    public UnityEvent boostEvent;

    // time bonus
    public int boostTime = 15;

    // initialize components
    private void Awake()
    {
        mImage = GetComponent<Image>();
        mRectXform = GetComponent<RectTransform>();
        mBoard = FindObjectOfType<Board>().GetComponent<Board>();
    }

    private void Start()
    {
        EnableBooster(false);
    }

    // toggle the Booster on/off
    public void EnableBooster(bool state)
    {
        isEnabled = state;

        if (state)
        {
            DisableOtherBoosters();
            Booster.activeBooster = gameObject;
        }
        else if (gameObject == Booster.activeBooster)
        {
            Booster.activeBooster = null;
        }

        mImage.color = (state) ? Color.white : Color.gray;

        if (instructionsText != null)
        {
            instructionsText.gameObject.SetActive(Booster.activeBooster != null);

            if (gameObject == Booster.activeBooster)
            {
                instructionsText.text = instructions;
            }
        }
    }

    // disable all other boosters
    void DisableOtherBoosters()
    {
        Booster[] allBoosters = Object.FindObjectsOfType<Booster>();

        foreach (Booster b in allBoosters)
        {
            if (b != this)
            {
                b.EnableBooster(false);
            }
        }
    }

    // toggle Booster state
    public void ToggleBooster()
    {
        EnableBooster(!isEnabled);
    }

    // frame where we begin dragging
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked)
        {
            mStartPosition = gameObject.transform.position;
            EnableCanvasGroups(false);
        }
    }

    // still dragging
    public void OnDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked && Camera.main != null)
        {
            Vector3 onscreenPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(mRectXform, eventData.position, 
                                                                    Camera.main, out onscreenPosition);
            gameObject.transform.position = onscreenPosition;

            RaycastHit2D hit2D = Physics2D.Raycast(onscreenPosition, Vector3.forward, Mathf.Infinity);

            if (hit2D.collider != null )
            {
                mTileTarget = hit2D.collider.GetComponent<Tile>();
            }
            else
            {
                mTileTarget = null;
            }
        }
    }

    // frame where we end drag
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked)
        {
            gameObject.transform.position = mStartPosition;
            EnableCanvasGroups(true);

            if (mBoard != null && mBoard.isRefilling)
            {
                return;
            }

            if (mTileTarget != null)
            {
                if (boostEvent != null)
                {
                    boostEvent.Invoke();
                }

                EnableBooster(false);

                mTileTarget = null;
                Booster.activeBooster = null;
            }
        }
    }

    // enable/disable blocksRaycasts for CanvasGroup components
    void EnableCanvasGroups(bool state)
    {
        if (canvasGroups != null && canvasGroups.Count > 0)
        {
            foreach (CanvasGroup cGroup in canvasGroups)
            {
                if (cGroup != null)
                {
                    cGroup.blocksRaycasts = state;
                }
            }
        }
    }

    // action to remove one GamePiece
    public void RemoveOneGamePiece()
    {
        if (mBoard != null && mTileTarget != null)
        {
            mBoard.boardClearer.ClearAndRefillBoard(mTileTarget.xIndex, mTileTarget.yIndex);
        }
    }

    // action to add bonus time
    public void AddTime()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddTime(boostTime);
        }
    }

    // action to replace GamePiece with Color Bomb
    public void DropColorBomb()
    {
        if (mBoard != null && mTileTarget != null)
        {
            mBoard.boardFiller.MakeColorBombBooster(mTileTarget.xIndex, mTileTarget.yIndex);
        }
    }
}
