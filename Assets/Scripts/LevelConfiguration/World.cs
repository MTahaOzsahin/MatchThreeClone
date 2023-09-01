using UnityEngine;

namespace LevelConfiguration
{
    [CreateAssetMenu(fileName = "LevelConfiguration", menuName = "LevelConfiguration/World")]
    public class World : ScriptableObject
    {
        public Level[] levels;

    }
}
