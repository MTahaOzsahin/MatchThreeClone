using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GamePieceBreakable : GamePiece
{

    public int breakableValue;

    // array of Sprites used to show damage on Breakable Tile
    public Sprite[] breakableSprites;

    // the Sprite for this Tile
    private SpriteRenderer mSpriteRenderer;

    public float breakDelay = 0.25f;
    public bool isBroken;

    public override void Awake()
    {
        base.Awake();

        // initialize our SpriteRenderer
        mSpriteRenderer = GetComponent<SpriteRenderer>();

        if (breakableSprites[breakableValue] != null)
        {
            mSpriteRenderer.sprite = breakableSprites[breakableValue];
        }

    }

    public void BreakPiece()
    {
        if (isBroken)
        {
            return;
        }

        StartCoroutine(BreakPieceRoutine());
    }

    // decrement the breakable value, switch to the appropriate sprite
    // and conver the Tile to become normal once the breakableValue reaches 0
    IEnumerator BreakPieceRoutine()
    {
        if (!isBroken)
        {
            breakableValue = Mathf.Clamp(breakableValue--, 0, breakableValue);

            if (breakableSprites[breakableValue] != null)
            {
                mSpriteRenderer.sprite = breakableSprites[breakableValue];
            }
        }

        yield return new WaitForSeconds(breakDelay);

        // if we are broken already, just clear normally
        if (breakableValue == 0)
        {
            isBroken = true;

        }
    }

}
