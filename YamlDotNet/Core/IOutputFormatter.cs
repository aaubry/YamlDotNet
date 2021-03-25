namespace YamlDotNet.Core
{
    public interface IOutputFormatter
    {
        void StreamStart();
        void StreamEnd();
        void DocumentStart();
        void DocumentEnd();
        void DocumentStartIndicatorStart();
        void DocumentStartIndicatorEnd();
        void DirectiveStart();
        void DirectiveEnd();
        void DocumentEndIndicatorStart();
        void DocumentEndIndicatorEnd();
        void ScalarStart(ScalarStyle style);
        void ScalarEnd(ScalarStyle style);
        void AliasStart();
        void AliasEnd();
        void AnchorStart();
        void AnchorEnd();
        void TagStart();
        void TagEnd();
        void DocumentSeparatorIndicatorStart();
        void DocumentSeparatorIndicatorEnd();
        void FlowSequenceStart();
        void FlowSequenceStartIndicatorStart();
        void FlowSequenceStartIndicatorEnd();
        void FlowSequenceEndIndicatorStart();
        void FlowSequenceEndIndicatorEnd();
        void FlowSequenceSeparatorStart();
        void FlowSequenceSeparatorEnd();
        void FlowSequenceEnd();
        void FlowMappingStart();
        void FlowMappingStartIndicatorStart();
        void FlowMappingStartIndicatorEnd();
        void FlowMappingEndIndicatorStart();
        void FlowMappingEndIndicatorEnd();
        void FlowMappingEnd();
        void FlowMappingSeparatorStart();
        void FlowMappingSeparatorEnd();
        void MappingKeyIndicatorStart();
        void MappingKeyIndicatorEnd();
        void MappingValueIndicatorStart();
        void MappingValueIndicatorEnd();
        void MappingKeyStart();
        void MappingKeyEnd();
        void MappingValueStart();
        void MappingValueEnd();
        void BlockSequenceStart();
        void BlockSequenceEnd();
        void BlockSequenceItemIndicatorStart();
        void BlockSequenceItemIndicatorEnd();
        void SequenceItemStart();
        void SequenceItemEnd();
        void BlockScalarHintIndicatorStart();
        void BlockScalarHintIndicatorEnd();
    }
}
