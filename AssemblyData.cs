namespace Game.Editor.Linker
{
    using System;
    using Sirenix.OdinInspector;

    [Serializable]
    public class AssemblyData
#if ODIN_INSPECTOR
        : ISearchFilterable
#endif
    {
        public string assemblyName = string.Empty;
        public bool preserve = false;
        
        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString)) return true;
            if (assemblyName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}