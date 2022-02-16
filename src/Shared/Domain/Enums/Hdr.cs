namespace MediaButler.Shared.Domain.Enums
{
    /// <summary>
    /// Describes HDR modes
    /// </summary>
    public enum Hdr
    {
        /// <summary>
        /// No HDR 
        /// </summary>
        None,

        /// <summary>
        /// HDR10
        /// </summary>
        HDR10,

        /// <summary>
        /// HDR10+
        /// </summary>
        HDR10Plus,

        /// <summary>
        /// Dolby Vision
        /// </summary>
        DolbyVision,

        /// <summary>
        /// Hybrid Log Gamma
        /// </summary>
        HLG,

        /// <summary>
        /// Advanced HDR by Technicolor (SL-HDR1)
        /// </summary>
        SLHDR1,

        /// <summary>
        /// Advanced HDR by Technicolor (SL-HDR2)
        /// </summary>
        SLHDR2,

        /// <summary>
        /// Advanced HDR by Technicolor (SL-HDR3)
        /// </summary>
        SLHDR3
    }
}