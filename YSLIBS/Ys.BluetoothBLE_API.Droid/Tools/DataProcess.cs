using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static Ys.BluetoothBLE_API.Droid.Enum_Republic;
using Ys.BluetoothBLE_API.Droid.Models;

namespace Ys.BluetoothBLE_API.Droid.Tools
{
    public static class DataProcess
    {
        public static string ProcessDetectResult(string[] hexData, int index)
        {
            var resultItemList = new List<ResultModel>();
            for (int i = 0; i < hexData.Length; i++)
            {
                var positionStr = hexData[i] + hexData[i + 1];
                i = i + 2;

                var heightStr = hexData[i] + hexData[i + 1];

                i = i + 2;
                var areaStr = hexData[i] + hexData[i + 1] + hexData[i + 2];

                i = i + 2;
                resultItemList.Add(new ResultModel
                {
                    Area = Java.Lang.Integer.ParseInt(areaStr, 16),
                    Height = Java.Lang.Integer.ParseInt(heightStr, 16),
                    Position = Java.Lang.Integer.ParseInt(positionStr, 16)
                });
            }

            var dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(resultItemList);

            var ctResultItem = resultItemList[0];
            resultItemList.RemoveAt(0);

            DetectResult totalResult = DetectResult.Negative;
            resultItemList.ForEach(testResultItem =>
            {
                var positiveVal = 1.1f;
                var negativeVal = 0.7f;
                var compareVal = testResultItem.Area * 1.0 / ctResultItem.Area;

                testResultItem.Value = compareVal.ToString("N2");
                testResultItem.AreaValue = compareVal.ToString();

                var hasAnoymosVal = positiveVal != negativeVal;
                DetectResult result;

                if (compareVal > positiveVal)
                    result = DetectResult.Positive;
                else if (hasAnoymosVal && compareVal > negativeVal)
                    result = DetectResult.Positive;
                else
                    result = DetectResult.Weakly_reactive;
                testResultItem.Result = (DetectResult)result;
                if ((int)totalResult < (int)result)
                    totalResult = result;
            });
            

            return ProcessTestString(totalResult, ctResultItem);
        }


        public static string ProcessTestString(DetectResult result, ResultModel textItem)
        {
            var resultStr = "<文字占位符A>";

            switch (result)
            {
                case DetectResult.Negative:
                    resultStr += "<Negative>";
                    break;
                case DetectResult.Weakly_reactive:
                    resultStr += "<Weakly_reactive>";
                    break;
                case DetectResult.Positive:
                    resultStr += "<Positive>";
                    break;
            }
            return resultStr;
        }
    }
}