using UnityEngine;

// a Generic Singleton class

public class Singleton<T> : MonoBehaviour where T: MonoBehaviour 
{
    // private static instance
	static T mInstance;

    // public static instance used to refer to Singleton (e.g. MyClass.Instance)
	public static T Instance
	{
		get 
		{
            // if no instance is found, find the first GameObject of type T
			if (mInstance == null) 
			{
				mInstance = FindObjectOfType<T> ();

                // if no instance exists in the Scene, create a new GameObject and add the Component T 
				if (mInstance == null) 
				{
					GameObject singleton = new GameObject (typeof(T).Name);
					mInstance = singleton.AddComponent<T> ();
				}
			}
            // return the singleton instance
			return mInstance;
		}
	}

	public virtual void Awake()
	{
        // if 
		if (mInstance == null) 
		{
			mInstance = this as T;

            // if you want the Singleton to persist on Level loads, then uncomment the DontDestroyOnLoad line:
			//DontDestroyOnLoad (this.gameObject);
		} 
		else 
		{
			Destroy (gameObject);
		}
	}
}
