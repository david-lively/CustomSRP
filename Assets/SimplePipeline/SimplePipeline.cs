using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SimplePipeline : RenderPipeline
{
    Material errorMaterial;
    /*
     * Draw any objects with incompatible materials with magenta error shader
     */
    private void DrawWithDefaultPipeline(ScriptableRenderContext context, Camera camera, CullingResults cullResults)
    {
        if (null == errorMaterial)
        {
            var errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader) { hideFlags = HideFlags.HideAndDontSave };

        }

        var drawSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), new SortingSettings(camera));
        drawSettings.overrideMaterial = errorMaterial;

        var filterSettings = FilteringSettings.defaultValue;

        context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);
    }


    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        BeginFrameRendering(context, cameras);
        ScriptableCullingParameters cullParams;
        var clearColor = new Color(0, 0, 0.1f);
        foreach (var camera in cameras)
        {
            if (!camera.TryGetCullingParameters(out cullParams))
                continue;

            // notify engine and plugins that this camera is starting to render.
            BeginCameraRendering(context, camera);

            // configure view matrix, clipping, etc.
            context.SetupCameraProperties(camera);
            {
                var buffer = new CommandBuffer();
                buffer.ClearRenderTarget(true, true, clearColor);
                context.ExecuteCommandBuffer(buffer);
                buffer.Release();
            }
            // cull for this camera
            var cullResults = context.Cull(ref cullParams);



            // Draw opaque objects
            var drawSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), new SortingSettings(camera));
            //drawSettings.sortingSettings = new SortingSettings(camera);

            var filterSettings = FilteringSettings.defaultValue;
            filterSettings.renderQueueRange = RenderQueueRange.opaque;
            context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);

            // Draw skybox
            context.DrawSkybox(camera);


            // Draw transparent objects
            drawSettings.sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonTransparent };
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);

            /*
             * draw anything that's left. Materials are mutually exclusive between SRP and built-in
             * pipelines, so this will only draw objects that have not already been drawn.
             */
            DrawWithDefaultPipeline(context, camera, cullResults);

            // Notify engine, plugins, etc. that this camera is finished rendering.
            EndCameraRendering(context, camera);

        }

        context.Submit();
        EndFrameRendering(context, cameras);
    }
}
