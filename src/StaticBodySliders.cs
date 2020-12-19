using TaleWorlds.Core;

/* Credit for practically all of this file goes to zenDzeeMods, originally adopted from his/her mod Heritage under the MIT License.
 * I've only made the code slightly more fancy.
 */

namespace HousesCalradia
{
    internal sealed class StaticBodySliders
    {
        private ulong KeyPart1;
        private ulong KeyPart2;
        private ulong KeyPart3;
        private ulong KeyPart4;
        private ulong KeyPart5;
        private readonly ulong KeyPart6;
        private readonly ulong KeyPart7;
        private ulong KeyPart8;

        public StaticBodySliders(StaticBodyProperties properties)
        {
            KeyPart1 = properties.KeyPart1;
            KeyPart2 = properties.KeyPart2;
            KeyPart3 = properties.KeyPart3;
            KeyPart4 = properties.KeyPart4;
            KeyPart5 = properties.KeyPart5;
            KeyPart6 = properties.KeyPart6;
            KeyPart7 = properties.KeyPart7;
            KeyPart8 = properties.KeyPart8;
        }

        public StaticBodyProperties GetStaticBodyProperties()
            => new StaticBodyProperties(KeyPart1, KeyPart2, KeyPart3, KeyPart4, KeyPart5, KeyPart6, KeyPart7, KeyPart8);

        public StaticBodySliders Copy() => new StaticBodySliders(GetStaticBodyProperties());

        /* Bit Twiddling */

        private static void SetHalfByte(ref ulong key, int halfBytePosition, byte value)
        {
            halfBytePosition *= 4;
            ulong s = value;
            ulong n = s << halfBytePosition;
            s = 0x0Ful << halfBytePosition;
            key = (key & ~s) | (n & s);
        }

        private static byte GetHalfByte(ulong key, int halfBytePosition)
        {
            halfBytePosition *= 4;
            return (byte)((key >> halfBytePosition) & 0x0Ful);
        }

        private static void SetByte(ref ulong key, int halfBytePosition, byte value)
        {
            halfBytePosition *= 4;
            ulong s = value;
            ulong n = s << halfBytePosition;
            s = 0xFFul << halfBytePosition;
            key = (key & ~s) | (n & s);
        }

        private static byte GetByte(ulong key, int halfBytePosition)
        {
            halfBytePosition *= 4;
            return (byte)((key >> halfBytePosition) & 0xFFul);
        }

        private static void SetBits(ref ulong key, int bitPosition, int size, byte value)
        {
            ulong s = value;
            ulong n = s << bitPosition;
            s = (~(0xFFFFFFFFFFFFFFFFul << size)) << bitPosition;
            key = (key & ~s) | (n & s);
        }

        private static byte GetBits(ulong key, int bitPosition, int size)
        {
            ulong s = ~(0xFFFFFFFFFFFFFFFFul << size);
            return (byte)((key >> bitPosition) & s);
        }

        /* Slider Properties */

        public byte FaceWidth
        {
            get => GetHalfByte(KeyPart2, 0);
            set => SetHalfByte(ref KeyPart2, 0, value);
        }

        public byte FaceDepth
        {
            get => GetHalfByte(KeyPart2, 1);
            set => SetHalfByte(ref KeyPart2, 1, value);
        }

        public byte FaceCenterHeight
        {
            get => GetHalfByte(KeyPart2, 2);
            set => SetHalfByte(ref KeyPart2, 2, value);
        }

        public byte FaceRatio
        {
            get => GetHalfByte(KeyPart2, 3);
            set => SetHalfByte(ref KeyPart2, 3, value);
        }

        public byte FaceWeight
        {
            get => GetHalfByte(KeyPart2, 4);
            set => SetHalfByte(ref KeyPart2, 4, value);
        }

        public byte FaceCheekboneHeight
        {
            get => GetHalfByte(KeyPart2, 5);
            set => SetHalfByte(ref KeyPart2, 5, value);
        }

        public byte FaceCheekboneWidth
        {
            get => GetHalfByte(KeyPart2, 6);
            set => SetHalfByte(ref KeyPart2, 6, value);
        }

        public byte FaceCheekboneDepth
        {
            get => GetHalfByte(KeyPart2, 7);
            set => SetHalfByte(ref KeyPart2, 7, value);
        }

        public byte FaceSharpness
        {
            get => GetHalfByte(KeyPart2, 8);
            set => SetHalfByte(ref KeyPart2, 8, value);
        }

        public byte FaceTempleWidth
        {
            get => GetHalfByte(KeyPart2, 9);
            set => SetHalfByte(ref KeyPart2, 9, value);
        }

