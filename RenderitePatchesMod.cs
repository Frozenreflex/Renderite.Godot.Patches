using System.Collections.Generic;
using FrooxEngine;
using HarmonyLib;
using Renderite.Shared;
using ResoniteModLoader;

namespace Renderite.Godot.Patches;

public class RenderitePatchesMod : ResoniteMod 
{
    public override string Name => "RenderitePatchesMod";
    public override string Author => "Frozenreflex";
    public override string Version => "1.0.1";

    public override void OnEngineInit() 
    {
        var harmony = new Harmony("Renderite.Godot.Patches");
        harmony.PatchAll();
    }
}
[HarmonyPatch(typeof(Shader))]
public static class ShaderPatches 
{
    [HarmonyPatch("LoadTargetVariant", MethodType.Async)]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> LoadTargetVariantTranspiler(IEnumerable<CodeInstruction> instructions) 
    {
        var codeMatcher = new CodeMatcher(instructions);

        var field = typeof(ShaderUpload).GetField(nameof(ShaderUpload.file));

        codeMatcher
            .MatchStartForward(CodeMatch.StoresField(field))
            .RemoveInstruction()
            .InsertAndAdvance(CodeInstruction.LoadLocal(1), CodeInstruction.Call(() => InsertNewPath(default, default, default)));

        return codeMatcher.Instructions();
    }
    private static void InsertNewPath(ShaderUpload upload, string originalString, Shader shader)
    {
        // ReSharper disable once PossibleInvalidOperationException
        var variant = shader.VariantIndex.Value;
        var keywords = shader.Metadata.UniqueKeywords;
        var usedKeywords = new List<string>(keywords.Count);
        for (var i = 0; i < keywords.Count; i++)
        {
            var mask = 1u << i;
            if ((variant & mask) > 0) usedKeywords.Add(keywords[i]);
        }
        upload.file = $"{shader.Metadata.SourceFile.FileName.Replace(".shader", "")} {string.Join(" ", usedKeywords)}";
    }
}