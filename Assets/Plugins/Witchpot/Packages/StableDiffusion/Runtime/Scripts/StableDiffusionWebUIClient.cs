﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;


namespace Witchpot.Runtime.StableDiffusion
{
    public class WebRequestException : Exception
    {
        public WebRequestException(string str) : base(str) { }
    }

    public static class StableDiffusionWebUIClient
    {
        public interface IParameters { }
        public interface IRequestBody { }
        public interface IResponses { }

        public struct RequestHeader
        {
            private string _name;
            public string Name => _name;

            private string _value;
            public string Value => _value;

            public RequestHeader(string name, string value)
            {
                _name = name;
                _value = value;
            }
        }

        public abstract class WebRequestWrapper : IDisposable
        {
            // field
            private UnityWebRequest _request;

            // property
            protected UnityWebRequest WebRequest => _request;
            public bool isDone => _request.isDone;
            public UnityWebRequest.Result Result => _request.result;

            protected WebRequestWrapper(string uri, string method, IReadOnlyList<RequestHeader> list)
            {
                _request = new UnityWebRequest(uri, method);

                foreach (var header in list)
                {
                    _request.SetRequestHeader(header.Name, header.Value);
                }
            }

            protected static UploadHandlerRaw GetUploadHandler(IRequestBody body)
            {
                var json = JsonUtility.ToJson(body);
                var bytes = Encoding.UTF8.GetBytes(json);

                return new UploadHandlerRaw(bytes);
            }

            protected static TResponses GetResponce<TResponses>(string result) where TResponses : IResponses
            {
                return JsonUtility.FromJson<TResponses>(result);
            }

            protected static IReadOnlyList<TResponses> GetListResponce<TResponses>(string result) where TResponses : IResponses
            {
                return JsonConvert.DeserializeObject<TResponses[]>(result);
            }

            protected static string GetImageString(byte[] data)
            {
                return Convert.ToBase64String(data);
            }

            protected static string[] GetImageStringArray(byte[] data)
            {
                return new string[] { GetImageString(data) };
            }

            protected static byte[] GetImageByteArray(string[] images)
            {
                return Convert.FromBase64String(images[0].Split(",")[0]);
            }

            protected void CheckDone()
            {
                if (isDone)
                {
                    throw new WebRequestException("WebRequest already done.");
                }
            }

            protected TResult ParseResult<TResult>(Func<string, TResult> parser)
            {
                if (WebRequest.result == UnityWebRequest.Result.Success)
                {
                    if (string.IsNullOrEmpty(WebRequest.downloadHandler.text) || WebRequest.downloadHandler.text == "null")
                    {
                        return default;
                    }
                    else
                    {
                        return parser(WebRequest.downloadHandler.text);
                    }
                }
                else
                {
                    throw new WebRequestException($"{WebRequest.error}");
                }
            }

            protected async ValueTask<TResponses> SendRequestAsync<TResponses>()
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetResponce<TResponses>);
            }