        public byte FaceEyeSocketSize
        {
            get => GetHalfByte(KeyPart2, 10);
            set => SetHalfByte(ref KeyPart2, 10, value);
        }

        public byte FaceEarShape
        {
            get => GetHalfByte(KeyPart2, 11);
            set => SetHalfByte(ref KeyPart2, 11, value);
        }

        public byte FaceEarSize
        {
            get => GetHalfByte(KeyPart2, 12);
            set => SetHalfByte(ref KeyPart2, 12, value);
        }

        public byte FaceAsymmetry
        {
            get => GetHalfByte(KeyPart2, 13);
            set => SetHalfByte(ref KeyPart2, 13, value);
        }

        public byte EyeEyebrowType
        {
            get => GetHalfByte(KeyPart8, 3);
            set => SetHalfByte(ref KeyPart8, 3, value);
        }

        public byte EyeEyebrowDepth
        {
            get => GetHalfByte(KeyPart2, 14);
            set => SetHalfByte(ref KeyPart2, 14, value);
        }

        public byte EyeBrowOuterHeight
        {
            get => GetHalfByte(KeyPart2, 15);
            set => SetHalfByte(ref KeyPart2, 15, value);
        }

        public byte EyeBrowMiddleHeight
        {
            get => GetHalfByte(KeyPart3, 0);
            set => SetHalfByte(ref KeyPart3, 0, value);
        }

        public byte EyeBrowInnerHeight
        {
            get => GetHalfByte(KeyPart3, 1);
            set => SetHalfByte(ref KeyPart3, 1, value);
        }

        public byte EyePosition
        {
            get => GetHalfByte(KeyPart3, 2);
            set => SetHalfByte(ref KeyPart3, 2, value);
        }
        public byte EyeSize
        {
            get => GetHalfByte(KeyPart3, 3);
            set => SetHalfByte(ref KeyPart3, 3, value);
        }
        public byte EyeMonolidEyes
        {
            get => GetHalfByte(KeyPart3, 4);
            set => SetHalfByte(ref KeyPart3, 4, value);
        }
        public byte EyeEyelidHeight
        {
            get => GetHalfByte(KeyPart3, 5);
            set => SetHalfByte(ref KeyPart3, 5, value);
        }
        public byte EyeDepth
        {
            get => GetHalfByte(KeyPart3, 6);
            set => SetHalfByte(ref KeyPart3, 6, value);
        }

        public byte EyeShape
        {
            get => GetHalfByte(KeyPart3, 7);
            set => SetHalfByte(ref KeyPart3, 7, value);
        }

        public byte EyeOuterHeight
        {
            get => GetHalfByte(KeyPart3, 8);
            set => SetHalfByte(ref KeyPart3, 8, value);
        }

        public byte EyeInnerHeight
        {
            get => GetHalfByte(KeyPart3, 9);
            set => SetHalfByte(ref KeyPart3, 9, value);
        }

        public byte EyeToEyeDistance
        {
            get => GetHalfByte(KeyPart3, 10);
            set => SetHalfByte(ref KeyPart3, 10, value);
        }

        public byte EyeAsymmetry
        {
            get => GetHalfByte(KeyPart3, 11);
            set => SetHalfByte(ref KeyPart3, 11, value);
        }

        public byte EyeColor
        {
            get => GetHalfByte(KeyPart1, 11);
            set => SetHalfByte(ref KeyPart1, 11, value);
        }

        public byte NoseAngle
        {
            get => GetHalfByte(KeyPart3, 12);
            set => SetHalfByte(ref KeyPart3, 12, value);
        }
        public byte NoseLength
        {
            get => GetHalfByte(KeyPart3, 13);
            set => SetHalfByte(ref KeyPart3, 13, value);
        }
        public byte NoseBridge
        {
            get => GetHalfByte(KeyPart3, 14);
            set => SetHalfByte(ref KeyPart3, 14, value);
        }
        public byte NoseTipHeight
        {
            get => GetHalfByte(KeyPart3, 15);
            set => SetHalfByte(ref KeyPart3, 15, value);
        }

        public byte NoseSize
        {
            get => GetHalfByte(KeyPart4, 0);
            set => SetHalfByte(ref KeyPart4, 0, value);
        }

        public byte NoseWidth
        {
            get => GetHalfByte(KeyPart4, 1);
            set => SetHalfByte(ref KeyPart4, 1, value);
        }

        public byte NoseNostrilHeight
        {
            get => GetHalfByte(KeyPart4, 2);
            set => SetHalfByte(ref KeyPart4, 2, value);
        }

