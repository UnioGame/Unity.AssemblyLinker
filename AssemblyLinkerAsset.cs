using UnityEngine;

namespace Game.Editor.Linker
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEditor.Compilation;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    [CreateAssetMenu(menuName = "Game/AssemblyLinker/AssemblyLinkerAsset", fileName = "AssemblyLinkerAsset")]
    public class AssemblyLinkerAsset : ScriptableObject
    {
        private const string PreserveInfo = "preserve info";
        private const string PreserveRules = "preserve rules";
        
        [BoxGroup(PreserveRules)]
        public bool preserveAllAssemblies = false;
        [BoxGroup(PreserveRules)]
        public List<string> preservedAssembliesRules = new();
        [BoxGroup(PreserveRules)]
        public List<string> preserveTypesRules = new();
        
#if ODIN_INSPECTOR
        [BoxGroup(PreserveInfo)]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        [ListDrawerSettings(ListElementLabelName = "@assemblyName")]
#endif
        public List<AssemblyData> assemblyNames = new();

#if ODIN_INSPECTOR
        [PropertySpace]
        [BoxGroup(PreserveInfo)]
        [ListDrawerSettings(ListElementLabelName = "@assemblyName")]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
#endif
        public List<AssemblyData> preservedAssemblies = new();

#if ODIN_INSPECTOR
        [PropertyOrder(-1)]
        [BoxGroup(PreserveRules)]
        [Button(icon:SdfIconType.CollectionFill)]
#endif
        public void CollectAssemblies()
        {
            var assemblyMap = assemblyNames
                .ToDictionary(x => x.assemblyName);

            assemblyNames.Clear();

            Debug.Log("== Player Assemblies ==");

            var stringBuilder = new StringBuilder();

            var playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

            foreach (var assembly in playerAssemblies)
            {
                var assemblyName = assembly.name;

                stringBuilder.AppendLine(assemblyName);

                if (assemblyMap.TryGetValue(assemblyName, out var assemblyData))
                {
                    assemblyNames.Add(assemblyData);
                    continue;
                }

                assemblyNames.Add(new AssemblyData()
                {
                    assemblyName = assemblyName,
                    preserve = false,
                });
            }

            EditorUtility.SetDirty(this);

            Debug.Log(stringBuilder);
        }

#if ODIN_INSPECTOR
        [PropertyOrder(-1)]
        [BoxGroup(PreserveRules)]
        [Button(SdfIconType.Activity)]
#endif
        public void ApplyPreserveAssemblies()
        {
            CollectAssemblies();

            var linkGeneratorData = new LinkXmlGeneratorData
            {
                assemblyNames = assemblyNames,
                preservedAssemblies = preservedAssembliesRules,
                preserveAllAssemblies = preserveAllAssemblies,
                linkXmlPath = $"{Application.dataPath}/link.xml",
                preservedTypes = preserveTypesRules,
            };
            var generator = new LinkXmlGenerator(linkGeneratorData);
            
            preservedAssemblies.Clear();
            var result = generator.ApplyPreserveAssemblies();
            preservedAssemblies.AddRange(result.Values);
            
            EditorUtility.SetDirty(this);
        }
    }
}