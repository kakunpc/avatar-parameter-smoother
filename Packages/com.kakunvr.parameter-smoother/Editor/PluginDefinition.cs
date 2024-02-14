using nadena.dev.ndmf;

[assembly: ExportsPlugin(typeof(com.kakunvr.parameter_smoother.editor.PluginDefinition))]

// ReSharper disable once CheckNamespace
namespace com.kakunvr.parameter_smoother.editor
{
    public class PluginDefinition : Plugin<PluginDefinition>
    {
        public override string QualifiedName => "com.kakunvr.parameter_smoother";
        public override string DisplayName => "Parameter Smoother";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating)
                .Run(ParameterSmootherPass.Instance);
        }
    }
}
