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
using Android.Support.V4.Content;
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

        private const string TAG_BELAZYDIRPATH = "LazyBreezy";
        private const string TAG_BELAZYDOWNLOADDIRPATH = "Download";
        private const string TAG_BELAZYDOWNLOADFILENAME = "UpdateApk.apk";

        private string BroadcastAction = "";


        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            BroadcastAction = intent.GetStringExtra(TAG_BROADCASTACTION);

            CheckUchengUpdate();
            return base.OnStartCommand(intent, flags, startId);
        }


        private string DownLoadLocalPath = "";
        private void CheckUchengUpdate()
        {
            Task.Run(async () =>
            {
                var requestMd = GetDownloadUrlFromAss();
                if (requestMd == null)
                    return CK_Result<Md_CheckUpdate>.Error<Md_CheckUpdate>();

                DownLoadLocalPath = GetDownloadPath(); ;
                var url = $"{requestMd.RequestUrl}?apktype={requestMd.ApkType}";
                var parmas = new
                {
                    apktype = requestMd.ApkType
                };
                return await YS_PublicMilf_Lite.Stuff_Http.HttpClientProxy.PostAsync<CK_Result<Md_CheckUpdate>>(url, parmas);
            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                    return;
                if (x.Result.IsSuccess)
                {//访问成功
                    var data = x.Result.Data;
                    var versionCode = GetAppVersionCode();
                    if (data.Version > versionCode)
                    {//如果版本号大于当前版本号
                        StartDownload(data.Url);
                    }

                }

            });
        }

        private void StartDownload(string url)
        {
            Task.Factory.StartNew(async () =>
            {
                var fileInfo = new FileInfo(DownLoadLocalPath);
                if (!fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();

                if (File.Exists(DownLoadLocalPath))
                    File.Delete(DownLoadLocalPath);

                var downloader = new YS_PublicMilf_Lite.Stuff_Http.DownloadHelper();
                return await downloader.DownloadFileForResultAsync(url, DownLoadLocalPath);
            }).ContinueWith(x =>
            {
                if (x.Exception != null)
                {
                    return;
                }

                switch (x.Result.Result)
                {
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.DownloadUrlIsNull:
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.SaveFilePathIsNull:
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.DownloadFileLengthIsZero:
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.FileAlreadyExist:
                        break;
                    case YS_PublicMilf_Lite.Stuff_Http.DownloadHelper.DownloadState.DownloadComplete:
                        {
                            if (File.Exists(DownLoadLocalPath))
                            {
                                var intent_2 = new Intent();
                                intent_2.SetAction(BroadcastAction);
                                intent_2.PutExtra("result", true);
                                if (Build.VERSION.SdkInt > BuildVersionCodes.OMr1)
                                    intent_2.SetPackage(PackageName);
                                SendBroadcast(intent_2);
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
                    count = stream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, count));
                } while (count > 0);

                var content = sb.ToString();
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        var jk = Newtonsoft.Json.JsonConvert.DeserializeObject<Md_DownloadConfig>(content);
                        //var jk = new Md_DownloadConfig { ApkType = 1, RequestUrl = "asdasdasfasjjfhasjkfda" }.ToJson();
                        //var js = jk.ToObject<Md_DownloadConfig>();
                        //var jss = content.Replace("\r\n", "").Replace(" ", "");
                        //var obj = jss.ToObject<Md_DownloadConfig>();
                        //return obj;
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

        private string GetDownloadPath()
        {
            var path = Path.Combine(
                Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                TAG_BELAZYDIRPATH,
                TAG_BELAZYDOWNLOADDIRPATH,
                TAG_BELAZYDOWNLOADFILENAME
                );
            var fileInfo = new FileInfo(path);
            if (fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            if (File.Exists(path))
                File.Delete(path);

            return path;
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