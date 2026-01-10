using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FLib.Gen
{
    [Generator]
    public class BytesPackGen : ISourceGenerator
    {
        private class SyntaxReceiver : ISyntaxReceiver
        {
            public readonly List<TypeDeclarationSyntax> CandidateTypes = new List<TypeDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is TypeDeclarationSyntax typeDeclaration &&
                    typeDeclaration.AttributeLists.Count > 0)
                {
                    CandidateTypes.Add(typeDeclaration);
                }
            }
        }

        public class MemberData
        {
            public int Key;
            public ISymbol Symbol;
            public ITypeSymbol Type;
            public string Name => Symbol.Name;

            public MemberData(ISymbol symbol, int key)
            {
                Symbol = symbol;
                Key = key;
                Type = (Symbol as IFieldSymbol)?.Type ?? (Symbol as IPropertySymbol)?.Type!;
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver || !receiver.CandidateTypes.Any())
                return;

            var compilation = context.Compilation;
            var targets = new Dictionary<INamedTypeSymbol, MemberData[]>(SymbolEqualityComparer.Default);

            foreach (var candidateType in receiver.CandidateTypes)
            {
                var model = compilation.GetSemanticModel(candidateType.SyntaxTree);
                var typeSymbol = model.GetDeclaredSymbol(candidateType) as INamedTypeSymbol;

                if (typeSymbol == null)
                    continue;

                if (!typeSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "BytesPackGenAttribute"))
                    continue;

                var members = typeSymbol.GetMembers()
                    .Where(m => m is IFieldSymbol or IPropertySymbol &&
                                m.GetAttributes().Any(attr => attr.AttributeClass?.Name == "BytesPackGenFieldAttribute"))
                    .ToArray();

                var memberDatas = new MemberData[members.Length];
                var keyOffset = 0;

                for (var i = 0; i < memberDatas.Length; i++)
                {
                    var symbol = members[i];
                    var attr = symbol.GetAttributes().Single(v => v.AttributeClass?.Name == "BytesPackGenFieldAttribute");

                    if ((attr.ConstructorArguments.ElementAtOrDefault(0).Value ??
                         attr.NamedArguments.SingleOrDefault(v => v.Key == "Key").Value.Value) is int defKey)
                        keyOffset = defKey;
                    else
                        ++keyOffset;

                    memberDatas[i] = new MemberData(symbol, keyOffset);
                }

                targets.Add(typeSymbol, memberDatas);
            }

            if (!targets.Any())
                return;

            var strbuf = new StringBuilder();
            foreach (var target in targets)
            {
                strbuf.AppendLine("using System;");
                strbuf.AppendLine("using System.Collections;");
                strbuf.AppendLine("using System.Collections.Generic;");
                strbuf.AppendLine("using FLib;");
                strbuf.AppendLine();

                var members = target.Value;
                var symbol = target.Key;
                var fileName = symbol.Name;
                var isExistParentBytesPack = CheckTypeIsBytesPack(symbol.BaseType!);

                // 开始写入类型头 (namespace and class)
                var braceCount = 1;
                var ns = symbol.ContainingNamespace;
                if (!ns.IsGlobalNamespace)
                {
                    strbuf.AppendLine($"namespace {ns} {{");
                    ++braceCount;
                }

                // 写入嵌套类型的父级
                var parentSymbol = symbol.ContainingType;
                while (parentSymbol != null)
                {
                    ++braceCount;
                    fileName = parentSymbol.Name + "." + fileName;
                    WriteTypeDefine(strbuf, parentSymbol).AppendLine(" {");
                    parentSymbol = parentSymbol.ContainingType;
                }

                WriteTypeDefine(strbuf, symbol).AppendLine(" : IBytesPackable {");

                // 计算全部父类得到key偏移
                var parentKey = GetAllParentKeyValue(symbol.BaseType!);

                var modifier = "";
                if (symbol.TypeKind == TypeKind.Class)
                    modifier = isExistParentBytesPack ? " override" : " virtual";

                var tempUniqueIndex = 0;
                // pack写入
                strbuf.AppendLine($"public{modifier} void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer) {{");
                if (isExistParentBytesPack && symbol.BaseType?.GetMembers("Z_BytesPackWrite").SingleOrDefault()?.IsAbstract != true)
                    strbuf.AppendLine("base.Z_BytesPackWrite(ref key, ref writer);");

                for (var i = 1; i <= members.Length; i++)
                {
                    var m = members[i - 1];
                    m.Key += parentKey;
                    tempUniqueIndex = BytesPackWrite(strbuf, m, targets, tempUniqueIndex) + 1;
                }
                strbuf.AppendLine("}");

                // pack读取
                strbuf.AppendLine($"public{modifier} void Z_BytesPackRead(int key, ref BytesReader reader) {{");
                if (members.Length > 0)
                {
                    strbuf.AppendLine("switch (key) {");
                    tempUniqueIndex = 0;
                    for (var i = 1; i <= members.Length; i++)
                    {
                        var m = members[i - 1];
                        tempUniqueIndex = BytesPackRead(strbuf, m, targets, tempUniqueIndex) + 1;
                    }
                    if (isExistParentBytesPack && symbol.BaseType?.GetMembers("Z_BytesPackRead").SingleOrDefault()?.IsAbstract != true)
                        strbuf.AppendLine("default: base.Z_BytesPackRead(key, ref reader); break;");
                    strbuf.Append('}', 2);
                }
                else
                {
                    if (isExistParentBytesPack && symbol.BaseType?.GetMembers("Z_BytesPackRead").SingleOrDefault()?.IsAbstract != true)
                        strbuf.AppendLine("base.Z_BytesPackRead(key, ref reader);");
                    strbuf.Append('}');
                }
                // 结束写入类型头
                strbuf.Append('}', braceCount);

                // over
                context.AddSource($"{fileName}.g.cs", strbuf.ToString());
                strbuf.Clear();
            }
        }

        /// <summary>
        ///
        /// </summary>
        private static int BytesPackWrite(StringBuilder strbuf, MemberData m, Dictionary<INamedTypeSymbol, MemberData[]> targets, int uniqueIndex)
        {
            var isCheckWrite = BytesPackWriteAppendCheckCode(strbuf, m.Name, m.Type, true);
            strbuf.Append($"key.Push(ref writer, {m.Key}); ");
            WriteValue(m.Type, m.Name, strbuf, ref uniqueIndex);
            if (isCheckWrite)
                strbuf.AppendLine("}");
            return uniqueIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool BytesPackWriteAppendCheckCode(StringBuilder strbuf, string fieldName, ITypeSymbol fieldType, bool isIgnoreDefaultValue)
        {
            var specialType = fieldType.SpecialType;
            var isNullable = CheckTypeIsNullable(fieldType);
            var isNumeric = specialType >= SpecialType.System_Boolean && specialType <= SpecialType.System_Double;
            var isCheckWrite = isNullable || isNumeric;
            if (isCheckWrite)
            {
                var isStr = specialType == SpecialType.System_String;
                if (!isIgnoreDefaultValue && (isNumeric || isStr))
                    return false;
                strbuf.Append("if (");
                strbuf.Append(isStr ? $"!string.IsNullOrEmpty({fieldName})" : $"{fieldName} != {(isNullable ? "null" : "default")}");
                strbuf.AppendLine(") {");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void WriteValue(ITypeSymbol type, string fieldName, StringBuilder strbuf, ref int uniqueIndex)
        {
            if (!TryGenCustomCode(type, fieldName, strbuf, ref uniqueIndex, false))
            {
                if (IsVInt(type))
                {
                    strbuf.AppendLine($"writer.PushVInt({fieldName});");
                }
                else
                {
                    ++uniqueIndex;
                    if (CheckTypeIsListOrDict(type, out var args, out var isList))
                    {
                        strbuf.AppendLine();
                        strbuf.AppendLine($"writer.PushLength({fieldName}.Count);");
                        var key = "item" + uniqueIndex;
                        strbuf.AppendLine($"foreach (var {key} in {fieldName}) {{");
                        if (isList)
                        {
                            WriteValue(args[0], key, strbuf, ref uniqueIndex);
                        }
                        else
                        {
                            WriteValue(args[0], key + ".Key", strbuf, ref uniqueIndex);
                            WriteValue(args[1], key + ".Value", strbuf, ref uniqueIndex);
                        }
                        strbuf.AppendLine("}");
                    }
                    else if (CheckTypeIsBytesPack(type))
                    {
                        if (type is IArrayTypeSymbol array && CheckTypeIsNullable(array.ElementType))
                        {
                            strbuf.AppendLine($"BytesPack.PackNullableElement({fieldName}, ref writer);");
                        }
                        else
                        {
                            strbuf.AppendLine($"BytesPack.Pack({fieldName}, ref writer);");
                        }
                    }
                    else if (type is IArrayTypeSymbol arrType && (!arrType.ElementType.IsUnmanagedType || CheckTypeIsBytesSerializable(type)))
                    {
                        var iVarName = $"i{uniqueIndex}";
                        strbuf.AppendLine();
                        strbuf.AppendLine($"writer.PushLength({fieldName}.Length); ");
                        strbuf.Append($"for (var {iVarName} = 0; {iVarName} < {fieldName}.Length; {iVarName}++) {{");
                        WriteValue(arrType.ElementType, $"{fieldName}[{iVarName}]", strbuf, ref uniqueIndex);
                        strbuf.AppendLine("}");
                    }
                    else if (CheckTypeIsBytesSerializable(type))
                    {
                        if (type.TypeKind != TypeKind.Array && CheckTypeIsNullable(type))
                            strbuf.Append($"{fieldName} ??= new(); ");
                        strbuf.Append($"{fieldName}.Z_BytesWrite(ref writer); ");
                    }
                    else if (type.SpecialType == SpecialType.System_Nullable_T || (type.TypeKind > TypeKind.Enum && type.NullableAnnotation == NullableAnnotation.Annotated))
                    {
                        strbuf.AppendLine($"writer.Push({fieldName}.Value);");
                    }
                    else if (CheckTypeIsLocate(type, out var relocateField))
                    {
                        WriteValue(relocateField.Type, $"{fieldName}.{relocateField.Name}", strbuf, ref uniqueIndex);
                    }
                    else
                    {
                        strbuf.AppendLine($"writer.Push({fieldName});");
                    }
                }
            }
            var additionalCodeAttr = FindAttribute(type, "BytesPackGenAdditionalCodeAttribute");
            var arg = additionalCodeAttr?.NamedArguments.SingleOrDefault(v => v.Key == "WriteCode").Value.Value;
            if (arg != null)
                strbuf.Append(ReplaceAdditionalText(arg.ToString(), type, fieldName)).Append(' ');
        }

        /// <summary>
        ///
        /// </summary>
        private static int BytesPackRead(StringBuilder strbuf, MemberData m, Dictionary<INamedTypeSymbol, MemberData[]> targets, int uniqueIndex)
        {
            strbuf.Append("case ").Append(m.Key).Append(": ");
            ReadValue(m.Type, m.Name, strbuf, ref uniqueIndex);
            strbuf.AppendLine("break;");
            return uniqueIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ReadValue(ITypeSymbol type, string fieldName, StringBuilder strbuf, ref int uniqueIndex)
        {
            if (!TryGenCustomCode(type, fieldName, strbuf, ref uniqueIndex, true))
            {
                if (IsVInt(type))
                {
                    strbuf.Append($"{fieldName} = ({type.Name})reader.ReadVInt();");
                }
                else
                {
                    ++uniqueIndex;
                    var isBytesSerializable = CheckTypeIsBytesSerializable(type);
                    if (CheckTypeIsListOrDict(type, out var args, out var isList))
                    {
                        strbuf.AppendLine();
                        strbuf.AppendLine($"var __count{uniqueIndex} = reader.ReadLength();");
                        strbuf.AppendLine($"({fieldName} ??= new {GetTypeString(type, true)}(__count{uniqueIndex})).Clear();");
                        var iVarName = $"i{uniqueIndex}";
                        strbuf.AppendLine($"for (var {iVarName} = 0; {iVarName} < __count{uniqueIndex}; {iVarName}++) {{");
                        if (isList)
                        {
                            var vStr = "v" + uniqueIndex;
                            WriteVarDef(args[0], vStr);
                            ReadValue(args[0], vStr, strbuf, ref uniqueIndex);
                            strbuf.AppendLine($"{fieldName}.Add({vStr});");
                        }
                        else
                        {
                            var kStr = "k" + uniqueIndex;
                            var vStr = "v" + uniqueIndex;
                            strbuf.Append("var ");
                            ReadValue(args[0], kStr, strbuf, ref uniqueIndex);
                            WriteVarDef(args[1], vStr);
                            ReadValue(args[1], vStr, strbuf, ref uniqueIndex);
                            strbuf.AppendLine($"{fieldName}.Add({kStr}, {vStr}); ");
                        }
                        strbuf.AppendLine("}");

                        void WriteVarDef(ITypeSymbol t, string vStr)
                        {
                            if (t.IsValueType || t.SpecialType == SpecialType.System_String || t.TypeKind == TypeKind.Array)
                                strbuf.Append($"{GetTypeString(t, true)} {vStr} = default; ");
                            else
                                strbuf.Append($"var {vStr} = new {GetTypeString(t, true)}(); ");
                        }
                    }
                    else if (CheckTypeIsBytesPack(type))
                    {
                        if (type is IArrayTypeSymbol arrType && CheckTypeIsNullable(arrType.ElementType))
                        {
                            strbuf.Append($"BytesPack.UnpackNullableElement(ref {fieldName}, ref reader); ");
                        }
                        else
                        {
                            if (type.TypeKind != TypeKind.Array && CheckTypeIsNullable(type))
                                strbuf.Append($"{fieldName} ??= new(); ");
                            strbuf.Append($"BytesPack.Unpack(ref {fieldName}, ref reader); ");
                        }
                    }
                    else if (type is IArrayTypeSymbol arrType && (!type.IsUnmanagedType || isBytesSerializable))
                    {
                        if (arrType.ElementType.SpecialType == SpecialType.System_String)
                        {
                            strbuf.Append($"{fieldName} = reader.ReadStrings(); ");
                        }
                        else if (arrType.ElementType is IArrayTypeSymbol arrType2)
                        {
                            if (arrType2.ElementType.SpecialType == SpecialType.System_String)
                                strbuf.Append($"{fieldName} = reader.ReadStrings2(); ");
                            else
                                strbuf.Append($"{fieldName} = reader.ReadArray2<{GetTypeString(arrType2.ElementType, true)}>(); ");
                        }
                        else if (!arrType.ElementType.IsUnmanagedType || isBytesSerializable)
                        {
                            var iVarName = $"i{uniqueIndex}";
                            strbuf.AppendLine($"var __count{uniqueIndex} = reader.ReadLength();");
                            strbuf.AppendLine($"{fieldName} = new {GetTypeString(arrType.ElementType, true)}[__count{uniqueIndex}];");
                            strbuf.Append($"for (var {iVarName} = 0; {iVarName} < __count{uniqueIndex}; {iVarName}++) {{");
                            ReadValue(arrType.ElementType, $"{fieldName}[{iVarName}]", strbuf, ref uniqueIndex);
                            strbuf.AppendLine("}");
                        }
                        else
                        {
                            strbuf.Append($"{fieldName} = reader.ReadArray<{GetTypeString(arrType.ElementType, true)}>(); ");
                        }
                    }
                    else if (isBytesSerializable)
                    {
                        if (type.TypeKind != TypeKind.Array && CheckTypeIsNullable(type))
                            strbuf.Append($"{fieldName} ??= new(); ");
                        strbuf.Append($"{fieldName}.Z_BytesRead(ref reader); ");
                    }
                    else if (type.SpecialType == SpecialType.System_String)
                    {
                        strbuf.Append($"{fieldName} = reader.ReadString(); ");
                    }
                    else if (CheckTypeIsLocate(type, out var relocateField))
                    {
                        ReadValue(relocateField.Type, $"{fieldName}.{relocateField.Name}", strbuf, ref uniqueIndex);
                    }
                    else
                    {
                        strbuf.Append($"{fieldName} = reader.Read<{GetTypeString(type, true)}>(); ");
                    }
                }
            }
            var additionalCodeAttr = FindAttribute(type, "BytesPackGenAdditionalCodeAttribute");
            var arg = additionalCodeAttr?.NamedArguments.SingleOrDefault(v => v.Key == "ReadCode").Value.Value;
            if (arg != null)
                strbuf.Append(ReplaceAdditionalText(arg.ToString(), type, fieldName)).Append(' ');
        }

        /// <summary>
        ///
        /// </summary>
        private static StringBuilder WriteTypeDefine(StringBuilder strbuf, INamedTypeSymbol symbol)
        {
            var type = symbol.TypeKind == TypeKind.Class ? "class" : "struct";
            //strbuf.Append($"[System.Diagnostics.DebuggerNonUserCode] ");
            strbuf.Append($"{symbol.DeclaredAccessibility.ToString().ToLowerInvariant()} partial {type} {symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}");
            return strbuf;
        }

        /// <summary>
        ///
        /// </summary>
        private static bool IsVInt(ITypeSymbol t)
        {
            var type = t.SpecialType;
            return type >= SpecialType.System_Char && type <= SpecialType.System_UInt32;
        }

        /// <summary>
        ///
        /// </summary>
        private static int GetAllParentKeyValue(INamedTypeSymbol? parent)
        {
            var incrementKey = 0;
            while (parent != null)
            {
                var fieldKey = 0;
                foreach (var member in parent.GetMembers())
                {
                    var attr = member.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "BytesPackGenFieldAttribute");
                    if (attr != null)
                    {
                        if ((attr.ConstructorArguments.ElementAtOrDefault(0).Value ?? attr.NamedArguments.SingleOrDefault(item => item.Key == "Key").Value.Value) is int v)
                            fieldKey = v;
                        else
                            ++fieldKey;
                    }
                }

                var holdKeyAttr = FindAttribute(parent, "BytesPackGenHoldKeyAttribute", false);
                if (holdKeyAttr != null)
                {
                    var holdKey = (holdKeyAttr.ConstructorArguments.First().Value as int?).GetValueOrDefault();
                    if (fieldKey < holdKey)
                        fieldKey = holdKey;
                }

                incrementKey += fieldKey;
                parent = parent.BaseType;
            }
            return incrementKey;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool CheckTypeIsListOrDict(ITypeSymbol t, out ImmutableArray<ITypeSymbol> args, out bool isList)
        {
            isList = false;
            if (t.IsUnmanagedType)
                return false;
            if (t is not INamedTypeSymbol namedType || !namedType.IsGenericType) return false;
            args = namedType.TypeArguments;
            foreach (var i in namedType.Interfaces)
            {
                switch (i.Name)
                {
                    case nameof(IDictionary):
                        isList = false;
                        return true;
                    case nameof(IEnumerable):
                        isList = true;
                        break; // 不能直接返回,因为 Dictionary:IEnumerable。
                }
            }
            return isList;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool CheckTypeIsDict(ITypeSymbol t)
        {
            return t is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.AllInterfaces.Any(v => v.Name == "IDictionary");
        }

        /// <summary>
        ///
        /// </summary>
        public static string GetTypeString(ITypeSymbol t, bool trimNullable)
        {
            if (trimNullable)
            {
                if (t.NullableAnnotation == NullableAnnotation.Annotated)
                    t = t.WithNullableAnnotation(NullableAnnotation.None);
                if (t.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                    t = ((INamedTypeSymbol)t).TypeArguments[0];
            }
            return t.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        public static bool CheckTypeIsBytesPack(ITypeSymbol? t)
        {
            if (t == null)
                return false;
            for (var i = 0; i < 4; i++)
            {
                if (t is IArrayTypeSymbol arr)
                    t = arr.ElementType;
                else
                    break;
            }
            while (t != null)
            {
                if (t.AllInterfaces.Any(i => i.Name == "IBytesPackable") || t.GetAttributes().Any(attr => attr.AttributeClass?.Name == "BytesPackGenAttribute"))
                    return true;
                t = t.BaseType;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        public static bool CheckTypeIsNullable(ITypeSymbol t)
        {
            return t.NullableAnnotation == NullableAnnotation.Annotated ||
                   (t.NullableAnnotation == NullableAnnotation.None && t.TypeKind is TypeKind.Class or TypeKind.Array);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool CheckTypeIsBytesSerializable(ITypeSymbol t)
        {
            for (var i = 0; i < 4; i++)
            {
                if (t is IArrayTypeSymbol arr)
                    t = arr.ElementType;
                else
                    break;
            }
            return t.AllInterfaces.Any(i => i.Name == "IBytesSerializable");
        }

        /// <summary>
        ///
        /// </summary>
        public static bool CheckTypeIsLocate(ITypeSymbol t, out IFieldSymbol relocateField)
        {
            relocateField = null!;
            foreach (var item in t.GetAttributes())
            {
                if (item.AttributeClass?.Name == "BytesPackGenRelocateAttribute")
                {
                    var relocateFieldName = item.ConstructorArguments.First().Value!.ToString();
                    relocateField = t.GetMembers(relocateFieldName).SingleOrDefault() as IFieldSymbol ??
                                    throw new Exception($"not found relocate field {t.Name}.{relocateFieldName} fields {string.Join(",", t.GetMembers(relocateFieldName).Select(v => v.Name))}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public static AttributeData? FindAttribute(ITypeSymbol? typeSymbol, string name, bool isRecursive = true)
        {
            while (typeSymbol != null)
            {
                var attrs = typeSymbol.GetAttributes();
                foreach (var attr in attrs)
                {
                    if (attr.AttributeClass?.Name == name)
                        return attr;
                }
                if (!isRecursive)
                    break;
                typeSymbol = typeSymbol.BaseType;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        private static bool TryGenCustomCode(ITypeSymbol? type, string fieldName, StringBuilder strbuf, ref int uniqueIndex, bool isRead)
        {
            var result = false;
            while (type != null)
            {
                var attrs = type.GetAttributes();
                if (!attrs.Any(v => v.AttributeClass?.Name == "BytesPackGenCustomCodeAttribute"))
                {
                    type = type.BaseType;
                    continue;
                }
                foreach (var item in type.GetMembers())
                {
                    if (item is not IFieldSymbol customCodeField)
                        continue;
                    if (!result)
                    {
                        if (isRead && CheckTypeIsNullable(type))
                            strbuf.Append($"{fieldName} ??= new(); ");
                        result = true;
                    }
                    var customCodeFieldAttr = customCodeField.GetAttributes().SingleOrDefault(v => v.AttributeClass?.Name == "BytesPackGenCustomCodeAttribute");
                    var readWriteCode = isRead ? "ReadCode" : "WriteCode";
                    var code = customCodeFieldAttr?.NamedArguments.SingleOrDefault(v => v.Key == readWriteCode).Value.Value?.ToString();
                    if (string.IsNullOrEmpty(code))
                        continue;
                    code = ReplaceAdditionalText(code!, type, fieldName);
                    if (code.Contains("${Gen}"))
                    {
                        var customFieldName = $"{fieldName}.{customCodeField.Name}";
                        var strbuf2 = new StringBuilder();
                        var isCheckWrite = !isRead && BytesPackWriteAppendCheckCode(strbuf2, customFieldName, customCodeField.Type, true);
                        if (isRead)
                        {
                            strbuf2.Append("if (reader.Read<bool>()){");
                            ReadValue(customCodeField.Type, customFieldName, strbuf2, ref uniqueIndex);
                            strbuf2.Append("}");
                        }
                        else
                        {
                            strbuf2.Append("writer.Push(true);");
                            WriteValue(customCodeField.Type, customFieldName, strbuf2, ref uniqueIndex);
                        }
                        if (isCheckWrite)
                        {
                            strbuf2.AppendLine("} else writer.Push(false);");
                        }
                        code = code.Replace("${Gen}", strbuf2.ToString());
                    }
                    strbuf.Append(code).Append(' ');
                }
                type = type.BaseType;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        public static string ReplaceAdditionalText(string s, ITypeSymbol type, string fieldName)
        {
            return s.Replace("${FieldName}", fieldName).Replace("${FieldType}", type.ToString());
        }

        public static void Log(in GeneratorExecutionContext context, object msg, DiagnosticSeverity type = DiagnosticSeverity.Warning)
        {
            context.ReportDiagnostic(Diagnostic.Create("BPG", "BytesPack.Gen", msg.ToString(), type, type, true, type == DiagnosticSeverity.Warning ? 1 : 0));
        }
    }
}
