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

        private string linkXmlPath;
        private bool preserveAllAssemblies;
        private List<AssemblyData> assemblyNames;
        private List<string> preservedAssemblies;
        
        public LinkXmlGenerator(LinkXmlGeneratorData xmlGeneratorData)
        {
            linkXmlPath = xmlGeneratorData.linkXmlPath;
            preserveAllAssemblies = xmlGeneratorData.preserveAllAssemblies;
            assemblyNames = xmlGeneratorData.assemblyNames;
            preservedAssemblies = xmlGeneratorData.preservedAssemblies;
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
            if(preserveAllAssemblies)
                return true;

            foreach (var preservedAssembly in preservedAssemblies)
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
            foreach (var assemblyData in assemblyNames)
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
            var linkXmlContent = File.Exists(linkXmlPath) ? File.ReadAllText(linkXmlPath) : string.Empty;
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
                generatedContent.AppendLine($"  <assembly fullname=\"{assembly}\" preserve=\"all\" />");

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
            File.WriteAllText(linkXmlPath, linkXmlContent);

            return selectedAssemblies;
        }
    }
}