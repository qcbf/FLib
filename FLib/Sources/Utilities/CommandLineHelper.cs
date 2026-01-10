//==================={By Qcbf|qcbf@qq.com|2/28/2022 4:52:14 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FLib
{
    public static class CommandLineHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public static void Execute(Type commandHandlerType, string[] args, object commandHandler = null, Action defaultMethod = null)
        {
            try
            {
                string methodName = null;
                for (var i = 0; i < args.Length; i++)
                {
                    var item = args[i];
                    if (item.StartsWith("--"))
                    {
                        var fieldName = item[2..];
                        var fieldValueStr = args.ElementAtOrDefault(++i) ?? string.Empty;
                        var field = commandHandlerType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Static) ?? throw new Exception("not found field[" + fieldName + ']');
                        object obj;
                        try
                        {
                            obj = Convert.ChangeType(fieldValueStr, field.FieldType);
                        }
                        catch
                        {
                            obj = Json5.Deserialize(fieldValueStr, field.FieldType);
                        }
                        field.SetValue(commandHandler, obj);
                    }
                    else if (item == "help")
                    {
                        Log.Info?.Write(GetHelp(commandHandlerType, commandHandler), nameof(CommandLineHelper));
                    }
                    else
                    {
                        methodName = item.ToLowerInvariant();
                    }
                }
                if (methodName == null)
                {
                    defaultMethod?.Invoke();
                }
                else
                {
                    var method = commandHandlerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (method?.IsDefined(typeof(CommentAttribute)) != true) throw new Exception("not found method: " + methodName);
                    method.Invoke(commandHandler, null);
                }
            }
            catch (Exception ex)
            {
                Log.Error?.Write(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static StringBuilder GetHelp(Type commandHandlerType, object commandHandler)
        {
            var strbuf = new StringBuilder();
            strbuf.AppendLine("").AppendLine("fields:");
            foreach (var item in commandHandlerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                var comment = item.GetCustomAttribute<CommentAttribute>(false);
                if (comment != null)
                {
                    strbuf.Append('\t').Append("--").Append(item.Name).Append('\t').Append(comment.Name)
                        .Append('[').Append("Default: ").Append(item.GetValue(commandHandler)).Append(']');
                    strbuf.AppendLine();
                }
            }

            strbuf.AppendLine().AppendLine("methods:");
            foreach (var item in commandHandlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var comment = item.GetCustomAttribute<CommentAttribute>(false);
                if (comment != null)
                {
                    strbuf.Append('\t').Append("--").Append(item.Name).Append('\t').Append(comment.Name);
                    strbuf.AppendLine();
                }
            }

            return strbuf;
        }
    }
}
