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
    public override string Version => "1.0.0";

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
    private static void InsertNewPath(ShaderUpload upload, string originalString, Shader shader) => upload.file = shader.Metadata.SourceFile.FileName;
}