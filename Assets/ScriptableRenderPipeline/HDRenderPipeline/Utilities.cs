using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using UnityEngine.Rendering;
using UnityObject = UnityEngine.Object;
using System.Reflection;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [Flags]
    public enum ClearFlag
    {
        ClearNone  = 0,
        ClearColor = 1,
        ClearDepth = 2
    }

    public class Utilities
    {
        public static List<RenderPipelineMaterial> GetRenderPipelineMaterialList()
        {
            List<RenderPipelineMaterial> materialList = new List<RenderPipelineMaterial>();

            var baseType = typeof(RenderPipelineMaterial);
            var assembly = baseType.Assembly;

            System.Type[] types = assembly.GetTypes();
            foreach (System.Type type in types)
            {
                if (type.IsSubclassOf(baseType))
                {
                    // Create an instance object of the given type
                    var obj = (RenderPipelineMaterial)Activator.CreateInstance(type);
                    materialList.Add(obj);
                }
            }

            // Note: If there is a need for an optimization in the future of this function, user can simply fill the materialList manually by commenting the code abode and
            // adding to the list material they used in their game.
            //  materialList.Add(new Lit());
            //  materialList.Add(new Unlit());
            // ...

            return materialList;
        }

        public const RendererConfiguration kRendererConfigurationBakedLighting = RendererConfiguration.PerObjectLightProbe | RendererConfiguration.PerObjectLightmaps | RendererConfiguration.PerObjectLightProbeProxyVolume;


        // Render Target Management.
        public const ClearFlag kClearAll = ClearFlag.ClearDepth | ClearFlag.ClearColor;

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier buffer, ClearFlag clearFlag, Color clearColor, int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown)
        {
            var cmd = CommandBufferPool.Get();
            cmd.name = "";
            cmd.SetRenderTarget(buffer, miplevel, cubemapFace);
            if (clearFlag != ClearFlag.ClearNone)
                cmd.ClearRenderTarget((clearFlag & ClearFlag.ClearDepth) != 0, (clearFlag & ClearFlag.ClearColor) != 0, clearColor);
            renderContext.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier buffer, ClearFlag clearFlag = ClearFlag.ClearNone, int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown)
        {
            SetRenderTarget(renderContext, buffer, clearFlag, Color.black, miplevel, cubemapFace);
        }

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier colorBuffer, RenderTargetIdentifier depthBuffer, int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown)
        {
            SetRenderTarget(renderContext, colorBuffer, depthBuffer, ClearFlag.ClearNone, Color.black, miplevel, cubemapFace);
        }

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier colorBuffer, RenderTargetIdentifier depthBuffer, ClearFlag clearFlag, int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown)
        {
            SetRenderTarget(renderContext, colorBuffer, depthBuffer, clearFlag, Color.black, miplevel, cubemapFace);
        }

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier colorBuffer, RenderTargetIdentifier depthBuffer, ClearFlag clearFlag, Color clearColor, int miplevel = 0, CubemapFace cubemapFace = CubemapFace.Unknown)
        {
            var cmd = CommandBufferPool.Get();
            cmd.name = "";
            cmd.SetRenderTarget(colorBuffer, depthBuffer, miplevel, cubemapFace);
            if (clearFlag != ClearFlag.ClearNone)
                cmd.ClearRenderTarget((clearFlag & ClearFlag.ClearDepth) != 0, (clearFlag & ClearFlag.ClearColor) != 0, clearColor);
            renderContext.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier[] colorBuffers, RenderTargetIdentifier depthBuffer)
        {
            SetRenderTarget(renderContext, colorBuffers, depthBuffer, ClearFlag.ClearNone, Color.black);
        }

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier[] colorBuffers, RenderTargetIdentifier depthBuffer, ClearFlag clearFlag = ClearFlag.ClearNone)
        {
            SetRenderTarget(renderContext, colorBuffers, depthBuffer, clearFlag, Color.black);
        }

        public static void SetRenderTarget(ScriptableRenderContext renderContext, RenderTargetIdentifier[] colorBuffers, RenderTargetIdentifier depthBuffer, ClearFlag clearFlag, Color clearColor)
        {
            var cmd = CommandBufferPool.Get();
            cmd.name = "";
            cmd.SetRenderTarget(colorBuffers, depthBuffer);
            if (clearFlag != ClearFlag.ClearNone)
                cmd.ClearRenderTarget((clearFlag & ClearFlag.ClearDepth) != 0, (clearFlag & ClearFlag.ClearColor) != 0, clearColor);
            renderContext.ExecuteCommandBuffer(cmd);
            
        }

        public static void ClearCubemap(ScriptableRenderContext renderContext, RenderTargetIdentifier buffer, Color clearColor)
        {
            var cmd = CommandBufferPool.Get();
            cmd.name = "";

            for(int i = 0 ; i < 6 ; ++i)
            {
                SetRenderTarget(renderContext, buffer, ClearFlag.ClearColor, Color.black, 0, (CubemapFace)i);
            }

            renderContext.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Miscellanous
        public static Material CreateEngineMaterial(string shaderPath)
        {
            var mat = new Material(Shader.Find(shaderPath))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            return mat;
        }

        public static Material CreateEngineMaterial(Shader shader)
        {
            var mat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            return mat;
        }

        public static void Destroy(UnityObject obj)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    UnityObject.Destroy(obj);
                else
                    UnityObject.DestroyImmediate(obj);
#else
                UnityObject.Destroy(obj);
#endif
            }
        }

        public static void SafeRelease(ComputeBuffer buffer)
        {
            if (buffer != null)
                buffer.Release();
        }

        public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expr.Body as UnaryExpression;
                    me = (ue != null ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            var members = new List<string>();
            while (me != null)
            {
                members.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            var sb = new StringBuilder();
            for (int i = members.Count - 1; i >= 0; i--)
            {
                sb.Append(members[i]);
                if (i > 0) sb.Append('.');
            }

            return sb.ToString();
        }

        public struct ProfilingSample
            : IDisposable
        {
            bool        disposed;
            ScriptableRenderContext  renderContext;
            string      name;

            public ProfilingSample(string _name, ScriptableRenderContext _renderloop)
            {
                renderContext = _renderloop;
                disposed = false;
                name = _name;

                CommandBuffer cmd = CommandBufferPool.Get();
                cmd.name = "";
                cmd.BeginSample(name);
                renderContext.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            
            public void Dispose()
            {
                Dispose(true);
            }

            // Protected implementation of Dispose pattern.
            void Dispose(bool disposing)
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    CommandBuffer cmd = CommandBufferPool.Get();
                    cmd.name = "";
                    cmd.EndSample(name);
                    renderContext.ExecuteCommandBuffer(cmd);
                    CommandBufferPool.Release(cmd);
                }

                disposed = true;
            }
        }

        public static Matrix4x4 GetViewProjectionMatrix(Matrix4x4 worldToViewMatrix, Matrix4x4 projectionMatrix)
        {
            // The actual projection matrix used in shaders is actually massaged a bit to work across all platforms
            // (different Z value ranges etc.)
            var gpuProj = GL.GetGPUProjectionMatrix(projectionMatrix, false);
            var gpuVP = gpuProj *  worldToViewMatrix * Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f)); // Need to scale -1.0 on Z to match what is being done in the camera.wolrdToCameraMatrix API.

            return gpuVP;
        }

        public static void SetupGlobalHDCamera(HDCamera hdCamera, CommandBuffer cmd)
        {
            cmd.SetGlobalMatrix("_ViewMatrix",         hdCamera.viewMatrix);
            cmd.SetGlobalMatrix("_InvViewMatrix",      hdCamera.viewMatrix.inverse);
            cmd.SetGlobalMatrix("_ProjMatrix",         hdCamera.projMatrix);
            cmd.SetGlobalMatrix("_InvProjMatrix",      hdCamera.projMatrix.inverse);
            cmd.SetGlobalMatrix("_ViewProjMatrix",     hdCamera.viewProjMatrix);
            cmd.SetGlobalMatrix("_InvViewProjMatrix",  hdCamera.viewProjMatrix.inverse);
            cmd.SetGlobalVector("_InvProjParam",       hdCamera.invProjParam);
            cmd.SetGlobalVector("_ScreenSize",         hdCamera.screenSize);
            cmd.SetGlobalMatrix("_PrevViewProjMatrix", hdCamera.prevViewProjMatrix);
        }

        // Does not modify global settings. Used for shadows, low res. rendering, etc.
        public static void OverrideGlobalHDCamera(HDCamera hdCamera, Material material)
        {
            material.SetMatrix("_ViewMatrix",         hdCamera.viewMatrix);
            material.SetMatrix("_InvViewMatrix",      hdCamera.viewMatrix.inverse);
            material.SetMatrix("_ProjMatrix",         hdCamera.projMatrix);
            material.SetMatrix("_InvProjMatrix",      hdCamera.projMatrix.inverse);
            material.SetMatrix("_ViewProjMatrix",     hdCamera.viewProjMatrix);
            material.SetMatrix("_InvViewProjMatrix",  hdCamera.viewProjMatrix.inverse);
            material.SetVector("_InvProjParam",       hdCamera.invProjParam);
            material.SetVector("_ScreenSize",         hdCamera.screenSize);
            material.SetMatrix("_PrevViewProjMatrix", hdCamera.prevViewProjMatrix);
        }

        public static void SetupComputeShaderHDCamera(HDCamera hdCamera, ComputeShader cs, CommandBuffer cmd)
        {
            SetMatrixCS(cmd,          cs, "_ViewMatrix",         hdCamera.viewMatrix);
            SetMatrixCS(cmd,          cs, "_InvViewMatrix",      hdCamera.viewMatrix.inverse);
            SetMatrixCS(cmd,          cs, "_ProjMatrix",         hdCamera.projMatrix);
            SetMatrixCS(cmd,          cs, "_InvProjMatrix",      hdCamera.projMatrix.inverse);
            SetMatrixCS(cmd,          cs, "_ViewProjMatrix",     hdCamera.viewProjMatrix);
            SetMatrixCS(cmd,          cs, "_InvViewProjMatrix",  hdCamera.viewProjMatrix.inverse);
            cmd.SetComputeVectorParam(cs, "_InvProjParam",       hdCamera.invProjParam);
            cmd.SetComputeVectorParam(cs, "_ScreenSize",         hdCamera.screenSize);
            SetMatrixCS(cmd,          cs, "_PrevViewProjMatrix", hdCamera.prevViewProjMatrix);
        }

        // TEMP: These functions should be implemented C++ side, for now do it in C#
        static List<float> m_FloatListdata = new List<float>();
        public static void SetMatrixCS(CommandBuffer cmd, ComputeShader shadercs, string name, Matrix4x4 mat)
        {
            m_FloatListdata.Clear();

            for (int c = 0; c < 4; c++)
                for (int r = 0; r < 4; r++)
                    m_FloatListdata.Add(mat[r, c]);

            cmd.SetComputeFloatParams(shadercs, name, m_FloatListdata);
        }

        public static void SetMatrixArrayCS(CommandBuffer cmd, ComputeShader shadercs, string name, Matrix4x4[] matArray)
        {
            int numMatrices = matArray.Length;

            m_FloatListdata.Clear();
            
            for (int n = 0; n < numMatrices; n++)
                for (int c = 0; c < 4; c++)
                    for (int r = 0; r < 4; r++)
                        m_FloatListdata.Add(matArray[n][r, c]);

            cmd.SetComputeFloatParams(shadercs, name, m_FloatListdata);
        }

        public static void SetVectorArrayCS(CommandBuffer cmd, ComputeShader shadercs, string name, Vector4[] vecArray)
        {
            int numVectors = vecArray.Length;
            m_FloatListdata.Clear();

            for (int n = 0; n < numVectors; n++)
                for (int i = 0; i < 4; i++)
                    m_FloatListdata.Add(vecArray[n][i]);

            cmd.SetComputeFloatParams(shadercs, name, m_FloatListdata);
        }

        public static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        public static void SelectKeyword(Material material, string keyword1, string keyword2, bool enableFirst)
        {
            material.EnableKeyword(enableFirst ? keyword1 : keyword2);
            material.DisableKeyword(enableFirst ? keyword2 : keyword1);
        }

        public static void SelectKeyword(Material material, string[] keywords, int enabledKeywordIndex)
        {
            material.EnableKeyword(keywords[enabledKeywordIndex]);

            for (int i = 0; i < keywords.Length; i++)
            {
                if (i != enabledKeywordIndex)
                {
                    material.DisableKeyword(keywords[i]);
                }
            }
        }

        // Draws a full screen triangle as a faster alternative to drawing a full screen quad.
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier colorBuffer,
            MaterialPropertyBlock properties = null, int shaderPassID = 0)
        {
            commandBuffer.SetRenderTarget(colorBuffer);
            commandBuffer.DrawProcedural(Matrix4x4.identity, material, shaderPassID, MeshTopology.Triangles, 3, 1, properties);
        }

        // Draws a full screen triangle as a faster alternative to drawing a full screen quad.
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier colorBuffer, RenderTargetIdentifier depthStencilBuffer,
            MaterialPropertyBlock properties = null, int shaderPassID = 0)
        {
            commandBuffer.SetRenderTarget(colorBuffer, depthStencilBuffer);
            commandBuffer.DrawProcedural(Matrix4x4.identity, material, shaderPassID, MeshTopology.Triangles, 3, 1, properties);
        }

        // Draws a full screen triangle as a faster alternative to drawing a full screen quad.
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier[] colorBuffers, RenderTargetIdentifier depthStencilBuffer,
            MaterialPropertyBlock properties = null, int shaderPassID = 0)
        {
            commandBuffer.SetRenderTarget(colorBuffers, depthStencilBuffer);
            commandBuffer.DrawProcedural(Matrix4x4.identity, material, shaderPassID, MeshTopology.Triangles, 3, 1, properties);
        }

        // Draws a full screen triangle as a faster alternative to drawing a full screen quad.
        // Important: the first RenderTarget must be created with 0 depth bits!
        public static void DrawFullScreen(CommandBuffer commandBuffer, Material material,
            RenderTargetIdentifier[] colorBuffers,
            MaterialPropertyBlock properties = null, int shaderPassID = 0)
        {
            // It is currently not possible to have MRT without also setting a depth target.
            // To work around this deficiency of the CommandBuffer.SetRenderTarget() API,
            // we pass the first color target as the depth target. If it has 0 depth bits,
            // no depth target ends up being bound.
            DrawFullScreen(commandBuffer, material, colorBuffers, colorBuffers[0], properties, shaderPassID);
        }

        // Helper to help to display debug info on screen
        static float overlayLineHeight = -1.0f;
        public static void NextOverlayCoord(ref float x, ref float y, float overlayWidth, float overlayHeight, float width)
        {
            x += overlayWidth;
            overlayLineHeight = Mathf.Max(overlayHeight, overlayLineHeight);
            // Go to next line if it goes outside the screen.
            if (x + overlayWidth > width)
            {
                x = 0;
                y -= overlayLineHeight;
                overlayLineHeight = -1.0f;
            }
        }
    }
}
