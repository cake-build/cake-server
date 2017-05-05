﻿using System.IO;
using System.Linq;
using Cake.ScriptServer.Reflection;

namespace Cake.ScriptServer.CodeGen
{
    internal sealed class CakeMethodAliasGenerator
    {
        private readonly TypeSignatureWriter _typeWriter;

        public CakeMethodAliasGenerator(TypeSignatureWriter typeWriter)
        {
            _typeWriter = typeWriter;
        }

        public void Generate(TextWriter writer, CakeScriptAlias alias)
        {
            writer.Write("public ");

            // Return type
            if (alias.Method.ReturnType != null)
            {
                _typeWriter.Write(writer, alias.Method.ReturnType);
                writer.Write(" ");
            }

            // Render the method signature.
            writer.Write(alias.Method.Name);

            // Generic arguments?
            if (alias.Method.GenericParameters.Count > 0)
            {
                writer.Write("<");
                writer.Write(string.Join(",", alias.Method.GenericParameters));
                writer.Write(">");
            }

            // Arguments
            writer.Write("(");
            WriteMethodParameters(writer, alias, invokation: false);
            writer.Write(")");

            writer.WriteLine();
            writer.WriteLine("{");
            writer.Write("\t");

            // Render the method invocation.
            WriteInvokation(writer, alias);

            writer.WriteLine("}");
            writer.WriteLine();
        }

        private void WriteMethodParameters(TextWriter writer, CakeScriptAlias alias, bool invokation)
        {
            var includeNamespace = !invokation;
            var includeParameterTypes = !invokation;

            var parameterResult = alias.Method.Parameters
                .Select(p => BuildParameter(p, includeNamespace, includeParameterTypes, true))
                .ToList();

            if (parameterResult.Count > 0)
            {
                parameterResult.RemoveAt(0);
                if (invokation)
                {
                    parameterResult.Insert(0, "Context");
                }
                writer.Write(string.Join(", ", parameterResult));
            }
        }

        private string BuildParameter(ParameterSignature parameter, bool includeNamespace, bool includeType, bool includeName)
        {
            var kind = parameter.IsOutParameter ? "out " : parameter.IsRefParameter ? "ref " : string.Empty;

            var options = includeNamespace ? TypeRenderOption.Namespace | TypeRenderOption.Name : TypeRenderOption.Name;
            var type = includeType ? _typeWriter.GetString(parameter.ParameterType, options) : string.Empty;

            return includeName
                ? $"{kind}{type} {parameter.Name}".Trim()
                : $"{kind}{type}".Trim();
        }

        private void WriteInvokation(TextWriter writer, CakeScriptAlias alias)
        {
            // Has return type?
            var hasReturnValue = !(alias.Method.ReturnType.Namespace.Name == "System" && alias.Method.ReturnType.Name == "Void");
            if (hasReturnValue)
            {
                writer.Write("return ");
            }

            // Method name.
            _typeWriter.Write(writer, alias.Method.DeclaringType);
            writer.Write(".");
            writer.Write(alias.Method.Name);

            // Generic arguments?
            if (alias.Method.GenericParameters.Count > 0)
            {
                writer.Write("<");
                writer.Write(string.Join(",", alias.Method.GenericParameters));
                writer.Write(">");
            }

            // Arguments
            writer.Write("(");
            WriteMethodParameters(writer, alias, invokation: true);
            writer.WriteLine(");");
        }
    }
}
