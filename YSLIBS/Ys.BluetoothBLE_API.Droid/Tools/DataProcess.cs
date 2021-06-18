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
        public static string ProcessDetectResult(string[] hexData, int index, TestStrip testStrip)
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
                    Name = testStrip.Name,
                    Area = Java.Lang.Integer.ParseInt(areaStr, 16),
                    Height = Java.Lang.Integer.ParseInt(heightStr, 16),
                    Position = Java.Lang.Integer.ParseInt(positionStr, 16)
                });
            }

            //获取检测标准值属性  标准值后面的resultItem都是样品检测数值
            //比如我要检测大白菜，样品大白菜
            //但是否大白菜是合格的，要基于试纸条的标准数值来进行比较。
            var ctResultItem = resultItemList[0];
            resultItemList.RemoveAt(0);

            DetectResult totalResult = DetectResult.Negative;
            for (int i = 0; i < resultItemList.Count; i++)
            {
                var testResultItem = resultItemList[i];
                var positiveVal = testStrip.PositiveValue;
                var negativeVal = testStrip.NegativeValue;
                double compareVal = 0d;
                switch (testStrip.JudgeType)
                {
                    case JudgeType.Area:
                        compareVal = (testResultItem.Area * 1.0) / ctResultItem.Area;
                        break;
                    case JudgeType.Height:
                        compareVal = (testResultItem.Height * 1.0) / ctResultItem.Height;
                        break;
                }
                compareVal = compareVal > 5.0f ? 5.0 : compareVal;
                testResultItem.Value = compareVal.ToString("N2");
                testResultItem.AreaValue = compareVal.ToString();
                testResultItem.Name = testStrip.StripItemList[i >= testStrip.StripItemList.Count ? testStrip.StripItemList.Count - 1 : i].Name;

                var hasAnoymosVal = positiveVal != negativeVal;
                DetectResult result;

                if (testStrip.CTDriection == CTDriection.Positive)
                {
                    if (compareVal > positiveVal)
                        result = DetectResult.Positive;
                    else
                        result = DetectResult.Negative;
                }
                else if (testStrip.CTDriection == CTDriection.Ngative)
                {
                    if (compareVal < positiveVal)
                        result = DetectResult.Positive;
                    else
                        result = DetectResult.Negative;
                }
                else
                    result = DetectResult.DEFAULT;

                testResultItem.Result = (DetectResult)result;
                if ((int)totalResult < (int)result)
                    totalResult = result;
            };
            return ProcessTestString(totalResult, ctResultItem, resultItemList);
        }

        public static string ProcessTestString(DetectResult result, ResultModel textItem, List<ResultModel> resultModels)
        {
            var resultStr = "<文字占位符A>";
            resultStr += GetResultFromEnum(result);
            resultStr += $"{textItem.Name}T/C:\t";
            if (resultModels.Count == 1)
            {
                var firstData = resultModels.FirstOrDefault();
                resultStr += $"{firstData.Value}\t{GetResultFromEnum(firstData.Result)}";
            }
            else
            {
                var tIndex = 1;
                resultModels.ForEach(x =>
                {
                    resultStr += $"T{tIndex}\t{x.Value}\t{GetResultFromEnum(x.Result)}";
                    resultStr += "\n";
                });
            }
            return resultStr;
        }

        public static string GetResultFromEnum(DetectResult detectResult)
        {
            var resultStr = "";
            switch (detectResult)
            {
                case DetectResult.Negative:
                    resultStr += "<Negative>";
                    break;
                case DetectResult.Positive:
                    resultStr += "<Positive>";
                    break;
                case DetectResult.DEFAULT:
                    resultStr += "<IllgelParams>";
                    break;
            }
            return resultStr;
        }

        public static string ProcessTestDataFromCmd(string[] hexData, int cmd, TestStrip testStrip)
        {
            if (cmd == Constants_Republic.TEST_CMD)
            {
                var success = Constants_Republic.RESULT_OK == hexData[0];
                if (!success)
                    return "Test Failed";
                return ProcessDetectResult(hexData, 1, testStrip);
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
                return ProcessDetectResult(hexData, i + 1, testStrip);
            }
            return "Not Process Yeat";
        }

    }
}