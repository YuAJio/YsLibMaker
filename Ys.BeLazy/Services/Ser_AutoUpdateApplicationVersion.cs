using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Newtonsoft.Json.Linq;
using YS_PublicMilf_Lite.Stuff_CupCake;

namespace Ys.BeLazy.Services
{
    /// <summary>
    /// 自动更新app服务
    /// </summary>
    [Service]
    public class Ser_AutoUpdateApplicationVersion : Service
    {
        public const string TAG_ASSFILENAME = "UpdateAppConfig.json";
        public const string TAG_BROADCASTACTION = "KEY_BCACTION";
        public const string TAG_DOWNLOAD_LOCAL_FILE_NAME = "KEY_LOCALFILENAME";
        public const string TAG_DOWNLOAD_PLANTFORM_URL = "KEY_PLANTFORM_URL";
        public const string TAG_BROADCAST_INTENT_ISSUCEESS = "KEY_ISSUCEESS";
        public const string TAG_BROADCAST_INTENT_ENUMSTATE = "KEY_STATE";
        public const string TAG_BROADCAST_INTENT_MESSAGE = "KEY_MESSAGE";

        public enum AutoUpdateResultState
        {
            Succees,
            CheckUpdateException,
            RequestNewVersionFaliled,
            StartDownloadException,
            DownloadUrlIsNull,
            SaveFilePathIsNull,
            DownloadFileLengthIsZero,
            FileAlreadyExist,
        }

        private const string TAG_BELAZYDIRPATH = "LazyBreezy";
        private const string TAG_BELAZYDOWNLOADDIRPATH = "Download";
        private const string TAG_BELAZYDOWNLOADFILENAME = "UpdateApk.apk";
        /// <summary>
        /// 备选下载文件名称
        /// </summary>
        private static string TAG_APPOINTDOWNLOADFILENAME = "";

        private string BroadcastAction = "";
        private string PlantformUrl = "";


        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent == null)
            {
                StopSelf();
                return base.OnStartCommand(intent, flags, startId);
            }
            BroadcastAction = intent.GetStringExtra(TAG_BROADCASTACTION);
            if (string.IsNullOrEmpty(BroadcastAction))
            {
                StopSelf();
                return base.OnStartCommand(intent, flags, startId);
            }

            PlantformUrl = intent.GetStringExtra(TAG_DOWNLOAD_PLANTFORM_URL);

            var localFileName = intent.GetStringExtra(TAG_DOWNLOAD_LOCAL_FILE_NAME);
            if (!string.IsNullOrEmpty(localFileName))
            {
                if (!localFileName.EndsWith(".apk"))
                    TAG_APPOINTDOWNLOADFILENAME = localFileName + ".apk";
                else
                    TAG_APPOINTDOWNLOADFILENAME = localFileName;
            }

