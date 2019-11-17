using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SimplePipeline : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        ScriptableCullingParameters cullParams;
        foreach (var camera in cameras)
        {
            context.SetupCameraProperties(camera);
            if (!camera.TryGetCullingParameters(out cullParams))
                continue;
        }



        context.Submit();
    }
}
