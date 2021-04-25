using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibMaker.Droid
{
    public class Uri2PathUtil
    {
        public static string GetRealPathFromUri(Context context, Android.Net.Uri uri)
        {
            var sdkVersion = Build.VERSION.SdkInt;
            if (sdkVersion < BuildVersionCodes.Honeycomb) return getRealPathFromUri_BelowApi11(context, uri);
            if (sdkVersion < BuildVersionCodes.Kitkat) return getRealPathFromUri_Api11To18(context, uri);
            else return getRealPathFromUri_AboveApi19(context, uri);
        }

        private static string getRealPathFromUri_BelowApi11(Context context, Android.Net.Uri uri)
        {
            string filePath = null;
            string[] projection = { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
            var cursor = context.ContentResolver.Query(uri, projection, null, null, null);
            if (cursor != null && cursor.MoveToFirst())
            {
                filePath = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                cursor.Close();
            }
            return filePath;
        }

        private static string getRealPathFromUri_Api11To18(Context context, Android.Net.Uri uri)
        {
            string filePath = null;
            string[] projection = { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
            //这个有两个包不知道是哪个。。。。不过这个复杂版一般用不到
            CursorLoader loader = new CursorLoader(context, uri, projection, null, null, null);
            var cursor = loader.LoadInBackground() as Android.Database.ICursor;

            if (cursor != null)
            {
                cursor.MoveToFirst();
                filePath = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                cursor.Close();
            }
            return filePath;
        }

        private static string getRealPathFromUri_AboveApi19(Context context, Android.Net.Uri uri)
        {
            if (DocumentsContract.IsDocumentUri(context, uri))
            {
                if (isExternalStorageDocument(uri))
                {
                    var docId = DocumentsContract.GetDocumentId(uri);
                    var split = docId.Split(":");
                    var type = split[0];
                    if ("primary" == (type))
                        return Environment.ExternalStorageDirectory + "/" + split[1];
                }
                else if (isDownloadsDocument(uri))
                {
                    var id = DocumentsContract.GetDocumentId(uri);
                    var contentUri = ContentUris.WithAppendedId(
                              Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return getDataColumn(context, contentUri, null, null);
                }
                else if (isMediaDocument(uri))
                {
                    var docId = DocumentsContract.GetDocumentId(uri);
                    var split = docId.Split(":");
                    var type = split[0];

                    Uri contentUri;
                    if ("image" == (type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video" == (type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio" == (type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }
                    else
                    {
                        contentUri = MediaStore.Files.GetContentUri("external");
                    }

                    var selection = "_id=?";
                    string[] selectionArgs = new string[] { split[1] };
                    return getDataColumn(context, contentUri, selection, selectionArgs);
                }

            }
            else if ("content" == (uri.Scheme))
            {
                return getDataColumn(context, uri, null, null);
            }
            else if ("file" == (uri.Scheme))
            {
                return uri.Path;
            }
            return null;
        }



        /**
   * Get the value of the data column for this Uri. This is useful for
   * MediaStore Uris, and other file-based ContentProviders.
   *
   * @param context       The context.
   * @param uri           The Uri to query.
   * @param selection     (Optional) Filter used in the query.
   * @param selectionArgs (Optional) Selection arguments used in the query.
   * @return The value of the _data column, which is typically a file path.
   */
        public static string getDataColumn(Context context, Uri uri, string selection,
                                           string[] selectionArgs)
        {
            ICursor cursor = null;
            string column = MediaStore.MediaColumns.Data;
            string[] projection = { column };
            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
                        null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int column_index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(column_index);
                }
            }
            catch (Java.Lang.Exception e)
            {
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        /**
    * @param uri The Uri to check.
    * @return Whether the Uri authority is ExternalStorageProvider.
    */
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents" == (uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is DownloadsProvider.
         */
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents" == (uri.Authority);
        }

        /**
         * @param uri The Uri to check.
         * @return Whether the Uri authority is MediaProvider.
         */
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents" == uri.Authority;
        }
    }
}