using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SimplePipeline : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        ScriptableCullingParameters cullParams;
        var clearColor = new Color(0, 0, 0.1f);
        foreach (var camera in cameras)
        {
            var buffer = new CommandBuffer();

            context.SetupCameraProperties(camera);

            buffer.ClearRenderTarget(true, true, clearColor);
            context.ExecuteCommandBuffer(buffer);
            buffer.Release();

            if (!camera.TryGetCullingParameters(out cullParams))
                continue;

            var cullResults = context.Cull(ref cullParams);

            var drawSettings = new DrawingSettings();
            var filterSettings = new FilteringSettings();

            context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);
        }

        context.Submit();
    }
}
