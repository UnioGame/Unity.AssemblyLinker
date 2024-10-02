namespace Game.Editor.Linker
{
    using System;
    using Sirenix.OdinInspector;
    using UniGame.UniBuild.Editor.ClientBuild.Interfaces;
    using UniModules.Editor;
    using UniModules.UniGame.UniBuild.Editor.ClientBuild.Commands.PreBuildCommands;

    [Serializable]
    public class LinkerBuildCommand : SerializableBuildCommand
    {
        public override void Execute(IUniBuilderConfiguration buildParameters)
        {
            Execute();
        }

        [Button]
        public void Execute()
        {
            var linker = AssetEditorTools.GetAsset<AssemblyLinkerAsset>();
            linker.ApplyPreserveAssemblies();
            linker.MarkDirty();
        }
    }
}