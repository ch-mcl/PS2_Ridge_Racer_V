# Usage
Prepare these fies.
- R5.ALL
- SLUS_200.02(for USA)
- SCES_500.00(for PAL)
- SLPS_200.01(for JP)
- RRV1_A(for Arcade Battle RRV3 Ver A)
- rrv3vera.ic002(for Arcade Battle RRV3 Ver A)

And run "ridge_racer_r5_all.bms" by QuickBMS.

# Compatibility
List of Region Compatibility. If you have a file not wrote this. pls conntact me.
| Region    | SHA1 (Execution file)                                     | SHA1 (R5.ALL)                                  | Works |
|-----------|-----------------------------------------------------------|------------------------------------------------|-------|
| USA       | 273B1B20A08B5686E44E085FD8FE25AF42DBB23E (SLUS_200.02)    | D70165146274A5137F2F0CD4535B525EB52369A1       | Yes   |
| PAL       | 05C952E230360499B52E326F3F23B331294295FC (SCES_500.00)    | EFD9D83BC013A0F1DBEC1FBB1E3C9BEBBEA61B31       | Yes   |
| JP        | 9F6151848313607B5C9EC46BADF0BA62EAE02B41 (SLPS_200.01)    | E279D18A500ED10F94F7B805B4D3A32DC50D1193       | Yes   |
| AC_RRV3_A | 07BDDAAC958AC62D9FC29671FC83BD1E3B27F4B8 (rrv3vera.ic002) | 72F0793C456F10335B4B5181BCE0A808EA44EB67([^1]) | Yes   |

[^1]: "RRV1_A" works instead of "R5.ALL".

# About R5.ALL
This file is Archive file. TOC exsits in Execution file of "Ridge Racer V".

# Execution file of Ridge Racer V
| Region    | Name           |
|-----------|----------------|
| USA       | SLUS_200.02    |
| PAL       | SCES_500.00    |
| JP        | SLPS_200.01    |
| AC_RRV3_A | rrv3vera.ic002 |

# Region Diference
| Region    | Offset     | File Counts |
|-----------|------------|-------------|
| USA       | 0x10D258   | 1136        |
| PAL       | 0x1103B8   | 1208        |
| JP        | 0x10BFE8   | 1136        |
| AC_RRV3_A | 0x1AB398   | 1155        |

# TOC Format
TOC is structured by 4 uint values. I'll figure that structure.
| Offset | Type         | Description                                         |
| ------ | ------------ | --------------------------------------------------- |
| 0x00   | `uint`       | Offset. Needs times 0x800 for actual values.        |
| 0x04   | `uint`       | Size of Block. Needs times 0x800 for actual values. |
| 0x08   | `uint`       | Compressed Size of file                             |
| 0x0C   | `uint`       | Uncompressed Size of file.                          |

When Compressed Size(0x08) and Uncompressed Size(0x0C) has different means file is compressed.
