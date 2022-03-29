using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AMDColorTweaks.ViewModel;
using AMDColorTweaks.Util;

namespace AMDColorTweaks.LittleCms
{
    internal class IccReader
    {
        private static IntPtr MustReverseToneCurve(IntPtr trc)
        {
            if (trc != IntPtr.Zero)
            {
                var revtrc = CmsNative.cmsReverseToneCurve(trc);
                if (revtrc == IntPtr.Zero)
                {
                    throw new FileFormatException("cmsReverseToneCurveEx failed");
                }
                return revtrc;
            }
            throw new ArgumentNullException(nameof(trc));
        }



        private static unsafe IntPtr MustReadTag(IntPtr profile_, cmsTagSignature tag)
        {
            var result = CmsNative.cmsReadTag(profile_, tag);
            if (result == IntPtr.Zero)
            {
                var tagName = Encoding.ASCII.GetString((byte*)&tag, 4);
                throw new FileFormatException($"tag {tagName} not found");
            }
            return result;
        }

        public static unsafe (GamutViewModel, TransferViewModel) ReadICC(ReadOnlySpan<byte> iccProfile, bool applyVcgt)
        {
            IntPtr profile;
            fixed (byte* ptr = iccProfile)
                profile = CmsNative.cmsOpenProfileFromMem((IntPtr)ptr, (uint)iccProfile.Length);
            if (profile == IntPtr.Zero)
            {
                throw new FileFormatException("cmsOpenProfileFromMem failed");
            }
            using var defer1 = new Defer(() => CmsNative.cmsCloseProfile(profile));
            var pcs = CmsNative.cmsGetPCS(profile);
            var targetSpace = CmsNative.cmsGetColorSpace(profile);

            if (pcs != cmsColorSpaceSignature.cmsSigXYZData || targetSpace != cmsColorSpaceSignature.cmsSigRgbData)
            {
                throw new FileFormatException("ICC profile is not XYZ->RGB");
            }

            var wtpt = (cmsCIEXYZ*)MustReadTag(profile, cmsTagSignature.cmsSigMediaWhitePointTag);
            var gamut = new GamutViewModel();
            gamut.UseCustomGamut = true;
            gamut.UseCustomWhitePoint = true;
            gamut.PredefinedGamut = ADL.ADLGamutSpace.ADL_GAMUT_SPACE_CUSTOM;
            gamut.PredefinedWhitePoint = ADL.ADLWhitePoint.ADL_WHITE_POINT_CUSTOM;

            var rXYZ = (cmsCIEXYZ*)MustReadTag(profile, cmsTagSignature.cmsSigRedColorantTag);
            var gXYZ = (cmsCIEXYZ*)MustReadTag(profile, cmsTagSignature.cmsSigGreenColorantTag);
            var bXYZ = (cmsCIEXYZ*)MustReadTag(profile, cmsTagSignature.cmsSigBlueColorantTag);
            var rxyY = rXYZ->ToCIExyY();
            var gxyY = gXYZ->ToCIExyY();
            var bxyY = bXYZ->ToCIExyY();
            var wxyY = wtpt->ToCIExyY();
            gamut.CustomRed.X = rxyY.x;
            gamut.CustomRed.Y = rxyY.y;
            gamut.CustomGreen.X = gxyY.x;
            gamut.CustomGreen.Y = gxyY.y;
            gamut.CustomBlue.X = bxyY.x;
            gamut.CustomBlue.Y = bxyY.y;
            gamut.CustomWhitePoint.X = wxyY.x;
            gamut.CustomWhitePoint.Y = wxyY.y;

            var rTRC = MustReadTag(profile, cmsTagSignature.cmsSigRedTRCTag);
            var gTRC = MustReadTag(profile, cmsTagSignature.cmsSigGreenTRCTag);
            var bTRC = MustReadTag(profile, cmsTagSignature.cmsSigBlueTRCTag);
            var trcs = new IntPtr[] { MustReverseToneCurve(rTRC), MustReverseToneCurve(gTRC), MustReverseToneCurve(bTRC) };
            using var defer2 = new Defer(() =>
            {
                foreach (var t in trcs)
                    CmsNative.cmsFreeToneCurve(t);
            });

            var vcgt = (IntPtr*)CmsNative.cmsReadTag(profile, cmsTagSignature.cmsSigVcgtTag);
            var luts = new ushort[][] { new ushort[256], new ushort[256], new ushort[256] };
            var curves = new ParametricCurveViewModel[3];
            var transfer = new TransferViewModel();

            bool useLUT = false;
            if (vcgt != null && applyVcgt)
            {
                useLUT = true;
                for (var channel = 0; channel < 3; channel++)
                {
                    for (var x = 0; x < 256; x++)
                    {
                        var trcOut = CmsNative.cmsEvalToneCurve16(trcs[channel], (ushort)Math.Round(x / 255.0 * 65535.0));
                        var vcgtOut = CmsNative.cmsEvalToneCurve16(vcgt[channel], trcOut);
                        luts[channel][x] = vcgtOut;
                    }
                }
            }
            else
            {
                bool fallbackToLUT = false;
                for (var channel = 0; channel < 3; channel++)
                {
                    var curveType = CmsNative.cmsGetToneCurveParametricType(trcs[channel]);
                    if (curveType == 0)
                    {
                        fallbackToLUT = true;
                        break;
                    }
                    var curve = new ParametricCurveViewModel();
                    var curveParams = CmsNative.cmsGetToneCurveParams(trcs[channel]);
                    switch (curveType)
                    {
                        case 1:
                            curve.Gamma = 1 / curveParams[0];
                            break;
                        case 5:
                            if (Math.Abs(curveParams[6]) > 0.0001)
                            {
                                fallbackToLUT = true;
                                break;
                            }
                            curve.A3 = -curveParams[5];
                            goto case 4;
                        case 4:
                            curve.A0 = curveParams[4];
                            curve.A1 = curveParams[3];
                            curve.A2 = curveParams[1] - 1.0;
                            if (Math.Abs(curveParams[2]) > 0.0001)
                            {
                                fallbackToLUT = true;
                                break;
                            }
                            curve.Gamma = 1 / curveParams[0];
                            break;
                        case 6:
                            if (Math.Abs(curveParams[2]) > 0.0001)
                            {
                                fallbackToLUT = true;
                                break;
                            }
                            curve.A0 = 0;
                            curve.Gamma = 1 / curveParams[0];
                            curve.A3 = -curveParams[3];
                            break;
                        default:
                            fallbackToLUT = true;
                            break;
                    }
                    if (fallbackToLUT) break;
                    curves[channel] = curve;
                }

                if (fallbackToLUT)
                {
                    useLUT = true;
                    for (var channel = 0; channel < 3; channel++)
                    {
                        for (var x = 0; x < 256; x++)
                        {
                            var trcOut = CmsNative.cmsEvalToneCurve16(trcs[channel], (ushort)Math.Round(x / 255.0 * 65535.0));
                            luts[channel][x] = trcOut;
                        }
                    }
                }
            }
            if (useLUT)
            {
                transfer.TransferType = TransferType.ChannelLUT;
                luts[0].CopyTo(transfer.RedLUT, 0);
                luts[1].CopyTo(transfer.GreenLUT, 0);
                luts[2].CopyTo(transfer.BlueLUT, 0);
            }
            else
            {
                transfer.TransferType = TransferType.ParametricOETF;
                curves[0].CopyTo(transfer.RedCurve);
                curves[1].CopyTo(transfer.GreenCurve);
                curves[2].CopyTo(transfer.BlueCurve);
            }

            return (gamut, transfer);
        }
    }
}
