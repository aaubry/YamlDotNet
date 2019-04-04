using System;

namespace YamlDotNet.Core
{
    public sealed class EmitterSettings
    {
        /// <summary>
        /// The preferred indentation.
        /// </summary>
        public int BestIndent { get; } = 2;

        /// <summary>
        /// The preferred text width.
        /// </summary>
        public int BestWidth { get; } = int.MaxValue;

        /// <summary>
        /// If true, write the output in canonical form.
        /// </summary>
        public bool IsCanonical { get; } = false;

        /// <summary>
        /// The maximum allowed length for simple keys.
        /// </summary>
        /// <remarks>
        /// The specifiction mandates 1024 characters, but any desired value may be used.
        /// </remarks>
        public int MaxSimpleKeyLength { get; } = 1024;

        public static readonly EmitterSettings Default = new EmitterSettings();

        public EmitterSettings()
        {
        }

        public EmitterSettings(int bestIndent, int bestWidth, bool isCanonical, int maxSimpleKeyLength)
        {
            if(bestIndent < 2 || bestIndent > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(bestIndent), $"BestIndent must be between 2 and 9, inclusive");
            }

            if (bestWidth <= bestIndent * 2)
            {
                throw new ArgumentOutOfRangeException(nameof(bestWidth), "BestWidth must be greater than BestIndent x 2.");
            }

            if (maxSimpleKeyLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSimpleKeyLength), "MaxSimpleKeyLength must be >= 0");
            }

            BestIndent = bestIndent;
            BestWidth = bestWidth;
            IsCanonical = isCanonical;
            MaxSimpleKeyLength = maxSimpleKeyLength;
        }

        public EmitterSettings WithBestIndent(int bestIndent)
        {
            return new EmitterSettings(
                bestIndent,
                BestWidth,
                IsCanonical,
                MaxSimpleKeyLength
            );
        }

        public EmitterSettings WithBestWidth(int bestWidth)
        {
            return new EmitterSettings(
                BestIndent,
                bestWidth,
                IsCanonical,
                MaxSimpleKeyLength
            );
        }

        public EmitterSettings WithMaxSimpleKeyLength(int maxSimpleKeyLength)
        {
            return new EmitterSettings(
                BestIndent,
                BestWidth,
                IsCanonical,
                maxSimpleKeyLength
            );
        }

        public EmitterSettings Canonical()
        {
            return new EmitterSettings(
                BestIndent,
                BestWidth,
                true,
                MaxSimpleKeyLength
            );
        }
    }
}
