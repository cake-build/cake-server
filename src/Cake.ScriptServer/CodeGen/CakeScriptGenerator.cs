﻿using Cake.Core.IO;
using Cake.ScriptServer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.Core.Scripting;
using Cake.ScriptServer.CodeGen.Generators;
using Cake.ScriptServer.Extensions;
using Cake.ScriptServer.Reflection.Emitters;

namespace Cake.ScriptServer.CodeGen
{
    internal sealed class CakeScriptGenerator
    {
        private readonly IFileSystem _fileSystem;
        private readonly CakeScriptAliasFinder _aliasFinder;
        private readonly CakeMethodAliasGenerator _methodGenerator;
        private readonly CakePropertyAliasGenerator _propertyGenerator;

        public CakeScriptGenerator(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _aliasFinder = new CakeScriptAliasFinder(fileSystem);

            var typeEmitter = new TypeEmitter();
            var parameterEmitter = new ParameterEmitter(typeEmitter);

            _methodGenerator = new CakeMethodAliasGenerator(typeEmitter, parameterEmitter);
            _propertyGenerator = new CakePropertyAliasGenerator(typeEmitter);
        }

        public ScriptResponse Generate(FilePath assembly, bool verify)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            if (!_fileSystem.Exist(assembly))
            {
                return ScriptResponse.Empty;
            }

            // Find aliases.
            var aliases = _aliasFinder.FindAliases(new[] { assembly });

            // Create the response.
            var response = new ScriptResponse();
            response.Source = GenerateSource(aliases);
            response.Usings.AddRange(aliases.SelectMany(a => a.Namespaces));
            response.References.Add(assembly.FullPath);

            // Return the response.
            return response;
        }

        private string GenerateSource(IEnumerable<CakeScriptAlias> aliases)
        {
            var writer = new StringWriter();

            foreach (var alias in aliases)
            {
                if (alias.Type == ScriptAliasType.Method)
                {
                    _methodGenerator.Generate(writer, alias);
                }
                else
                {
                    _propertyGenerator.Generate(writer, alias);
                }

                writer.WriteLine();
                writer.WriteLine();
            }

            return writer.ToString();
        }
    }
}
