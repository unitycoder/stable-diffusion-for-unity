﻿using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Witchpot.Runtime.StableDiffusion
{
    public class Text2Img : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private StableDiffusionWebUISettings _stableDiffusionWebUISettings;
        [SerializeField, TextArea] private string _prompt;
        [SerializeField, TextArea] private string _negativePrompt;
        [SerializeField] private int _width = 960;
        [SerializeField] private int _height = 540;
        [SerializeField] private int _steps = 50;
        [SerializeField] private float _cfgScale = 7;
        //[SerializeField][Range(0.0f, 1.0f)] public float _denoisingStrength = 0.0f;
        [SerializeField] private long _seed = -1;
        [SerializeField] private ImagePorter.ImageType _exportType = ImagePorter.ImageType.PNG;
        [HideInInspector][SerializeField] private int _selectedSampler;
        [HideInInspector][SerializeField] private int _selectedModel;
        [HideInInspector][SerializeField] private int _selectedLoraModel;
        private bool _generating = false;

        public string Prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }

        public string[] SamplersList
        {
            get { return _stableDiffusionWebUISettings.Samplers; }
        }

        public string[] ModelsList
        {
            get { return _stableDiffusionWebUISettings.ModelNames; }
        }

        public string[] LoraModelsList
        {
            get { return _stableDiffusionWebUISettings.ModelNamesForLora; }
        }

        public int SelectedSampler
        {
            get { return _selectedSampler; }
            set { _selectedSampler = value; }
        }

        public int SelectedModel
        {
            get { return _selectedModel; }
            set { _selectedModel = value; }
        }

        public int SelectedLoraModel
        {
            get { return _selectedLoraModel; }
            set {_selectedLoraModel = value; }
        }

        public void OnClickGenerateButton()
        {
            if (_generating)
            {
                Debug.LogWarning("Generate already working.");
                return;
            }

            if (string.IsNullOrEmpty(_prompt))
            {
                Debug.LogWarning("Prompt is empty");
                return;
            }

            GenerateImage().Forget();
        }

        private async ValueTask GenerateImage()
        {
            if (EditorApplication.isCompiling) return;

            try
            {
                _generating = true;

                Debug.Log("Image generating started.");

                using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Options(_stableDiffusionWebUISettings))
                {
                    var body = client.GetRequestBody();

                    body.sd_model_checkpoint = ModelsList[_selectedModel];

                    var responses = await client.SendRequestAsync(body);
                }

                using (var client = new StableDiffusionWebUIClient.Post.SdApi.V1.Txt2Img(_stableDiffusionWebUISettings))
                {
                    var body = client.GetRequestBody(_stableDiffusionWebUISettings);

                    body.prompt = _prompt;
                    body.steps = _steps;
                    body.negative_prompt = _negativePrompt;
                    body.seed = _seed;
                    body.cfg_scale = _cfgScale;
                    //body.denoising_strength = _denoisingStrength;
                    body.width = _width;
                    body.height = _height;

                    var responses = await client.SendRequestAsync(body);

                    using (var clientInfo = new StableDiffusionWebUIClient.Post.SdApi.V1.PngInfo(_stableDiffusionWebUISettings))
                    {
                        var bodyInfo = clientInfo.GetRequestBody();
                        bodyInfo.SetImage(responses.GetImage());

                        var responsesInfo = await clientInfo.SendRequestAsync(bodyInfo);

                        var dic = responsesInfo.Parse();

                        Debug.Log($"Seed:{dic.GetValueOrDefault("Seed")}");
                    }

                    var texture = ImagePorter.GenerateTexture(responses.GetImage());

                    if (ImagePorter.SaveImage(texture, _exportType))
                    {
                        Debug.Log("Image generating completed.");
                    }
                    else
                    {
                        Debug.LogWarning("Faled to save generated image.");
                    }

                    Image image = GetComponent<Image>();
                    if (image != null)
                    {
                        if (ImagePorter.LoadIntoImage(texture, image))
                        {
                            Debug.Log($"Image loaded in {image.name}.", image);
                            return;
                        }
                    }

                    RawImage rawImage = GetComponent<RawImage>();
                    if (rawImage != null)
                    {
                        if (ImagePorter.LoadIntoImage(texture, rawImage))
                        {
                            Debug.Log($"Image loaded in {rawImage.name}.", rawImage);
                            return;
                        }
                    }

                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (ImagePorter.LoadIntoImage(texture, renderer))
                        {
                            Debug.Log($"Image loaded in {renderer.name}.", renderer);
                            return;
                        }
                    }
                }
            }
            finally
            {
                _generating = false;
            }
        }
#endif
    }
}
