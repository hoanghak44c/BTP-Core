using System;
using System.Collections.Generic;
using System.Text;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QLBH.Core
{
    public class VNCodeConverter
    {
        private static VNCodeConverter instance;
        public static VNCodeConverter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VNCodeConverter();
                }
                return instance;
            }
        }
        //khai báo các ký tự dấu VIQR
        private const int VN_CAA = 0x27; //sắc
        private const int VN_CGA = 0x60; //huyền
        private const int VN_CHA = 0x3f; //hỏi
        private const int VN_CTD = 0x7e; //ngã
        private const int VN_CDB = 0x2e; //nặng
        private const int VN_BRE = 0x28; //dấu ă
        private const int VN_CIR = 0x5e; //dấu â
        private const int VN_HOR = 0x2b; //dấu ư

        //khai báo các ký tự dấu Unicode Composite
        private const int VN_CAA1 = 0x301; //sắc
        private const int VN_CGA1 = 0x300; //huyền
        private const int VN_CHA1 = 0x309; //hỏi
        private const int VN_CTD1 = 0x303; //ngã
        private const int VN_CDB1 = 0x323; //nặng
        
        //hàm chuyển chuỗi VIQR sang Unicode dựng sẵn
        //dùng thuật toán duyệt tuần tự
        public int VIQR2PreCompose(String src, ref String dst)
        {
            int i = 0; uint c, a1, a2; int idxo = 0; int cnt = src.Length;
            char[] buf = new char[cnt + 1];
            while (i < cnt)
            {
                c = src[i++];
                switch (c)
                {
                    case 'a':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xe1; goto precomp;
                            case VN_CGA: c = 0xe0; goto precomp;
                            case VN_CHA: c = 0x1ea3; goto precomp;
                            case VN_CTD: c = 0xe3; goto precomp;
                            case VN_CDB: c = 0x1ea1; goto precomp;
                            case VN_BRE:
                                c = 0x103;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1eaf; goto precomp;
                                    case VN_CGA: c = 0x1eb1; goto precomp;
                                    case VN_CHA: c = 0x1eb3; goto precomp;
                                    case VN_CTD: c = 0x1eb5; goto precomp;
                                    case VN_CDB: c = 0x1eb7; goto precomp;
                                    default: i--; goto precomp;
                                }
                            case VN_CIR:
                                c = 0xe2;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ea5; goto precomp;
                                    case VN_CGA: c = 0x1ea7; goto precomp;
                                    case VN_CHA: c = 0x1ea9; goto precomp;
                                    case VN_CTD: c = 0x1eab; goto precomp;
                                    case VN_CDB: c = 0x1ead; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'A':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xc1; goto precomp;
                            case VN_CGA: c = 0xc0; goto precomp;
                            case VN_CHA: c = 0x1ea2; goto precomp;
                            case VN_CTD: c = 0xc3; goto precomp;
                            case VN_CDB: c = 0x1ea0; goto precomp;
                            case VN_BRE:
                                c = 0x102;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1eae; goto precomp;
                                    case VN_CGA: c = 0x1eb0; goto precomp;
                                    case VN_CHA: c = 0x1eb2; goto precomp;
                                    case VN_CTD: c = 0x1eb4; goto precomp;
                                    case VN_CDB: c = 0x1eb6; goto precomp;
                                    default: i--; goto precomp;
                                }
                            case VN_CIR:
                                c = 0xc2;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ea4; goto precomp;
                                    case VN_CGA: c = 0x1ea6; goto precomp;
                                    case VN_CHA: c = 0x1ea8; goto precomp;
                                    case VN_CTD: c = 0x1eaa; goto precomp;
                                    case VN_CDB: c = 0x1eac; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'e':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xe9; goto precomp;
                            case VN_CGA: c = 0xe8; goto precomp;
                            case VN_CHA: c = 0x1ebb; goto precomp;
                            case VN_CTD: c = 0x1ebd; goto precomp;
                            case VN_CDB: c = 0x1eb9; goto precomp;
                            case VN_CIR:
                                c = 0xea;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ebf; goto precomp;
                                    case VN_CGA: c = 0x1ec1; goto precomp;
                                    case VN_CHA: c = 0x1ec3; goto precomp;
                                    case VN_CTD: c = 0x1ec5; goto precomp;
                                    case VN_CDB: c = 0x1ec7; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'E':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xc9; goto precomp;
                            case VN_CGA: c = 0xc8; goto precomp;
                            case VN_CHA: c = 0x1eba; goto precomp;
                            case VN_CTD: c = 0x1ebc; goto precomp;
                            case VN_CDB: c = 0x1eb8; goto precomp;
                            case VN_CIR:
                                c = 0xca;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ebe; goto precomp;
                                    case VN_CGA: c = 0x1ec0; goto precomp;
                                    case VN_CHA: c = 0x1ec2; goto precomp;
                                    case VN_CTD: c = 0x1ec4; goto precomp;
                                    case VN_CDB: c = 0x1ec6; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'i':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xed; goto precomp;
                            case VN_CGA: c = 0xec; goto precomp;
                            case VN_CHA: c = 0x1ec9; goto precomp;
                            case VN_CTD: c = 0x129; goto precomp;
                            case VN_CDB: c = 0x1ecb; goto precomp;
                            default: i--; goto precomp;
                        }
                    case 'I':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xcd; goto precomp;
                            case VN_CGA: c = 0xcc; goto precomp;
                            case VN_CHA: c = 0x1ec8; goto precomp;
                            case VN_CTD: c = 0x128; goto precomp;
                            case VN_CDB: c = 0x1eca; goto precomp;
                            default: i--; goto precomp;
                        }
                    case 'o':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xf3; goto precomp;
                            case VN_CGA: c = 0xf2; goto precomp;
                            case VN_CHA: c = 0x1ecf; goto precomp;
                            case VN_CTD: c = 0xf5; goto precomp;
                            case VN_CDB: c = 0x1ecd; goto precomp;
                            case VN_CIR:
                                c = 0xf4;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ed1; goto precomp;
                                    case VN_CGA: c = 0x1ed3; goto precomp;
                                    case VN_CHA: c = 0x1ed5; goto precomp;
                                    case VN_CTD: c = 0x1ed7; goto precomp;
                                    case VN_CDB: c = 0x1ed9; goto precomp;
                                    default: i--; goto precomp;
                                }
                            case VN_HOR:
                                c = 0x1a1;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1edb; goto precomp;
                                    case VN_CGA: c = 0x1edd; goto precomp;
                                    case VN_CHA: c = 0x1edf; goto precomp;
                                    case VN_CTD: c = 0x1ee1; goto precomp;
                                    case VN_CDB: c = 0x1ee3; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'O':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xd3; goto precomp;
                            case VN_CGA: c = 0xd2; goto precomp;
                            case VN_CHA: c = 0x1ece; goto precomp;
                            case VN_CTD: c = 0xd5; goto precomp;
                            case VN_CDB: c = 0x1ecc; goto precomp;
                            case VN_CIR:
                                c = 0xd4;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ed0; goto precomp;
                                    case VN_CGA: c = 0x1ed2; goto precomp;
                                    case VN_CHA: c = 0x1ed4; goto precomp;
                                    case VN_CTD: c = 0x1ed6; goto precomp;
                                    case VN_CDB: c = 0x1ed8; goto precomp;
                                    default: i--; goto precomp;
                                }
                            case VN_HOR:
                                c = 0x1a0;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1eda; goto precomp;
                                    case VN_CGA: c = 0x1edc; goto precomp;
                                    case VN_CHA: c = 0x1ede; goto precomp;
                                    case VN_CTD: c = 0x1ee0; goto precomp;
                                    case VN_CDB: c = 0x1ee2; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'u':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xfa; goto precomp;
                            case VN_CGA: c = 0xf9; goto precomp;
                            case VN_CHA: c = 0x1ee7; goto precomp;
                            case VN_CTD: c = 0x169; goto precomp;
                            case VN_CDB: c = 0x1ee5; goto precomp;
                            case VN_HOR:
                                c = 0x1b0;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ee9; goto precomp;
                                    case VN_CGA: c = 0x1eeb; goto precomp;
                                    case VN_CHA: c = 0x1eed; goto precomp;
                                    case VN_CTD: c = 0x1eef; goto precomp;
                                    case VN_CDB: c = 0x1ef1; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'U':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xda; goto precomp;
                            case VN_CGA: c = 0xd9; goto precomp;
                            case VN_CHA: c = 0x1ee6; goto precomp;
                            case VN_CTD: c = 0x168; goto precomp;
                            case VN_CDB: c = 0x1ee4; goto precomp;
                            case VN_HOR:
                                c = 0x1af;
                                if (i == cnt) goto precomp;
                                a2 = src[i++];
                                switch (a2)
                                {
                                    case VN_CAA: c = 0x1ee8; goto precomp;
                                    case VN_CGA: c = 0x1eea; goto precomp;
                                    case VN_CHA: c = 0x1eec; goto precomp;
                                    case VN_CTD: c = 0x1eee; goto precomp;
                                    case VN_CDB: c = 0x1ef0; goto precomp;
                                    default: i--; goto precomp;
                                }
                            default: i--; goto precomp;
                        }
                    case 'y':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xfd; goto precomp;
                            case VN_CGA: c = 0x1ef3; goto precomp;
                            case VN_CHA: c = 0x1ef7; goto precomp;
                            case VN_CTD: c = 0x1ef9; goto precomp;
                            case VN_CDB: c = 0x1ef5; goto precomp;
                            default: i--; goto precomp;
                        }
                    case 'Y':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case VN_CAA: c = 0xdd; goto precomp;
                            case VN_CGA: c = 0x1ef2; goto precomp;
                            case VN_CHA: c = 0x1ef6; goto precomp;
                            case VN_CTD: c = 0x1ef8; goto precomp;
                            case VN_CDB: c = 0x1ef4; goto precomp;
                            default: i--; goto precomp;
                        }
                    case 'd':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case 0x2d: c = 0x111; goto precomp;
                            default: i--; goto precomp;
                        }
                    case 'D':
                        if (i == cnt) goto precomp;
                        a1 = src[i++];
                        switch (a1)
                        {
                            case 0x2d: c = 0x110; goto precomp;
                            default: i--; goto precomp;
                        }
                }
            precomp:
                buf[idxo++] = (char)c;
            }
            buf[idxo] = (char)0;
            dst = new String(buf);
            return idxo;
        } //end of VIRQ2PreCompose

        //định nghĩa bảng các ký tự Unicode có dấu
        uint[] VnPreComposedChar = new uint[134] {
 0x00c0, 0x00c1, 0x00c2, 0x00c3, 0x00c8, 0x00c9, 0x00ca, 0x00cc, 0x00cd, 0x00d2, 0x00d3, 0x00d4, 0x00d5, 0x00d9, 0x00da, 0x00dd, 0x00e0, 0x00e1, 0x00e2, 0x00e3, 0x00e8, 0x00e9, 0x00ea, 0x00ec, 0x00ed, 0x00f2, 0x00f3, 0x00f4, 0x00f5, 0x00f9, 0x00fa, 0x00fd, 0x0102, 0x0103, 0x0110, 0x0111, 0x0128, 0x0129, 0x0168, 0x0169, 0x01a0, 0x01a1, 0x01af, 0x01b0, 0x1ea0, 0x1ea1, 0x1ea2, 0x1ea3, 0x1ea4, 0x1ea5, 0x1ea6, 0x1ea7, 0x1ea8, 0x1ea9, 0x1eaa, 0x1eab, 0x1eac, 0x1ead, 0x1eae, 0x1eaf, 0x1eb0, 0x1eb1, 0x1eb2, 0x1eb3, 0x1eb4, 0x1eb5, 0x1eb6, 0x1eb7, 0x1eb8, 0x1eb9, 0x1eba, 0x1ebb, 0x1ebc, 0x1ebd, 0x1ebe, 0x1ebf, 0x1ec0, 0x1ec1, 0x1ec2, 0x1ec3, 0x1ec4, 0x1ec5, 0x1ec6, 0x1ec7, 0x1ec8, 0x1ec9, 0x1eca, 0x1ecb, 0x1ecc, 0x1ecd, 0x1ece, 0x1ecf, 0x1ed0, 0x1ed1, 0x1ed2, 0x1ed3, 0x1ed4, 0x1ed5, 0x1ed6, 0x1ed7, 0x1ed8, 0x1ed9, 0x1eda, 0x1edb, 0x1edc, 0x1edd, 0x1ede, 0x1edf, 0x1ee0, 0x1ee1, 0x1ee2, 0x1ee3, 0x1ee4, 0x1ee5, 0x1ee6, 0x1ee7, 0x1ee8, 0x1ee9, 0x1eea, 0x1eeb, 0x1eec, 0x1eed, 0x1eee, 0x1eef, 0x1ef0, 0x1ef1, 0x1ef2, 0x1ef3, 0x1ef4, 0x1ef5, 0x1ef6, 0x1ef7, 0x1ef8, 0x1ef9
 };

        //định nghĩa bảng các ký tự VIQR tương ứng
        uint[][] VnVIQRChar = new uint[134][] {
 new uint[] {0x0041,VN_CGA,0}, new uint[] {0x0041,VN_CAA,0}, new uint[] {0x0041,VN_CIR,0}, new uint[] {0x0041,VN_CTD,0}, new uint[] {0x0045,VN_CGA,0}, new uint[] {0x0045,VN_CAA,0}, new uint[] {0x0045,VN_CIR,0}, new uint[] {0x0049,VN_CGA,0}, new uint[] {0x0049,VN_CAA,0}, new uint[] {0x004f,VN_CGA,0}, new uint[] {0x004f,VN_CAA,0}, new uint[] {0x004f,VN_CIR,0}, new uint[] {0x004f,VN_CTD,0}, new uint[] {0x0055,VN_CGA,0}, new uint[] {0x0055,VN_CAA,0}, new uint[] {0x0059,VN_CAA,0}, new uint[] {0x0061,VN_CGA,0}, new uint[] {0x0061,VN_CAA,0}, new uint[] {0x0061,VN_CIR,0}, new uint[] {0x0061,VN_CTD,0}, new uint[] {0x0065,VN_CGA,0}, new uint[] {0x0065,VN_CAA,0}, new uint[] {0x0065,VN_CIR,0}, new uint[] {0x0069,VN_CGA,0}, new uint[] {0x0069,VN_CAA,0}, new uint[] {0x006f,VN_CGA,0}, new uint[] {0x006f,VN_CAA,0}, new uint[] {0x006f,VN_CIR,0}, new uint[] {0x006f,VN_CTD,0}, new uint[] {0x0075,VN_CGA,0}, new uint[] {0x0075,VN_CAA,0}, new uint[] {0x0079,VN_CAA,0}, new uint[] {0x0041,VN_BRE,0}, new uint[] {0x0061,VN_BRE,0}, new uint[] {0x044,(uint)'-',0}, new uint[] {0x064,(uint)'-',0}, new uint[] {0x0049,VN_CTD,0}, new uint[] {0x0069,VN_CTD,0}, new uint[] {0x0055,VN_CTD,0}, new uint[] {0x0075,VN_CTD,0}, new uint[] {0x004f,VN_HOR,0}, new uint[] {0x006f,VN_HOR,0}, new uint[] {0x0055,VN_HOR,0}, new uint[] {0x0075,VN_HOR,0}, new uint[] {0x0041,VN_CDB,0}, new uint[] {0x0061,VN_CDB,0}, new uint[] {0x0041,VN_CHA,0}, new uint[] {0x0061,VN_CHA,0}, new uint[] {0x0041,VN_CIR,VN_CAA,0}, new uint[] {0x0061,VN_CIR,VN_CAA,0}, new uint[] {0x0041,VN_CIR,VN_CGA,0}, new uint[] {0x0061,VN_CIR,VN_CGA,0}, new uint[] {0x0041,VN_CIR,VN_CHA,0}, new uint[] {0x0061,VN_CIR,VN_CHA,0}, new uint[] {0x0041,VN_CIR,VN_CTD,0}, new uint[] {0x0061,VN_CIR,VN_CTD,0}, new uint[] {0x0041,VN_CIR,VN_CDB,0}, new uint[] {0x0061,VN_CIR,VN_CDB,0}, new uint[] {0x0041,VN_BRE,VN_CAA,0}, new uint[] {0x0061,VN_BRE,VN_CAA,0}, new uint[] {0x0041,VN_BRE,VN_CGA,0}, new uint[] {0x0061,VN_BRE,VN_CGA,0}, new uint[] {0x0041,VN_BRE,VN_CHA,0}, new uint[] {0x0061,VN_BRE,VN_CHA,0}, new uint[] {0x0041,VN_BRE,VN_CTD,0}, new uint[] {0x0061,VN_BRE,VN_CTD,0}, new uint[] {0x0041,VN_BRE,VN_CDB,0}, new uint[] {0x0061,VN_BRE,VN_CDB,0}, new uint[] {0x0045,VN_CDB,0}, new uint[] {0x0065,VN_CDB,0}, new uint[] {0x0045,VN_CHA,0}, new uint[] {0x0065,VN_CHA,0}, new uint[] {0x0045,VN_CTD,0}, new uint[] {0x0065,VN_CTD,0}, new uint[] {0x0045,VN_CIR,VN_CAA,0}, new uint[] {0x0065,VN_CIR,VN_CAA,0}, new uint[] {0x0045,VN_CIR,VN_CGA,0}, new uint[] {0x0065,VN_CIR,VN_CGA,0}, new uint[] {0x0045,VN_CIR,VN_CHA,0}, new uint[] {0x0065,VN_CIR,VN_CHA,0}, new uint[] {0x0045,VN_CIR,VN_CTD,0}, new uint[] {0x0065,VN_CIR,VN_CTD,0}, new uint[] {0x0045,VN_CIR,VN_CDB,0}, new uint[] {0x0065,VN_CIR,VN_CDB,0}, new uint[] {0x0049,VN_CHA,0}, new uint[] {0x0069,VN_CHA,0}, new uint[] {0x0049,VN_CDB,0}, new uint[] {0x0069,VN_CDB,0}, new uint[] {0x004f,VN_CDB,0}, new uint[] {0x006f,VN_CDB,0}, new uint[] {0x004f,VN_CHA,0}, new uint[] {0x006f,VN_CHA,0}, new uint[] {0x004f,VN_CIR,VN_CAA,0}, new uint[] {0x006f,VN_CIR,VN_CAA,0}, new uint[] {0x004f,VN_CIR,VN_CGA,0}, new uint[] {0x006f,VN_CIR,VN_CGA,0}, new uint[] {0x004f,VN_CIR,VN_CHA,0}, new uint[] {0x006f,VN_CIR,VN_CHA,0}, new uint[] {0x004f,VN_CIR,VN_CTD,0}, new uint[] {0x006f,VN_CIR,VN_CTD,0}, new uint[] {0x004f,VN_CIR,VN_CDB,0}, new uint[] {0x006f,VN_CIR,VN_CDB,0}, new uint[] {0x004f,VN_HOR,VN_CAA,0}, new uint[] {0x006f,VN_HOR,VN_CAA,0}, new uint[] {0x004f,VN_HOR,VN_CGA,0}, new uint[] {0x006f,VN_HOR,VN_CGA,0}, new uint[] {0x004f,VN_HOR,VN_CHA,0}, new uint[] {0x006f,VN_HOR,VN_CHA,0}, new uint[] {0x004f,VN_HOR,VN_CTD,0}, new uint[] {0x006f,VN_HOR,VN_CTD,0}, new uint[] {0x004f,VN_HOR,VN_CDB,0}, new uint[] {0x006f,VN_HOR,VN_CDB,0}, new uint[] {0x0055,VN_CDB,0}, new uint[] {0x0075,VN_CDB,0}, new uint[] {0x0055,VN_CHA,0}, new uint[] {0x0075,VN_CHA,0}, new uint[] {0x0055,VN_HOR,VN_CAA,0}, new uint[] {0x0075,VN_HOR,VN_CAA,0}, new uint[] {0x0055,VN_HOR,VN_CGA,0}, new uint[] {0x0075,VN_HOR,VN_CGA,0}, new uint[] {0x0055,VN_HOR,VN_CHA,0}, new uint[] {0x0075,VN_HOR,VN_CHA,0}, new uint[] {0x0055,VN_HOR,VN_CTD,0}, new uint[] {0x0075,VN_HOR,VN_CTD,0}, new uint[] {0x0055,VN_HOR,VN_CDB,0}, new uint[] {0x0075,VN_HOR,VN_CDB,0}, new uint[] {0x0059,VN_CGA,0}, new uint[] {0x0079,VN_CGA,0}, new uint[] {0x0059,VN_CDB,0}, new uint[] {0x0079,VN_CDB,0}, new uint[] {0x0059,VN_CHA,0}, new uint[] {0x0079,VN_CHA,0}, new uint[] {0x0059,VN_CTD,0}, new uint[] {0x0079,VN_CTD,0}
 };
        //định nghĩa bảng các ký tự Unicode composite tương ứng
        uint[][] VnUCOMChar = new uint[134][] {
 new uint[] {0x0041,VN_CGA1,0}, new uint[] {0x0041,VN_CAA1,0}, new uint[] {0x0041,0x00c2,0}, new uint[] {0x0041,VN_CTD1,0}, new uint[] {0x0045,VN_CGA1,0}, new uint[] {0x0045,VN_CAA1,0}, new uint[] {0x00ca,0,0}, new uint[] {0x0049,VN_CGA1,0}, new uint[] {0x0049,VN_CAA1,0}, new uint[] {0x004f,VN_CGA1,0}, new uint[] {0x004f,VN_CAA1,0}, new uint[] {0x00d4,0,0}, new uint[] {0x004f,VN_CTD1,0}, new uint[] {0x0055,VN_CGA1,0}, new uint[] {0x0055,VN_CAA1,0}, new uint[] {0x0059,VN_CAA1,0}, new uint[] {0x0061,VN_CGA1,0}, new uint[] {0x0061,VN_CAA1,0}, new uint[] {0x00e2,0,0}, new uint[] {0x0061,VN_CTD1,0}, new uint[] {0x0065,VN_CGA1,0}, new uint[] {0x0065,VN_CAA1,0}, new uint[] {0x00c2,0,0}, new uint[] {0x0069,VN_CGA1,0}, new uint[] {0x0069,VN_CAA1,0}, new uint[] {0x006f,VN_CGA1,0}, new uint[] {0x006f,VN_CAA1,0}, new uint[] {0x00f4,0,0}, new uint[] {0x006f,VN_CTD1,0}, new uint[] {0x0075,VN_CGA1,0}, new uint[] {0x0075,VN_CAA1,0}, new uint[] {0x0079,VN_CAA1,0}, new uint[] {0x0102,0,0}, new uint[] {0x0103,0,0}, new uint[] {0x0110,0,0}, new uint[] {0x0111,0,0}, new uint[] {0x0049,VN_CTD1,0}, new uint[] {0x0069,VN_CTD1,0}, new uint[] {0x0055,VN_CTD1,0}, new uint[] {0x0075,VN_CTD1,0}, new uint[] {0x01a0,0,0}, new uint[] {0x01a1,0,0}, new uint[] {0x01af,0,0}, new uint[] {0x01b0,0,0}, new uint[] {0x0041,VN_CDB1,0}, new uint[] {0x0061,VN_CDB1,0}, new uint[] {0x0041,VN_CHA1,0}, new uint[] {0x0061,VN_CHA1,0}, new uint[] {0x00c2,VN_CAA1,0}, new uint[] {0x00e2,VN_CAA1,0}, new uint[] {0x00c2,VN_CGA1,0}, new uint[] {0x00e2,VN_CGA1,0}, new uint[] {0x00c2,VN_CHA1,0}, new uint[] {0x00e2,VN_CHA1,0}, new uint[] {0x00c2,VN_CTD1,0}, new uint[] {0x00e2,VN_CTD1,0}, new uint[] {0x00c2,VN_CDB1,0}, new uint[] {0x00e2,VN_CDB1,0}, new uint[] {0x0102,VN_CAA1,0}, new uint[] {0x0103,VN_CAA1,0}, new uint[] {0x0102,VN_CGA1,0}, new uint[] {0x0103,VN_CGA1,0}, new uint[] {0x0102,VN_CHA1,0}, new uint[] {0x0103,VN_CHA1,0}, new uint[] {0x0102,VN_CTD1,0}, new uint[] {0x0103,VN_CTD1,0}, new uint[] {0x0102,VN_CDB1,0}, new uint[] {0x0103,VN_CDB1,0}, new uint[] {0x0045,VN_CDB1,0}, new uint[] {0x0065,VN_CDB1,0}, new uint[] {0x0045,VN_CHA1,0}, new uint[] {0x0065,VN_CHA1,0}, new uint[] {0x0045,VN_CTD1,0}, new uint[] {0x0065,VN_CTD1,0}, new uint[] {0x00ca,VN_CAA1,0}, new uint[] {0x00ea,VN_CAA1,0}, new uint[] {0x00ca,VN_CGA1,0}, new uint[] {0x00ea,VN_CGA1,0}, new uint[] {0x00ca,VN_CHA1,0}, new uint[] {0x00ea,VN_CHA1,0}, new uint[] {0x00ca,VN_CTD1,0}, new uint[] {0x00ea,VN_CTD1,0}, new uint[] {0x00ca,VN_CDB1,0}, new uint[] {0x00ea,VN_CDB1,0}, new uint[] {0x0049,VN_CHA1,0}, new uint[] {0x0069,VN_CHA1,0}, new uint[] {0x0049,VN_CDB1,0}, new uint[] {0x0069,VN_CDB1,0}, new uint[] {0x004f,VN_CDB1,0}, new uint[] {0x006f,VN_CDB1,0}, new uint[] {0x004f,VN_CHA1,0}, new uint[] {0x006f,VN_CHA1,0}, new uint[] {0x00d4,VN_CAA1,0}, new uint[] {0x00f4,VN_CAA1,0}, new uint[] {0x00d4,VN_CGA1,0}, new uint[] {0x00f4,VN_CGA1,0}, new uint[] {0x00d4,VN_CHA1,0}, new uint[] {0x00f4,VN_CHA1,0}, new uint[] {0x00d4,VN_CTD1,0}, new uint[] {0x00f4,VN_CTD1,0}, new uint[] {0x00d4,VN_CDB1,0}, new uint[] {0x00f4,VN_CDB1,0}, new uint[] {0x01A0,VN_CAA1,0}, new uint[] {0x01A1,VN_CAA1,0}, new uint[] {0x01A0,VN_CGA1,0}, new uint[] {0x01A1,VN_CGA1,0}, new uint[] {0x01A0,VN_CHA1,0}, new uint[] {0x01A1,VN_CHA1,0}, new uint[] {0x01A0,VN_CTD1,0}, new uint[] {0x01A1,VN_CTD1,0}, new uint[] {0x01A0,VN_CDB1,0}, new uint[] {0x01A1,VN_CDB1,0}, new uint[] {0x0055,VN_CDB1,0}, new uint[] {0x0075,VN_CDB1,0}, new uint[] {0x0055,VN_CHA1,0}, new uint[] {0x0075,VN_CHA1,0}, new uint[] {0x01af,VN_CAA1,0}, new uint[] {0x01b0,VN_CAA1,0}, new uint[] {0x01af,VN_CGA1,0}, new uint[] {0x01b0,VN_CGA1,0}, new uint[] {0x01af,VN_CHA1,0}, new uint[] {0x01b0,VN_CHA1,0}, new uint[] {0x01af,VN_CTD1,0}, new uint[] {0x01b0,VN_CTD1,0}, new uint[] {0x01af,VN_CDB1,0}, new uint[] {0x01b0,VN_CDB1,0}, new uint[] {0x0059,VN_CGA1,0}, new uint[] {0x0079,VN_CGA1,0}, new uint[] {0x0059,VN_CDB1,0}, new uint[] {0x0079,VN_CDB1,0}, new uint[] {0x0059,VN_CHA1,0}, new uint[] {0x0079,VN_CHA1,0}, new uint[] {0x0059,VN_CTD1,0}, new uint[] {0x0079,VN_CTD1,0}                                              
 };

        public bool IsSignedLetter(char c)
        {
            int i, j, min = 0, max = 133;
            uint ccode;
            while (min <= max)
            {
                j = (max + min) >> 1;
                ccode = (uint)c;
                if (c == VnPreComposedChar[j]) break;
                if (c < VnPreComposedChar[j]) max = j - 1;
                else min = j + 1;
            }
            if (min <= max) return true;
            return false;
        }

        //hàm chuyển chuỗi từ Unicode sang VIQR dùng bảng tra
        public int PreCompose2VIQR(String src, ref String dst)
        {
            int i, j, min, max;
            int idxo = 0;
            int cnt = src.Length;
            uint ccode;
            char[] buf = new char[3 * cnt + 1];
            j = 0;
            for (i = 0; i < cnt; i++)
            {
                min = 0; max = 133;
                while (min <= max)
                {
                    j = (max + min) >> 1;
                    ccode = (uint)src[i];
                    if (src[i] == VnPreComposedChar[j]) break;
                    if (src[i] < VnPreComposedChar[j]) max = j - 1;
                    else min = j + 1;
                }
                if (min <= max)
                {
                    int k = 0;
                    while (VnVIQRChar[j][k] != 0)
                        buf[idxo++] = (char)VnVIQRChar[j][k++];
                }
                else buf[idxo++] = src[i];
            }
            buf[idxo] = (char)0;
            dst = new String(buf);
            return idxo;
        }

        //hàm chuyển chuỗi từ Unicode sang Unicode composite dùng bảng tra
        public int PreCompose2UCOM(String src, ref String dst)
        {
            int i, j, min, max;
            int idxo = 0;
            int cnt = src.Length;
            uint ccode;
            char[] buf = new char[3 * cnt + 1];
            j = 0;
            for (i = 0; i < cnt; i++)
            {
                min = 0; max = 133;
                while (min <= max)
                {
                    j = (max + min) >> 1;
                    ccode = (uint)src[i];
                    if (src[i] == VnPreComposedChar[j]) break;
                    if (src[i] < VnPreComposedChar[j]) max = j - 1;
                    else min = j + 1;
                }
                if (min <= max)
                {
                    int k = 0;
                    while (VnUCOMChar[j][k] != 0)
                        buf[idxo++] = (char)VnUCOMChar[j][k++];
                }
                else buf[idxo++] = src[i];
            }
            buf[idxo] = (char)0;
            dst = new String(buf, 0, idxo);
            return idxo;
        }
    }

    //[TestClass]
    //public class TestConverter
    //{
    //    string unicodeDungSan = "ẩm thực";
    //    string unicodeToHop = "ẩm thực";
    //    [TestMethod]
    //    public void TestNotContain()
    //    {
    //        //char.IsWhiteSpace()
    //        Assert.AreEqual(false, unicodeToHop.Contains(unicodeDungSan));
    //    }

    //    [TestMethod]
    //    public void TestContain()
    //    {
    //        VIQR2Unicode t = new VIQR2Unicode();
    //        string temp = String.Empty;
    //        t.PreCompose2UCOM(unicodeDungSan, ref temp);
    //        Assert.AreEqual(true, unicodeToHop.Contains(temp));
    //    }        
    //}
}