            try
            {
                CheckUchengUpdate();
            }
            catch (Exception ex)
            {
                StopSelf();
            }
            return base.OnStartCommand(intent, flags, startId);
        }


        private static string DownLoadLocalPath = "";
        private void CheckUchengUpdate()
        {
            var url = "";
            var apktype = 0;
            Task.Run(async () =>
            {
                var requestMd = GetDownloadUrlFromAss();
                if (requestMd == null)
                    return CK_Result<Md_CheckUpdate>.Error<Md_CheckUpdate>();

                DownLoadLocalPath = GetDownloadPathWithDeleteFile();
                url = $"{requestMd.RequestUrl}?apktype={requestMd.ApkType}&platUrl={PlantformUrl}";
                var parmas = new
                {
                    apktype = requestMd.ApkType,
                    platUrl = PlantformUrl
                };
                apktype = parmas.apktype;
                return await YS_PublicMilf_Lite.Stuff_Http.HttpClientProxy.PostAsync<CK_Result<Md_CheckUpdate>>(url, parmas);
            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    SendBroadcast(false, AutoUpdateResultState.CheckUpdateException, $"CheckUpdateException :{x.Exception.Message.ToJson()}");
                    StopSelf();
                    return;
                }
                if (x.Result.IsSuccess)
                {//访问成功
                    var data = x.Result.Data;
                    var versionCode = GetAppVersionCode();
                    if (data.Version > versionCode)
                    {//如果版本号大于当前版本号
                        StartDownload(data.Url);
                    }
                }
                else
                {
                    SendBroadcast(false, AutoUpdateResultState.RequestNewVersionFaliled, $"RequestNewVersionFaliled : \n Url:{url}\n ApkType:{apktype}\n PlantformUrl:{PlantformUrl}\n Message:{ x.Result.Message}");
                    StopSelf();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void StartDownload(string url)
        {
            Task.Run(async () =>
            {
                var fileInfo = new FileInfo(DownLoadLocalPath);
                if (!fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();

                if (File.Exists(DownLoadLocalPath))
                    File.Delete(DownLoadLocalPath);

                var downloader = new YS_PublicMilf_Lite.Stuff_Http.DownloadHelper();
                var result = await downloader.DownloadFileForResultAsync(url, DownLoadLocalPath);
                return result;
            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    SendBroadcast(false, AutoUpdateResultState.StartDownloadException, $"StartDownloadException :{x.Exception.Message.ToJson()}");
                    return;
                }
                switch (x.Result)
                {
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.DownloadUrlIsNull:
                        {
                            SendBroadcast(false, AutoUpdateResultState.DownloadUrlIsNull, "DownloadUrlIsNull");
                            StopSelf();
                        }
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.SaveFilePathIsNull:
                        {
                            SendBroadcast(false, AutoUpdateResultState.SaveFilePathIsNull, "SaveFilePathIsNull");
                            StopSelf();
                        }
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.DownloadFileLengthIsZero:
                        {
                            SendBroadcast(false, AutoUpdateResultState.DownloadFileLengthIsZero, "DownloadFileLengthIsZero");
                            StopSelf();
                        }
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.FileAlreadyExist:
                        {
                            SendBroadcast(false, AutoUpdateResultState.FileAlreadyExist, "FileAlreadyExist");
                            StopSelf();
                        }
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.DownloadComplete:
                        {
                            if (File.Exists(DownLoadLocalPath))
                            {
                                SendBroadcast(true, AutoUpdateResultState.Succees, "DownloadOk");
                                StopSelf();
                            }
                        }
                        break;
                }

            });

        }


        /// <summary>
        /// 从Ass文件中获取下载地址
        /// </summary>
        /// <returns></returns>
        private Md_DownloadConfig GetDownloadUrlFromAss()
        {
            var sb = new StringBuilder();
            using (var stream = Assets.Open("UpdateAppConfig.json", Android.Content.Res.Access.Buffer))
            {
                var count = 0;
                var buffer = new byte[1024];


                do
                {
                    try
                    {
                        count = stream.Read(buffer, 0, buffer.Length);
                        if (count != 0)
                            sb.Append(Encoding.UTF8.GetString(buffer, 0, count));
                    }
                    catch (Exception ex)
                    {
                        sb = new StringBuilder();
                        break;
                    }
                } while (count > 0);

                var content = sb.ToString();
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        var js = content.ToObject<JToken>();
                        var jk = new Md_DownloadConfig { ApkType = js.Value<int>("ApkType"), RequestUrl = js.Value<string>("RequestUrl") };
                        return jk;
                    }
                    catch (System.Exception ex)
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        private int GetAppVersionCode()
        {
            var packInfo = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.Services);
            return packInfo.VersionCode;
        }

        private string GetDownloadPathWithDeleteFile()
        {
            var fileName = string.IsNullOrEmpty(TAG_APPOINTDOWNLOADFILENAME) ? TAG_BELAZYDOWNLOADFILENAME : TAG_APPOINTDOWNLOADFILENAME;
            var path = Path.Combine(
                Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                TAG_BELAZYDIRPATH,
                TAG_BELAZYDOWNLOADDIRPATH,
                fileName
                );
            var fileInfo = new FileInfo(path);
            if (fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            if (File.Exists(path))
                File.Delete(path);

            return path;
        }

        private void SendBroadcast(bool isOk, AutoUpdateResultState state, string message)
        {
            var intent_2 = new Intent();
            intent_2.SetAction(BroadcastAction);
            intent_2.PutExtra(TAG_BROADCAST_INTENT_ISSUCEESS, isOk);
            intent_2.PutExtra(TAG_BROADCAST_INTENT_ENUMSTATE, (int)state);
            intent_2.PutExtra(TAG_BROADCAST_INTENT_MESSAGE, message);
            if (Build.VERSION.SdkInt > BuildVersionCodes.OMr1)
                intent_2.SetPackage(PackageName);
            SendBroadcast(intent_2);
        }

        public static string DownloadPath
        {
            get
            {
                var fileName = string.IsNullOrEmpty(TAG_APPOINTDOWNLOADFILENAME) ? TAG_BELAZYDOWNLOADFILENAME : TAG_APPOINTDOWNLOADFILENAME;
                if (string.IsNullOrEmpty(DownLoadLocalPath))
                    return Path.Combine(
            Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
            TAG_BELAZYDIRPATH,
            TAG_BELAZYDOWNLOADDIRPATH,
            fileName
            );
                else
                    return DownLoadLocalPath;
            }
        }


        private class Md_DownloadConfig
        {

            /// <summary>
            /// 
            /// </summary>
            public string RequestUrl { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int ApkType { get; set; }
        }

        public class Md_CheckUpdate
        {
            public string Md5 { get; set; }
            public DateTime Date { get; set; }
            public string Url { get; set; }
            public int Version { get; set; }
        }
    }
}