using System.Collections.Generic;

namespace RidgeRacerVTool.TOC
{
    partial record TOCInformation (int FileCount, int TocAddress, string ArcName);

    class TableOfContents
    {
        public static Dictionary<string, TOCInformation> TOCInfos = new()
        {
            { "SLPS_200.01",    new TOCInformation(1136, 0x10BFE8, "R5.ALL") }, // JP
            { "SLUS_200.02",    new TOCInformation(1136, 0x10D258, "R5.ALL") }, // US
            { "SCES_500.00",    new TOCInformation(1208, 0x1103B8, "R5.ALL") }, // EU
            { "rrv3vera.ic002", new TOCInformation(1155, 0x1AB398, "RRV1_A") }, // AC_RRV3_A (Arcade Battle RRV3 Ver A)
            { "SLPM_601.09",    new TOCInformation(1320,  0xFD2E0, "R5.ALL") }  // JP (Demo, Taikenban)
        };
    }
}
