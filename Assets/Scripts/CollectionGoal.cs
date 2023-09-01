using UnityEngine;

public class CollectionGoal : MonoBehaviour
{
    public GamePiece prefabToCollect;

    [Range(1,50)]
    public int numberToCollect = 5;

    private SpriteRenderer mSpriteRenderer;

    // Use this for initialization
    private void Start()
    {
        if (prefabToCollect != null)
        {
            mSpriteRenderer = prefabToCollect.GetComponent<SpriteRenderer>();
        }
    }

    public void CollectPiece(GamePiece piece)
    {
        if (piece == null)
        {
            Debug.LogError("COLLECTIONGOAL CollectPiece: missing piece");
            return;
        }

        if (prefabToCollect == null)
        {
            Debug.LogError("COLLECTIONGOAL CollectPiece: missing prefab specified...");
            return;
        }

        if (mSpriteRenderer == null)
        {
            mSpriteRenderer = prefabToCollect.GetComponent<SpriteRenderer>();
        }

        if (mSpriteRenderer == null)
        {
            Debug.LogError("COLLECTIONGOAL CollectPiece: prefab missing SpriteRenderer...");
            return;
        }

        if (piece != null)
        {

            SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
            
            if (mSpriteRenderer.sprite == spriteRenderer.sprite && prefabToCollect.matchValue == piece.matchValue)
            {
                numberToCollect--;
                numberToCollect = Mathf.Clamp(numberToCollect, 0, numberToCollect);
            }
        }
    }
}
