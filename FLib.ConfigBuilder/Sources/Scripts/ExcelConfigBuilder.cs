//==================={By Qcbf|qcbf@qq.com|4/27/2022 4:52:08 PM}===================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using ExcelDataReader;

#pragma warning disable CA1050
namespace FLib
{
    /// <summary>
    /// <code>
    /// FieldName: id        Name
    /// Sign:                  c
    /// Comment:              名称
    /// Default:           Not Found
    /// Content:   1001       ABC
    /// Content:   1002       XXX
    /// </code>
    /// </summary>
    public class ExcelConfigBuilder : ConfigBuilder.IBuildable
    {
        // [ThreadStatic] private static ExcelPackage _excelBuffer;
        public string Extension => ".xlsx";

        public void Build(in ConfigBuilder.TableContext ctx)
        {
            using var stream = File.Open(ctx.SourceFile.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            
            // 特殊行
            var lineIndexTemp = 0;
            var fieldNameLineIndex = lineIndexTemp++;
            var signLineIndex = lineIndexTemp++;
            var commentLineIndex = lineIndexTemp++;
            var defaultValueLineIndex = lineIndexTemp;

            var fields = ctx.ConfigType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).ToDictionary(k => k.Name, v => v);
            var fieldObjectValues = new Dictionary<string, object>();

            string[] fieldNames = null;
            string[] defaultValues = null;
            char[] signs = null;

            var rows = reader.RowCount;
            var cols = reader.FieldCount;
            ctx.EnsureCapacity(rows - 4);
            var row = 0;
            while (reader.Read())
            {
                if (row == fieldNameLineIndex)
                {
                    while (reader.IsDBNull(cols - 1))
                        --cols;
                    fieldNames = new string[cols];
                    var emptyFieldCount = 0;
                    for (var i = 0; i < fieldNames.Length; i++)
                    {
                        ref var fieldName = ref fieldNames[i];
                        fieldName = reader.GetValue(i)?.ToString().Trim() ?? string.Empty;
                        if (fieldName.Length == 0)
                            emptyFieldCount++;
                        else
                            emptyFieldCount = 0;
                    }
                    if (emptyFieldCount > 0)
                        Array.Resize(ref fieldNames, fieldNames.Length - emptyFieldCount);
                    fieldObjectValues ??= fieldNames.ToDictionary(k => k, v => (object)v);
                }
                else if (row == commentLineIndex)
                {
                }
                else if (row == defaultValueLineIndex)
                {
                    defaultValues = new string[fieldNames.Length];
                    for (var col = 0; col < defaultValues.Length; col++)
                    {
                        if (fieldNames[col].Length > 0)
                            defaultValues[col] = reader.GetValue(col)?.ToString() ?? string.Empty;
                    }
                }
                else if (row == signLineIndex)
                {
                    signs = new char[fieldNames.Length];
                    for (var col = 0; col < fieldNames.Length; col++)
                    {
                        if (fieldNames[col].Length > 0)
                            signs[col] = reader.GetValue(col)?.ToString().SingleOrDefault() ?? char.MinValue;
                    }

                    if (!ConfigBuilderUtility.CheckSign(signs[0], ConfigBuilder.Sign))
                        break;
                }
                else
                {
                    object key = null;
                    var config = (IBytesPackable)TypeAssistant.New(ctx.ConfigType);
                    try
                    {
                        var isExistObjectField = false;
                        for (var col = 0; col < fieldNames.Length; col++)
                        {
                            var fieldName = fieldNames[col];
                            if (fieldName.Length == 0) continue;

                            try
                            {
                                var rawValue = reader.GetValue(col)?.ToString() ?? string.Empty;
                                // if (cellValue.Comment?.Text.First() == '&') // ExcelDataReader不支持备注
                                //     rawValue += cellValue.Comment.Text[1..];
                                if (string.IsNullOrWhiteSpace(rawValue))
                                {
                                    if (col == 0)
                                    {
                                        config = null;
                                        break;
                                    }

                                    rawValue = defaultValues[col];
                                }

                                if (col == 0)
                                {
                                    var field = fields.GetValueOrDefault(fieldName);
                                    Log.AssertNotNull(field)?.Write($"not found key: {fieldName}");
                                    key = ConfigBuilderUtility.ConvertStringToType(rawValue, field.FieldType);
                                    Log.AssertNotNull(key)?.Write($"not found key: {key}");
                                    field.SetValue(config, key);
                                }
                                else if (!string.IsNullOrWhiteSpace(rawValue))
                                {
                                    if (string.IsNullOrEmpty(rawValue) || !ConfigBuilderUtility.CheckSign(signs[col], ConfigBuilder.Sign))
                                        continue;
                                    if (TrySetObjectFieldValue(fieldObjectValues, fieldName, rawValue))
                                    {
                                        isExistObjectField = true;
                                    }
                                    else
                                    {
                                        if (fields.TryGetValue(fieldName, out var field))
                                            field.SetValue(config, ConfigBuilderUtility.ConvertStringToType(rawValue, field.FieldType));
                                        else
                                            throw new Exception("not found class field: " + fieldName);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"parse field [num:{col}, {fieldName}] error\n" + ex);
                            }
                        }

                        if (isExistObjectField)
                        {
                            foreach (var item in fieldObjectValues)
                            {
                                if (!fields.TryGetValue(item.Key, out var field))
                                    throw new Exception($"not found class field: {item.Key}");
                                var json = Json5.Serialize(item.Value, EJson5SerializeOption.RetainString | EJson5SerializeOption.DictDontWriteEmptyKeyWithColonChar);
                                var val = Json5.Deserialize(json, field.FieldType);
                                field.SetValue(config, val);
                            }
                            fieldObjectValues.Clear();
                        }

                        if (config != null)
                        {
                            ctx.AddConfig(key, config);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error?.Write($"{ctx} line[num:{row}, key:{key}]\n{ex}");
                    }
                }
                ++row;
            }
        }

        /// <summary>
        /// <para>特殊规则处理:</para>
        /// <para>字段名1.字段名2.字段名3 = {字段名1:{字段名2:{字段名3:单元格内容}}}</para>
        /// <para>字段名[] = 字段名:[[单元格],[单元格],[单元格]]</para>
        /// <para>字段名" = "单元格"</para>
        /// <para>字段名 单元格内容key:单元格内容|单元格内容key:单元格内容 = 字段名:{单元格内容key:单元格内容}</para>
        /// </summary>
        private static bool TrySetObjectFieldValue(Dictionary<string, object> fields, string fieldPath, string rawValue)
        {
            var fieldNameSegmentStartIndex = 0;
            for (var i = 0; i < fieldPath.Length; i++)
            {
                if (fieldPath[i] == '.')
                {
                    var segmentFieldName = fieldPath.Substring(fieldNameSegmentStartIndex, i - fieldNameSegmentStartIndex);
                    fieldNameSegmentStartIndex = i + 1;
                    if (!fields.TryGetValue(segmentFieldName, out var temp))
                        fields.Add(segmentFieldName, temp = new Dictionary<string, object>());
                    fields = (Dictionary<string, object>)temp;
                }
            }
            var endFieldName = fieldPath[fieldNameSegmentStartIndex..];
            if (endFieldName.EndsWith("[]"))
            {
                endFieldName = endFieldName[..^2];
                if (!fields.TryGetValue(endFieldName, out var arr))
                    fields.Add(endFieldName, arr = new List<string>());
                ((IList)arr).Add(WrapArray(rawValue));
                return true;
            }

            if (fieldNameSegmentStartIndex == 0)
                return false;
            if (endFieldName.EndsWith('"'))
            {
                endFieldName = endFieldName[..^1];
                rawValue = $"\"{rawValue}\"";
            }
            if (endFieldName.Length == 0)
            {
                var skipIndex = rawValue.IndexOf(':');
                if (skipIndex > 0)
                {
                    var key = rawValue[..skipIndex];
                    var value = rawValue[(skipIndex + 1)..];
                    if (fields.TryGetValue(key, out var oldValue))
                    {
                        if (oldValue is not List<string> list)
                            fields[key] = list = new List<string>() { WrapArray((string)oldValue) };
                        list.Add(WrapArray(value));
                    }
                    else
                    {
                        fields[key] = value;
                    }
                }
                else
                {
                    fields[endFieldName] = rawValue;
                }
            }
            else
            {
                fields[endFieldName] = rawValue;
            }
            return true;

            static string WrapArray(string str) => StringFLibUtility.ReleaseStrBufAndResult(StringFLibUtility.GetStrBuf(str.Length + 2).Append('[').Append(str).Append(']'));
        }
    }
}
