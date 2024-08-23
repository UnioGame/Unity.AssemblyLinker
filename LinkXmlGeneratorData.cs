namespace Game.Editor.Linker
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class LinkXmlGeneratorData
    {
        public string linkXmlPath;
        public bool preserveAllAssemblies;
        public List<AssemblyData> assemblyNames;
        public List<string> preservedAssemblies;
        public List<string> preservedTypes;
    }
}