            protected async ValueTask<IReadOnlyList<TResponses>> SendRequestAsListResponsesAsync<TResponses>()
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetListResponce<TResponses>);
            }

            protected async ValueTask<TResponses> SendRequestAsync<TRequestBody, TResponses>(TRequestBody body)
                where TRequestBody : IRequestBody
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.uploadHandler = GetUploadHandler(body);
                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetResponce<TResponses>);
            }

            protected async ValueTask<IReadOnlyList<TResponses>> SendRequestAsListResponsesAsync<TRequestBody, TResponses>(TRequestBody body)
                where TRequestBody : IRequestBody
                where TResponses : IResponses
            {
                CheckDone();

                WebRequest.uploadHandler = GetUploadHandler(body);
                WebRequest.downloadHandler = new DownloadHandlerBuffer();

                await WebRequest.SendWebRequest();

                return ParseResult(GetListResponce<TResponses>);
            }

            public void Dispose()
            {
                ((IDisposable)WebRequest).Dispose();
            }
        }

        public static string ServerUrl => "http://127.0.0.1:7860";

        private static string ContentType => "Content-Type";
        private static string ApplicationJson => "application/json";

        public static class Get
        {
            public static string Method => UnityWebRequest.kHttpVerbGET;

            public class AppId : WebRequestWrapper
            {
                // static
                public static string Paths => "/app_id";
                public static string Url => $"{ServerUrl}{Paths}";
                public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                static AppId()
                {
                    RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                }

                // interface
                public interface IUrl
                {
                    public string Url { get; }
                }

                // internal class
                [Serializable]
                public class Responses : IResponses
                {
                    public string app_id;
                }

                // method
                public AppId() : base(Url, Method, RequestHeaderList) { }
                public AppId(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                public ValueTask<Responses> SendRequestAsync()
                {
                    return base.SendRequestAsync<Responses>();
                }
            }

            public static class SdApi
            {
                public static class V1
                {
                    public class CmdFlags : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/cmd-flags";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static CmdFlags()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class Responses : IResponses
                        {
                            public bool f;
                            public bool update_all_extensions;
                            public bool skip_python_version_check;
                            public bool skip_torch_cuda_test;
                            public bool reinstall_xformers;
                            public bool reinstall_torch;
                            public bool update_check;
                            public string tests;
                            public bool no_tests;
                            public bool skip_install;
                            public string data_dir;
                            public string config;
                            public string ckpt;
                            public string ckpt_dir;
                            public string vae_dir;
                            public string gfpgan_dir;
                            public string gfpgan_model;
                            public bool no_half;
                            public bool no_half_vae;
                            public bool no_progressbar_hiding;
                            public int max_batch_count;
                            public string embeddings_dir;
                            public string textual_inversion_templates_dir;
                            public string hypernetwork_dir;
                            public string localizations_dir;
                            public bool allow_code;
                            public bool medvram;
                            public bool lowvram;
                            public bool lowram;
                            public bool always_batch_cond_uncond;
                            public bool unload_gfpgan;
                            public string precision;
                            public bool upcast_sampling;
                            public bool share;
                            public string ngrok;
                            public string ngrok_region;
                            public bool enable_insecure_extension_access;
                            public string codeformer_models_path;
                            public string gfpgan_models_path;
                            public string esrgan_models_path;
                            public string bsrgan_models_path;
                            public string realesrgan_models_path;
                            public string clip_models_path;
                            public bool xformers;
                            public bool force_enable_xformers;
                            public bool xformers_flash_attention;
                            public bool deepdanbooru;
                            public bool opt_split_attention;
                            public bool opt_sub_quad_attention;
                            public int sub_quad_q_chunk_size;
                            public string sub_quad_kv_chunk_size;
                            public string sub_quad_chunk_threshold;
                            public bool opt_split_attention_invokeai;
                            public bool opt_split_attention_v1;
                            public bool opt_sdp_attention;
                            public bool opt_sdp_no_mem_attention;
                            public bool disable_opt_split_attention;
                            public bool disable_nan_check;
                            // public string[] use_cpu;
                            public bool listen;
                            public string port;
                            public bool show_negative_prompt;
                            public string ui_config_file;
                            public bool hide_ui_dir_config;
                            public bool freeze_settings;
                            public string ui_settings_file;
                            public bool gradio_debug;
                            public string gradio_auth;
                            public string gradio_auth_path;
                            public string gradio_img2img_tool;
                            public string gradio_inpaint_tool;
                            public bool opt_channelslast;
                            public string styles_file;
                            public bool autolaunch;
                            public string theme;
                            public bool use_textbox_seed;
                            public bool disable_console_progressbars;
                            public bool enable_console_prompts;
                            public bool vae_path;
                            public bool disable_safe_unpickle;
                            public bool api;
                            public string api_auth;
                            public bool api_log;
                            public bool nowebui;
                            public bool ui_debug_mode;
                            public string device_id;
                            public bool administrator;
                            public string cors_allow_origins;
                            public string cors_allow_origins_regex;
                            public string tls_keyfile;
                            public string tls_certfile;
                            public string server_name;
                            public bool gradio_queue;
                            public bool no_gradio_queue;
                            public bool skip_version_check;
                            public bool no_hashing;
                            public bool no_download_sd_model;
                            public string controlnet_dir;
                            public string controlnet_annotator_models_path;
                            public string no_half_controlnet;
                            public string ldsr_models_path;
                            public string lora_dir;
                            public string scunet_models_path;
                            public string swinir_models_path;
                        }

                        // method
                        public CmdFlags() : base(Url, Method, RequestHeaderList) { }
                        public CmdFlags(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public ValueTask<Responses> SendRequestAsync()
                        {
                            return base.SendRequestAsync<Responses>();
                        }
                    }

                    public class SdModels : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/sd-models";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static SdModels()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string title;
                            public string model_name;
                            public string hash;
                            public string sha256;
                            public string filename;
                            public string config;
                        }

                        // method
                        public SdModels() : base(Url, Method, RequestHeaderList) { }
                        public SdModels(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public ValueTask<IReadOnlyList<Responses>> SendRequestAsync()
                        {
                            return base.SendRequestAsListResponsesAsync<Responses>();
                        }
                    }

                    public class Progress : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/progress";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Progress()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class Parameters : IParameters
                        {
                            public bool skip_current_image;
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public float progress = 0;
                            public float eta_relative = 0;
                            //public XXX state = { };
                            public string current_image = "string";
                            public string textinfo = "string";
                        }

                        // method
                        public Progress() : base(Url, Method, RequestHeaderList) { }
                        public Progress(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public ValueTask<Responses> SendRequestAsync()
                        {
                            return base.SendRequestAsync<Responses>();
                        }
                    }
                }
            }
        }

        public static class Post
        {
            public static string Method = UnityWebRequest.kHttpVerbPOST;

            public static class SdApi
            {
                public static class V1
                {
                    public class Options : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/options";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Options()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public string sd_model_checkpoint;
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            
                        }

                        // method
                        public Options() : base(Url, Method, RequestHeaderList) { }
                        public Options(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody()
                        {
                            return new RequestBody();
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }

                    public class Txt2Img : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/txt2img";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Txt2Img()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public interface IDefault
                            {
                                public string Sampler { get; }
                                public int Width { get; }
                                public int Height { get; }
                                public int Steps { get; }
                                public float CfgScale { get; }
                                public long Seed { get; }
                            }

                            public string sampler_index = "Euler a";
                            public string prompt = "";
                            public string negative_prompt = "";
                            public long seed = -1;
                            public int steps = 20;
                            public float cfg_scale = 7;
                            public int width = 960;
                            public int height = 540;
                            public float denoising_strength = 0.0f;

                            public RequestBody(IDefault def)
                            {
                                sampler_index = def.Sampler;
                                width = def.Width;
                                height = def.Height;
                                seed = def.Seed;
                                steps = def.Steps;
                                cfg_scale = def.CfgScale;
                            }
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string[] images;

                            public byte[] GetImage()
                            {
                                return GetImageByteArray(images);
                            }
                        }

                        // method
                        public Txt2Img() : base(Url, Method, RequestHeaderList) { }
                        public Txt2Img(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody(RequestBody.IDefault def)
                        {
                            return new RequestBody(def);
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }

                    public class Img2Img : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/img2img";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static Img2Img()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public interface IDefault
                            {
                                public string Sampler { get; }
                                public int Width { get; }
                                public int Height { get; }
                                public int Steps { get; }
                                public float CfgScale { get; }
                                public long Seed { get; }
                            }

                            public string[] init_images;
                            public string sampler_index = "Euler a";
                            public string prompt = "";
                            public string negative_prompt = "";
                            public long seed = -1;
                            public int steps = 20;
                            public float cfg_scale = 7;
                            public int width = 960;
                            public int height = 540;
                            public float denoising_strength = 0.75f;

                            public RequestBody(IDefault def)
                            {
                                sampler_index = def.Sampler;
                                width = def.Width;
                                height = def.Height;
                                seed = def.Seed;
                                steps = def.Steps;
                                cfg_scale = def.CfgScale;
                            }

                            public void SetImage(byte[] data)
                            {
                                init_images = GetImageStringArray(data);
                            }
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string[] images;

                            public byte[] GetImage()
                            {
                                return GetImageByteArray(images);
                            }
                        }

                        // method
                        public Img2Img() : base(Url, Method, RequestHeaderList) { }
                        public Img2Img(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody(RequestBody.IDefault def)
                        {
                            return new RequestBody(def);
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }

                    public class PngInfo : WebRequestWrapper
                    {
                        // static
                        public static string Paths => "/sdapi/v1/png-info";
                        public static string Url => $"{ServerUrl}{Paths}";
                        public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                        static PngInfo()
                        {
                            RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                        }

                        // interface
                        public interface IUrl
                        {
                            public string Url { get; }
                        }

                        // internal class
                        [Serializable]
                        public class RequestBody : IRequestBody
                        {
                            public string image;

                            public void SetImage(byte[] data)
                            {
                                image = GetImageString(data);
                            }
                        }

                        [Serializable]
                        public class Responses : IResponses
                        {
                            public string info;

                            public IReadOnlyDictionary<string, string> Parse()
                            {
                                var dic = new Dictionary<string, string>();

                                var lines = info.Split('\n');

                                if (lines.Length >= 2)
                                {
                                    dic.Add("Prompt", lines[0]);

                                    var items = lines[1].Split(", ");

                                    foreach (var item in items)
                                    {
                                        var split = item.Split(": ");

                                        if (split.Length >= 2)
                                        {
                                            dic.Add(split[0], split[1]);
                                        }
                                    }
                                }

                                return dic;
                            }
                        }

                        // method
                        public PngInfo() : base(Url, Method, RequestHeaderList) { }
                        public PngInfo(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                        public RequestBody GetRequestBody()
                        {
                            return new RequestBody();
                        }

                        public ValueTask<Responses> SendRequestAsync(RequestBody body)
                        {
                            return base.SendRequestAsync<RequestBody, Responses>(body);
                        }
                    }
                }
            }

            public static class ControlNet
            {
                public class Txt2Img : WebRequestWrapper
                {
                    // static
                    public static string Paths => "/controlnet/txt2img";
                    public static string Url => $"{ServerUrl}{Paths}";
                    public static IReadOnlyList<RequestHeader> RequestHeaderList { get; }

                    static Txt2Img()
                    {
                        RequestHeaderList = new List<RequestHeader>() { new RequestHeader(ContentType, ApplicationJson), };
                    }

                    // interface
                    public interface IUrl
                    {
                        public string Url { get; }
                    }

                    // internal class
                    [Serializable]
                    public class RequestBody : IRequestBody
                    {
                        public interface IDefault
                        {
                            public string Sampler { get; }
                            public int Width { get; }
                            public int Height { get; }
                            public int Steps { get; }
                            public float CfgScale { get; }
                            public long Seed { get; }
                        }

                        public string[] controlnet_input_image;
                        public string controlnet_module = "none";
                        public string controlnet_model = "control_v11f1p_sd15_depth_fp16 [4b72d323]";
                        public string sampler_index = "Euler a";
                        public float controlnet_weight = 1.0f;
                        public string prompt = "";
                        public string negative_prompt = "";
                        public long seed = -1;
                        public int steps = 20;
                        public float cfg_scale = 7;
                        public int width = 960;
                        public int height = 540;
                        public float denoising_strength = 0.0f;

                        public RequestBody(IDefault def)
                        {
                            sampler_index = def.Sampler;
                            width = def.Width;
                            height = def.Height;
                            seed = def.Seed;
                            steps = def.Steps;
                            cfg_scale = def.CfgScale;
                        }

                        public void SetImage(byte[] data)
                        {
                            controlnet_input_image = GetImageStringArray(data);
                        }
                    }

                    [Serializable]
                    public class Responses : IResponses
                    {
                        public string[] images;

                        public byte[] GetImage()
                        {
                            return GetImageByteArray(images);
                        }
                    }

                    // method
                    public Txt2Img() : base(Url, Method, RequestHeaderList) { }
                    public Txt2Img(IUrl url) : base(url.Url, Method, RequestHeaderList) { }

                    public RequestBody GetRequestBody(RequestBody.IDefault def)
                    {
                        return new RequestBody(def);
                    }

                    public ValueTask<Responses> SendRequestAsync(RequestBody body)
                    {
                        return base.SendRequestAsync<RequestBody, Responses>(body);
                    }
                }
            }
        }
    }
}
