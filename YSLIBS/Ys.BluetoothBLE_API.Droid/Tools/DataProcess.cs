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
            for (int i = index; i < hexData.Length; i++)
            {
                var positionStr = hexData[i] + hexData[i + 1];
                i += 2;
                var heightStr = hexData[i] + hexData[i + 1];
                i += 2;
                var areaStr = hexData[i] + hexData[i + 1] + hexData[i + 2];
                i += 2;
                resultItemList.Add(new ResultModel
                {
                    Area = Java.Lang.Integer.ParseInt(areaStr, 16),
                    Height = Java.Lang.Integer.ParseInt(heightStr, 16),
                    Position = Java.Lang.Integer.ParseInt(positionStr, 16)
                });
            }

            var dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(resultItemList);

            //获取检测标准值属性  标准值后面的resultItem都是样品检测数值
            //比如我要检测大白菜，样品大白菜
            //但是否大白菜是合格的，要基于试纸条的标准数值来进行比较。
            var ctResultItem = resultItemList[0];
            resultItemList.RemoveAt(0);

            DetectResult totalResult = DetectResult.Negative;
            resultItemList.ForEach(testResultItem =>
            {
                var positiveVal = 1.1f;
                var negativeVal = 0.7f;
                var compareValArea = testResultItem.Area * 1.0 / ctResultItem.Area;
                var compareValHeight = testResultItem.Height * 1.0 / ctResultItem.Height;

                testResultItem.Value = compareValArea.ToString("N2");
                testResultItem.AreaValue = compareValArea.ToString();

                var hasAnoymosVal = positiveVal != negativeVal;
                DetectResult result;

                if (compareValArea > positiveVal)
                    result = DetectResult.Positive;
                else if (hasAnoymosVal && compareValArea > negativeVal)
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

        public static string ProcessTestDataFromCmd(string[] hexData, int cmd)
        {
            if (cmd == Constants_Republic.TEST_CMD)
            {
                var success = Constants_Republic.RESULT_OK == hexData[0];
                if (!success)
                    return "Test Failed";
                return ProcessDetectResult(hexData, 1);
            }

            if (cmd == Constants_Republic.TEST_WITH_ALL_DATA)
            {
                var success = Constants_Republic.RESULT_OK == hexData[0];
                if (!success)
                    return " Debug Test Failed";

                var quXiandataList = new List<int>();
                int i;
                for (i = 1; i < hexData.Length; i++)
                {
                    var pointStr = hexData[i] + hexData[i + 1];
                    i++;
                    quXiandataList.Add(Java.Lang.Integer.ParseInt(pointStr, 16));
                    if (quXiandataList.Count == 1536)
                        break;
                }
                return ProcessDetectResult(hexData, i + 1);
            }
            return "Not Process Yeat";
        }

    }
}