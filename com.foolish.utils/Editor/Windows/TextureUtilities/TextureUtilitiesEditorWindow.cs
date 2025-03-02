using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Foolish.Utils.Editor.Windows
{
    public class TextureUtilitiesEditorWindow : EditorWindow
    {
        WindowType windowType = WindowType.Selection;

        enum WindowType
        {
            Selection,
            Slicer,
            Merger,
            Packer
        }

        [MenuItem("Tools/Developer/TextureUtilities")]
        public static void ShowWindow()
        {
            GetWindow<TextureUtilitiesEditorWindow>("TextureUtilities");
        }

        void OnGUI()
        {
            if (windowType != WindowType.Selection)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Go to Selection"))
                    {
                        windowType = WindowType.Selection;
                        return;
                    }
                }
                GUILayout.Space(10);
            }
            switch (windowType)
            {
                case WindowType.Slicer:
                    DrawSliceWindow();
                    return;
                case WindowType.Merger:
                    DrawMergeWindow();
                    return;
                case WindowType.Packer:
                    DrawTexturePackerWindow();
                    return;
                default:
                    DrawSelectionWindow();
                    break;
            }
        }

        #region Selection

        void DrawSelectionWindow()
        {
            if (GUILayout.Button("Slice Sprites"))
            {
                windowType = WindowType.Slicer;
                return;
            }
            if (GUILayout.Button("Merge textures"))
            {
                windowType = WindowType.Merger;
                return;
            }
            if (GUILayout.Button("Pack textures into Texture2DArray"))
            {
                windowType = WindowType.Packer;
            }
        }

        #endregion

        #region Slicer

        Texture2D selectedTexture;

        void DrawSliceWindow()
        {
            GUILayout.Label("Select Texture to Slice", EditorStyles.boldLabel);

            selectedTexture = EditorGUILayout.ObjectField(selectedTexture, typeof(Texture2D), false) as Texture2D;

            if (selectedTexture != null)
            {
                if (GUILayout.Button("Slice Sprites"))
                {
                    SliceSprites();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a texture to slice.", MessageType.Info);
            }
        }

        void SliceSprites()
        {
            string path = AssetDatabase.GetAssetPath(selectedTexture);
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
            string originalFolderPath = Path.GetDirectoryName(path);
            foreach (Object sprite in sprites)
            {
                if (sprite is Sprite slicedSprite)
                {
                    Texture2D slicedTexture = new Texture2D((int)slicedSprite.rect.width, (int)slicedSprite.rect.height);
                    Color[] pixels = selectedTexture.GetPixels((int)slicedSprite.rect.x, (int)slicedSprite.rect.y, (int)slicedSprite.rect.width, (int)slicedSprite.rect.height);
                    slicedTexture.SetPixels(pixels);
                    slicedTexture.Apply();
                    string spriteName = slicedSprite.name.Replace('/', '_');
                    string savePath = Path.Combine(originalFolderPath!, spriteName + ".png");
                    savePath = savePath.Replace(" ", string.Empty);
                    File.WriteAllBytes(savePath, slicedTexture.EncodeToPNG());
                }
            }
            AssetDatabase.Refresh();
        }

        #endregion

        #region Merger

        List<Texture2D> texturesToMerge = new();
        bool mergeHorizontally = true;
        string namePattern = "{0}_all";
        bool destroyAfterMerge;
        bool enableSizeCheck = true;

        Vector2 scrollPosition = Vector2.zero;

        void DrawMergeWindow()
        {
            GUILayout.Label("Merge texturesToMerge", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("Box"))
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                for (int i = 0; i < texturesToMerge.Count; i++)
                {
                    texturesToMerge[i] = (Texture2D)EditorGUILayout.ObjectField($"Texture {i + 1}", texturesToMerge[i], typeof(Texture2D), false);
                }
                EditorGUILayout.EndScrollView();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("From selection", GUILayout.Width(100)))
                    {
                        GetTexturesFromSelection();
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add Texture", GUILayout.Width(100)))
                    {
                        texturesToMerge.Add(null);
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (texturesToMerge.Count > 0 && GUILayout.Button("Remove Texture", GUILayout.Width(100)))
                    {
                        texturesToMerge.RemoveAt(texturesToMerge.Count - 1);
                    }
                }
            }

            enableSizeCheck = EditorGUILayout.Toggle("Enable texture size check", enableSizeCheck);
            mergeHorizontally = EditorGUILayout.Toggle("Merge Horizontally", mergeHorizontally);
            namePattern = EditorGUILayout.TextField("Pattern", namePattern);
            destroyAfterMerge = EditorGUILayout.Toggle("Delete initial image", destroyAfterMerge);

            if (GUILayout.Button("Merge texturesToMerge"))
            {
                MergeTextures();
            }
        }

        void GetTexturesFromSelection()
        {
            Object[] selection = Selection.objects;
            texturesToMerge = selection.OfType<Texture2D>().ToList();
        }

        void MergeTextures()
        {
            if (texturesToMerge == null || texturesToMerge.Count < 2 || texturesToMerge.All(t => t == null))
            {
                Debug.LogError("Please select at least 2 texturesToPack.");
                return;
            }

            texturesToMerge.RemoveAll(t => t == null);

            if (enableSizeCheck && !CheckSizes(texturesToMerge.ToArray()))
            {
                return;
            }

            string directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(texturesToMerge[0]));
            string combinedTextureName;
            try
            {
                combinedTextureName = $"{string.Format(namePattern, texturesToMerge[0].name)}.png";
            }
            catch
            {
                combinedTextureName = $"{texturesToMerge[0].name}_combined.png";
                namePattern = "{0}_combined";
            }

            var combinedTexture = texturesToMerge[0];
            for (int i = 1; i < texturesToMerge.Count; i++)
            {
                combinedTexture = Create(combinedTexture, texturesToMerge[i]);
            }

            byte[] bytes = combinedTexture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(directory!, combinedTextureName), bytes);
            if (destroyAfterMerge)
            {
                foreach (var texture in texturesToMerge)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(texture));
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"texturesToMerge merged and saved at: {directory}/{combinedTextureName}");
        }

        bool CheckSizes(Texture2D[] textures)
        {
            var width = textures[0].width;
            var height = textures[0].height;
            for (var index = 1; index < textures.Length; index++)
            {
                if (!textures[index])
                {
                    continue;
                }

                if (width != textures[index].width)
                {
                    Debug.LogError($"Different texture width between 1 and {index}");
                    return false;
                }

                if (height != textures[index].height)
                {
                    Debug.LogError($"Different texture height between 1 and {index}");
                    return false;
                }
            }

            return true;
        }

        Texture2D Create(Texture2D first, Texture2D second)
        {
            Color[] pixels = Merge(first, second);

            int width = mergeHorizontally ? first.width + second.width : Mathf.Max(first.width, second.width);
            int height = mergeHorizontally ? Mathf.Max(first.height, second.height) : first.height + second.height;

            Texture2D combinedTexture = new Texture2D(width, height);
            combinedTexture.SetPixels(pixels);
            combinedTexture.Apply();

            return combinedTexture;
        }

        Color[] Merge(Texture2D texture1, Texture2D texture2)
        {
            Color[] pixels1 = texture1.GetPixels();
            Color[] pixels2 = texture2.GetPixels();

            if (mergeHorizontally)
            {
                int width = texture1.width + texture2.width;
                int height = Mathf.Max(texture1.height, texture2.height);
                Color[] result = new Color[width * height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < texture1.width; x++)
                    {
                        result[y * width + x] = pixels1[Mathf.Clamp(y, 0, texture1.height - 1) * texture1.width + x];
                    }

                    for (int x = 0; x < texture2.width; x++)
                    {
                        result[y * width + texture1.width + x] = pixels2[Mathf.Clamp(y, 0, texture2.height - 1) * texture2.width + x];
                    }
                }

                return result;
            }
            else
            {
                int width = Mathf.Max(texture1.width, texture2.width);
                int height = texture1.height + texture2.height;
                Color[] result = new Color[width * height];

                for (int y = 0; y < texture2.height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        result[y * width + x] = pixels2[y * texture2.width + Mathf.Clamp(x, 0, texture2.width - 1)];
                    }
                }

                for (int y = 0; y < texture1.height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        result[(y + texture2.height) * width + x] = pixels1[y * texture1.width + Mathf.Clamp(x, 0, texture1.width - 1)];
                    }
                }

                return result;
            }
        }

        #endregion

        #region Texture Packer

        string fileNames = "None =)";
        bool isDone;
        bool isFirsUpdate;
        List<Texture2D> texturesToPack = new();

        void DrawTexturePackerWindow()
        {
            GUILayout.Label("Select Textures in project tab");
            if (isDone)
            {
                GUILayout.BeginHorizontal();
            }
            if (GUILayout.Button("CheckNames"))
            {
                isFirsUpdate = true;
                CheckNames();
            }

            if (isDone)
            {
                if (isFirsUpdate)
                {
                    isFirsUpdate = false;
                }
                else
                {
                    if (GUILayout.Button("Create 2DArray"))
                    {
                        CreateTextureArray();
                    }

                    GUILayout.EndHorizontal();
                }
            }
            fileNames = EditorGUILayout.TextField("Selected elements", fileNames);
        }

        void CheckNames()
        {
            Object[] selection = Selection.objects;
            texturesToPack = selection.OfType<Texture2D>().ToList();
            isDone = texturesToPack.Count > 0;
            if (isDone)
            {
                fileNames = string.Empty;
                StringBuilder sb = new StringBuilder();
                foreach (var texture2D in texturesToPack)
                {
                    sb.AppendJoin('\n', texture2D.name);
                }
                fileNames = sb.ToString();
            }
            else
            {
                fileNames = "None =)";
            }
        }

        void CreateTextureArray()
        {
            Texture2D firstTexture = texturesToPack[0];
            string textureArrayName = $"{firstTexture.name}_TexAr";

            int width = firstTexture.width;
            int height = firstTexture.height;
            int depth = texturesToPack.Count;

            var textureArray = new Texture2DArray(width, height, depth, TextureFormat.RGBA32, false);

            for (int i = 0; i < depth; i++)
            {
                Graphics.CopyTexture(texturesToPack[i], 0, 0, textureArray, i, 0);
            }

            string folderPath = AssetDatabase.GetAssetPath(firstTexture);
            folderPath = Path.GetDirectoryName(folderPath);

            string textureArrayPath = Path.Combine(folderPath!, $"{textureArrayName}.asset");

            AssetDatabase.CreateAsset(textureArray, textureArrayPath);
            AssetDatabase.Refresh();
            isDone = false;
            texturesToPack.Clear();
            fileNames = "None =)";
            Debug.Log($"Texture2DArray created and saved to '{textureArrayPath}'");
        }

        #endregion

    }
}