        public byte NoseNostrilSize
        {
            get => GetHalfByte(KeyPart4, 3);
            set => SetHalfByte(ref KeyPart4, 3, value);
        }

        public byte NoseBump
        {
            get => GetHalfByte(KeyPart4, 4);
            set => SetHalfByte(ref KeyPart4, 4, value);
        }

        public byte NoseDefenition
        {
            get => GetHalfByte(KeyPart4, 5);
            set => SetHalfByte(ref KeyPart4, 5, value);
        }

        public byte NoseShape
        {
            get => GetHalfByte(KeyPart4, 6);
            set => SetHalfByte(ref KeyPart4, 6, value);
        }

        public byte NoseAsymmetry
        {
            get => GetHalfByte(KeyPart4, 7);
            set => SetHalfByte(ref KeyPart4, 7, value);
        }

        public byte MouthTeethType
        {
            get => GetHalfByte(KeyPart8, 1);
            set => SetHalfByte(ref KeyPart8, 1, value);
        }

        public byte MouthWidth
        {
            get => GetHalfByte(KeyPart4, 8);
            set => SetHalfByte(ref KeyPart4, 8, value);
        }

        public byte MouthPosition
        {
            get => GetHalfByte(KeyPart4, 9);
            set => SetHalfByte(ref KeyPart4, 9, value);
        }

        public byte MouthFrowSmile
        {
            get => GetHalfByte(KeyPart4, 10);
            set => SetHalfByte(ref KeyPart4, 10, value);
        }

        public byte MouthLipThickness
        {
            get => GetHalfByte(KeyPart4, 11);
            set => SetHalfByte(ref KeyPart4, 11, value);
        }

        public byte MouthForward
        {
            get => GetHalfByte(KeyPart4, 12);
            set => SetHalfByte(ref KeyPart4, 12, value);
        }

        public byte MouthBottomLipShape
        {
            get => GetHalfByte(KeyPart4, 13);
            set => SetHalfByte(ref KeyPart4, 13, value);
        }

        public byte MouthTopLipShape
        {
            get => GetHalfByte(KeyPart4, 14);
            set => SetHalfByte(ref KeyPart4, 14, value);
        }

        public byte MouthLipsConcaveConvex
        {
            get => GetHalfByte(KeyPart4, 15);
            set => SetHalfByte(ref KeyPart4, 15, value);
        }

        public byte MouthJawLine
        {
            get => GetHalfByte(KeyPart5, 0);
            set => SetHalfByte(ref KeyPart5, 0, value);
        }

        public byte MouthJawShape
        {
            get => GetHalfByte(KeyPart5, 1);
            set => SetHalfByte(ref KeyPart5, 1, value);
        }

        public byte MouthJawHeight
        {
            get => GetHalfByte(KeyPart5, 2);
            set => SetHalfByte(ref KeyPart5, 2, value);
        }

        public byte MouthChinForward
        {
            get => GetHalfByte(KeyPart5, 3);
            set => SetHalfByte(ref KeyPart5, 3, value);
        }

        public byte MouthChinShape
        {
            get => GetHalfByte(KeyPart5, 4);
            set => SetHalfByte(ref KeyPart5, 4, value);
        }

        public byte MouthChinLength
        {
            get => GetHalfByte(KeyPart5, 5);
            set => SetHalfByte(ref KeyPart5, 5, value);
        }

        public static byte MaxHairType(bool isFemale) => isFemale ? 0x13 : 0x19;

        // limited set of value
        public byte HairType
        {
            get => GetByte(KeyPart1, 0);
            set => SetByte(ref KeyPart1, 0, value);
        }

        // limited set of value
        public byte HairColor
        {
            get => GetBits(KeyPart1, 30, 6);
            set => SetBits(ref KeyPart1, 30, 6, value);
        }

        // limited set of value
        public byte MarkingsColor
        {
            get => GetByte(KeyPart1, 4);
            set => SetByte(ref KeyPart1, 4, value);
        }

        // limited set of value
        public byte MarkingsType
        {
            get => GetBits(KeyPart1, 24, 6);
            set => SetBits(ref KeyPart1, 24, 6, value);
        }

        // limited set of value
        public byte VoiceType
        {
            get => GetHalfByte(KeyPart8, 0);
            set => SetHalfByte(ref KeyPart8, 0, value);
        }

        // limited set of value
        public byte VoicePitch
        {
            get => GetByte(KeyPart8, 6);
            set => SetByte(ref KeyPart8, 6, value);
        }

        // limited set of value
        public byte SkinColor
        {
            get => GetByte(KeyPart1, 12);
            set => SetByte(ref KeyPart1, 12, value);
        }
    }
}
