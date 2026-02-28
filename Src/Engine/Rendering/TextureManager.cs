using System;
using System.Collections.Generic;
using System.IO;

namespace SpatialEngine.Rendering
{
    public static class TextureManager
    {
        public static Dictionary<string, int> textureLocToIndex;
        public static List<Texture> textures;
        static Texture missingTexture;
        
        public static void Init()
        {
            textureLocToIndex = new Dictionary<string, int>();
            textures = new List<Texture>();
            missingTexture = new Texture();
            missingTexture.LoadTexture("");
        }

        public static bool LoadTexture(string texture)
        {
            if (!File.Exists(Resources.ImagePath + texture))
                return false;
            
            if (textureLocToIndex.TryAdd(texture, textures.Count))
            {
                textures.Add(new Texture());
                textures[^1].LoadTexture(texture);
                return true;
            }

            return false;
        }

        public static Texture RetrieveTexture(string texture)
        {
            if (textureLocToIndex.TryGetValue(texture, out int index))
            {
                return textures[index];
            }
            else
            {
                if (LoadTexture(texture))
                {
                    return textures[textureLocToIndex[texture]];
                }
            }
            
            //return a missing texture
            return missingTexture;
        }
        
        public static Texture RetrieveTexture(int texture)
        {
            if (texture >= 0 && texture < textures.Count)
            {
                return textures[texture];
            }
            
            //return a missing texture
            return missingTexture;
        }
    }
}