using Unity.VectorGraphics;
using UnityEngine;

namespace TerritoryWars.UI.General
{
    public class PlayerAvatar : MonoBehaviour
    {
        public string Username;
        public Sprite[] Avatars;
        public SVGImage AvatarImage;

        public void SetAvatar(string username)
        {
            Sprite avatar = Avatars[GetAvatarVariant(username)];
            if (avatar != null)
            {
                AvatarImage.sprite = avatar;
            }
            else
            {
                Debug.LogWarning($"Avatar for username '{username}' not found.");
            }
        }
        
        public int GetAvatarVariant(string username)
        {
            if (string.IsNullOrEmpty(username))
                return 0;
            
            int hash = 0;
            foreach (char c in username)
            {
                hash += c;
            }
            
            int index = hash % 8;
            
            return index switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 7,
                5 => 5,
                6 => 6,
                7 => 4,
                0 or _ => 0
            };
        }
    }
}