namespace Game.Editor.Linker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class LinkXmlGenerator
    {
        public const string LinkerKey = "<linker>";
        public const string LinkerEndKey = "</linker>";
        
        public const string LinkerTemplateStart = "<!--=== Linker Template ===-->";
        public const string LinkerTemplateEnd = "<!--=== Linker Template End ===-->";

        private string _linkXmlPath;
        private bool _preserveAllAssemblies;
        private List<AssemblyData> _assemblyNames;
        private List<string> _preservedAssemblies;
        private List<string> _preservedTypes;
        
        public LinkXmlGenerator(LinkXmlGeneratorData xmlGeneratorData)
        {
            _linkXmlPath = xmlGeneratorData.linkXmlPath;
            _preserveAllAssemblies = xmlGeneratorData.preserveAllAssemblies;
            _assemblyNames = xmlGeneratorData.assemblyNames;
            _preservedAssemblies = xmlGeneratorData.preservedAssemblies;
            _preservedTypes = xmlGeneratorData.preservedTypes;
        }

        public bool IsPreservedAssembly(AssemblyData assembly)
        {
            var assemblyName = assembly.assemblyName;
            if (assembly.preserve)
                return true;
            if (IsPreservedAssembly(assemblyName))
                return true;
            return false;
        }

        public bool IsPreservedAssembly(string assemblyName)
        {
            if(_preserveAllAssemblies)
                return true;

            foreach (var preservedAssembly in _preservedAssemblies)
            {
                if(assemblyName.Contains(preservedAssembly, StringComparison.OrdinalIgnoreCase))
                    return true;
                if(Regex.IsMatch(assemblyName, preservedAssembly, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        public Dictionary<string, AssemblyData> GetPreservedAssemblies()
        {
            var preservedAssembliesSet = new Dictionary<string,AssemblyData>();
            // Add assemblies to be preserved based on the flags and patterns
            foreach (var assemblyData in _assemblyNames)
            {
                var assemblyName = assemblyData.assemblyName;
                if (IsPreservedAssembly(assemblyData))
                    preservedAssembliesSet[assemblyName] = assemblyData;
            }

            return preservedAssembliesSet;
        }
        
        public Dictionary<string, AssemblyData> ApplyPreserveAssemblies()
        {
            var generatedContent = new StringBuilder();
            var selectedAssemblies = GetPreservedAssemblies();
            
            // Read existing link.xml content
            var linkXmlContent = File.Exists(_linkXmlPath) ? File.ReadAllText(_linkXmlPath) : string.Empty;
            var linkerIndex = linkXmlContent.IndexOf("<linker>",StringComparison.OrdinalIgnoreCase) ;
            
            // Remove existing generated block
            var startIndex = linkXmlContent.IndexOf(LinkerTemplateStart, StringComparison.Ordinal);
            var endIndex = linkXmlContent.IndexOf(LinkerTemplateEnd, StringComparison.Ordinal);
            if(startIndex >= 0 && endIndex >= 0)
            {
                endIndex += LinkerTemplateEnd.Length;
                linkXmlContent = linkXmlContent.Remove(startIndex, endIndex - startIndex);
            }
            
            var generateLink = linkerIndex < 0;
            startIndex = startIndex < 0 
                ? linkXmlContent.IndexOf(LinkerKey,StringComparison.OrdinalIgnoreCase) + LinkerKey.Length
                : startIndex;
            startIndex = Mathf.Clamp(startIndex,0,linkXmlContent.Length);
            
            // Generate new content for the assemblies
            if(generateLink) generatedContent.AppendLine(LinkerKey);
            generatedContent.AppendLine(LinkerTemplateStart);

            foreach (var assembly in selectedAssemblies.Keys)
            {
                if(string.IsNullOrEmpty(assembly)) continue;
                generatedContent.AppendLine($"  <assembly fullname=\"{assembly}\" preserve=\"all\" />");
            }

            foreach (var typeValue in _preservedTypes)
            {
                if(string.IsNullOrEmpty(typeValue)) continue;
                generatedContent.AppendLine($"  <type  fullname=\"{typeValue}\" preserve=\"all\" />");
            }
            
            generatedContent.Append(LinkerTemplateEnd);
            if(generateLink) generatedContent.AppendLine(LinkerEndKey);
            var generatedText = generatedContent.ToString();
            // Insert the generated content into the file
            // If link.xml was empty, create new content
            linkXmlContent = string.IsNullOrEmpty(linkXmlContent) 
                ? generatedText 
                // Insert generated content within existing content
                : linkXmlContent.Insert(startIndex, generatedText);
            
            // Write the updated content back to link.xml
            File.WriteAllText(_linkXmlPath, linkXmlContent);

            return selectedAssemblies;
        }
    }
}