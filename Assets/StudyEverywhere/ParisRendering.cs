using UnityEngine;
using UnityEngine.Rendering;

namespace StudyEverywhere
{
    // Scene-local rendering policy. Restore the previous pipeline when this scene is unloaded.
    // Android (Quest) builds, and the Editor while Android is the active target, use the lighter Quest pipeline.
    [ExecuteAlways]
    public sealed class ParisRendering : MonoBehaviour
    {
        public RenderPipelineAsset pipeline;
        public RenderPipelineAsset questPipeline;
        RenderPipelineAsset previous;
        ShadowQuality shadows;
        bool applied;
#if UNITY_ANDROID
        public RenderPipelineAsset Active => questPipeline ? questPipeline : pipeline;
#else
        public RenderPipelineAsset Active => pipeline;
#endif
        void OnEnable(){if(!Active)return;previous=QualitySettings.renderPipeline;shadows=QualitySettings.shadows;QualitySettings.renderPipeline=Active;QualitySettings.shadows=ShadowQuality.All;applied=true;}
        void OnDisable(){if(!applied)return;if(QualitySettings.renderPipeline==Active)QualitySettings.renderPipeline=previous;QualitySettings.shadows=shadows;applied=false;}
        public void Apply(){if(applied)OnDisable();OnEnable();}
    }
}
