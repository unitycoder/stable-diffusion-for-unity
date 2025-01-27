﻿using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


namespace Witchpot.Runtime.StableDiffusion
{
    public static class ImagePorter
    {
#if UNITY_EDITOR
        public enum ImageType
        {
            PNG,
            JPG,
            TGA,
        }

        private static string DefaultDirPath => $"{Application.streamingAssetsPath}/StableDiffusion";
        private static string DefaultFileName => Guid.NewGuid().ToString();

        public static Texture2D GenerateTexture(byte[] bytes)
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);

            return texture;
        }

        public static bool SaveImage(Texture2D texture, string dir, string filename, ImageType type = ImageType.PNG)
        {
            if (texture == null || string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException();
            }

            if (!CreateDirectoryRecursive(dir))
            {
                Debug.LogError($"Failed to create directory. aborting.");
                return false;
            }

            byte[] data;
            string path;

            switch (type)
            {
                default:
                case ImageType.PNG:
                    data = texture.EncodeToPNG();
                    path = Path.Combine(dir, $"{filename}.png");
                    break;

                case ImageType.JPG:
                    data = texture.EncodeToJPG();
                    path = Path.Combine(dir, $"{filename}.jpg");
                    break;

                case ImageType.TGA:
                    data = texture.EncodeToTGA();
                    path = Path.Combine(dir, $"{filename}.tga");
                    break;
            }

            File.WriteAllBytes(path, data);
            AssetDatabase.Refresh();

            return true;
        }

        public static bool SaveImage(Texture2D texture, ImageType type = ImageType.PNG)
        {
            return SaveImage(texture, DefaultDirPath, DefaultFileName, type);
        }

        private static bool CreateDirectoryRecursive(string path)
        {
            if (Directory.Exists(path)) { return true; }

            var parent = Path.GetDirectoryName(path);

            if (!Directory.Exists(parent))
            {
                var result = CreateDirectoryRecursive(parent);

                if (result == false) { return false; }
            }

            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }

        private static bool LoadIntoImage<TImage>(Texture2D texture, TImage image, Action<Texture2D, TImage> exporter)
        {
            if (texture == null || image == null || exporter == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                exporter(texture, image);

                if (!Application.isPlaying)
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    EditorApplication.QueuePlayerLoopUpdate();
                    EditorSceneManager.MarkAllScenesDirty();
                    EditorUtility.RequestScriptReload();
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }

        public static bool LoadIntoImage(Texture2D texture, Image image)
        {
            return LoadIntoImage(texture, image, (t, i) => { i.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero); });
        }

        public static bool LoadIntoImage(Texture2D texture, RawImage image)
        {
            return LoadIntoImage(texture, image, (t, i) => { i.texture = t; });
        }

        public static bool LoadIntoImage(Texture2D texture, Renderer image)
        {
            return LoadIntoImage(texture, image, LoadIntoRenderer);
        }

        private static string _suffix = "(Instance)";

        private static void LoadIntoRenderer(Texture2D texture, Renderer image)
        {
            if (image.sharedMaterial == null) { return; }

            var mat = new Material(image.sharedMaterial);

            if (image.sharedMaterial.name.EndsWith(_suffix))
            {
                mat.name = $"{image.sharedMaterial.name}";
            }
            else
            {
                mat.name = $"{image.sharedMaterial.name}{_suffix}";
            }

            mat.mainTexture = texture;
                
            image.sharedMaterial = mat;
        }
#endif
    }
}
