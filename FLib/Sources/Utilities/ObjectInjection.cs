//==================={By Qcbf|qcbf@qq.com|9/7/2023 10:05:56 AM}===================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class ObjectInjectToAttribute : Attribute
    {
        public string Name;
        public string StrParam;
        public int IntParam;

        public ObjectInjectToAttribute(string name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ObjectInjectionReceiverAttribute : Attribute
    {
        public string Name;
        public string ReceiveInjectMethodName;

        /// <summary>
        /// <code>private static void ReceiveInjection(List&lt;(object info, ObjectInjectToAttribute attr)&gt; list){ /*info is ClassType or MethodInfo or FieldInfo or PropertyInfo*/ }</code>
        /// </summary>
        public ObjectInjectionReceiverAttribute(string name, string receiveMethodName)
        {
            Name = name;
            ReceiveInjectMethodName = receiveMethodName;
        }
    }


    public static class ObjectInjection
    {
        public static void InjectAll(bool isLog = false, IEnumerable<Assembly> allAssemblies = null)
        {
            Stopwatch sw = null;
            if (isLog)
                sw = Stopwatch.StartNew();

            var receivers = new ConcurrentDictionary<string, (MethodInfo, List<(object, ObjectInjectToAttribute)>)>();
            var injections = new ConcurrentBag<(object, ObjectInjectToAttribute)>();
            var selfAsm = typeof(ObjectInjection).Assembly;
            foreach (var asm in allAssemblies ?? TypeAssistant.AllAssemblies)
            {
                if (asm == selfAsm) continue;
                asm.GetExportedTypes().AsParallel().Where(t => !t.IsGenericType && !t.IsInterface && !t.IsEnum).ForAll(t =>
                {
                    var receiverAttr = t.GetCustomAttribute<ObjectInjectionReceiverAttribute>();
                    if (receiverAttr != null)
                    {
                        try
                        {
                            var method = t.GetMethod(receiverAttr.ReceiveInjectMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                            Log.AssertNotNull(method)?.Write($"not found method: {receiverAttr.Name}.{receiverAttr.ReceiveInjectMethodName}()");
                            if (!receivers.TryAdd(receiverAttr.Name, (method, new List<(object, ObjectInjectToAttribute)>())))
                                throw new Exception($"add failure: {receiverAttr.Name}.{receiverAttr.ReceiveInjectMethodName}()");
                        }
                        catch (Exception ex)
                        {
                            Log.Error?.Write($"inject receiver error {t}.{Json5.SerializeToLog(receiverAttr)}\n{ex}");
                        }
                    }

                    var injectAttr = t.GetCustomAttribute<ObjectInjectToAttribute>();
                    if (injectAttr == null) return;

                    if (!string.IsNullOrEmpty(injectAttr.Name))
                        injections.Add((t, injectAttr));

                    var members = t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.SetProperty);
                    foreach (var m in members)
                    {
                        injectAttr = m.GetCustomAttribute<ObjectInjectToAttribute>();
                        if (injectAttr != null)
                            injections.Add((m, injectAttr));
                    }
                });
            }

            foreach (var injection in injections)
            {
                if (!receivers.TryGetValue(injection.Item2.Name, out var receiver))
                {
                    Log.Error?.Write($"{injection.Item1} not found receiver: {injection.Item2.Name}");
                    continue;
                }
                receiver.Item2.Add(injection);
            }

            var args = new object[1];
            foreach (var receiver in receivers)
            {
                args[0] = receiver.Value.Item2;
                try
                {
                    receiver.Value.Item1.Invoke(null, args);
                }
                catch (Exception ex)
                {
                    Log.Error?.Write($"{receiver.Key}, {receiver.Value.Item1?.DeclaringType}, {receiver.Value.Item2?.Count} inject error:\n{ex}");
                }
            }

            if (isLog)
                Log.Info?.Write($"inject all time: {sw.ElapsedMilliseconds} ms, receiver:{receivers.Count} inject: {injections.Count}");
        }
    }
}
