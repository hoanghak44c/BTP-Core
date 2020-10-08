using System;
using System.Collections.Generic;
using QLBH.Core.Data;

namespace QLBH.Core.DAO
{
    public class AutoGenSerialDao : BaseDAO
    {
        private static AutoGenSerialDao instance;

        private AutoGenSerialDao() { }

        public static AutoGenSerialDao Instance
        {
            get { return instance ?? (instance = new AutoGenSerialDao()); }
        }

        public bool IsUsing(string sanPham, string maTrungTam, string ngayThang, int idNhanVien)
        {
            return GetObjectCommand<int>(@"select count(*) from tbl_gen_serial_nhanvien 
                where  sanpham = :sanpham
                and trungtam = :trungtam
                and ngaytao = :ngaytao
                and idnhanvien = :idnhanvien
                and isused = 0", sanPham, maTrungTam, ngayThang, idNhanVien) > 0;
        }

        public int CountNotUsed(string sanPham, string maTrungTam, string ngayThang, int idNhanVien)
        {
            var count =
                GetObjectCommand<int>(
                    @"select count(*) from tbl_gen_serial_nhanvien 
                where  sanpham = :sanpham
                and trungtam = :trungtam
                and ngaytao = :ngaytao",
                    sanPham, maTrungTam, ngayThang);

            if(count == 0)
            {
                var genCodeInfo =
                    GetObjectCommand<GenCodeInfo>(
                        @"select gencode, last_updated_date as LastUpdatedDate from tbl_gen_serial 
                        where sanpham = :sanpham and trungtam = :trungtam",
                        sanPham,
                        maTrungTam);

                if(genCodeInfo != null)
                {
                    ExecuteCommand(
                        @"update tbl_gen_serial set gencode = 0 
                    where sanpham = :sanpham and trungtam = :trungtam
                        and last_updated_date = :lastupdatedate",
                        sanPham, maTrungTam, genCodeInfo.LastUpdatedDate);

                    ExecuteCommand(
                        @"delete tbl_gen_serial_nhanvien
                    where sanpham = :sanpham and trungtam = :trungtam
                        and ngaytao < :ngaytao",
                        sanPham, maTrungTam, ngayThang);
                }
            }
            return
                GetObjectCommand<int>(
                    @"select count(*) from tbl_gen_serial_nhanvien 
                where  sanpham = :sanpham
                and trungtam = :trungtam
                and ngaytao = :ngaytao
                and idnhanvien = :idnhanvien
                and isused = 0",
                    sanPham, maTrungTam, ngayThang, idNhanVien);
        }

        public class GenCodeInfo
        {
            public int GenCode { get; set; }

            public string SanPham { get; set; }

            public string TrungTam { get; set; }

            public string NgayTao { get; set; }

            public DateTime LastUpdatedDate { get; set; }
        }

        public int IncreaseCount(string sanPham, string maTrungTam)
        {
            var genCodeInfo =
                GetObjectCommand<GenCodeInfo>(
                    @"select gencode, last_updated_date as LastUpdatedDate from tbl_gen_serial 
                        where sanpham = :sanpham and trungtam = :trungtam", sanPham,
                    maTrungTam) ?? new GenCodeInfo();

            if(genCodeInfo.GenCode == 0)
            {
                genCodeInfo.GenCode += 1;

                try
                {
                    ExecuteCommand(
                        @"insert into tbl_gen_serial(sanpham, trungtam, gencode) values(:sanpham, :trungtam, :gencode)",
                        sanPham, maTrungTam, genCodeInfo.GenCode);

                    return genCodeInfo.GenCode;
                }
                catch (Exception)
                {
                    //violate constraint key

                    return IncreaseCount(sanPham, maTrungTam);
                }
            }

            var resultAffected = 0;

            while(resultAffected == 0)
            {
                genCodeInfo.GenCode += 1;

                resultAffected = ExecuteCommand(
                    @"update tbl_gen_serial set gencode = :gencode 
                    where sanpham = :sanpham and trungtam = :trungtam
                        and last_updated_date = :lastupdatedate",
                    genCodeInfo.GenCode, sanPham, maTrungTam, genCodeInfo.LastUpdatedDate);

                if (resultAffected != 0) continue;

                genCodeInfo =
                    GetObjectCommand<GenCodeInfo>(
                        @"select gencode, last_updated_date as LastUpdatedDate from tbl_gen_serial 
                        where sanpham = :sanpham and trungtam = :trungtam",
                        sanPham,
                        maTrungTam);
            }

            return genCodeInfo.GenCode;
        }

        public void SaveToManaged(string sanPham, string maTrungTam, string ngayThang, int idNhanVien, int seqNum, int isUsed)
        {
            ExecuteCommand(
                @"insert into tbl_gen_serial_nhanvien(sanpham, trungtam, ngaytao, idnhanvien, gencode, isused) 
                values(:sanpham, :trungtam, :ngaytao, :idnhanvien, :gencode, :isused)",
                sanPham, maTrungTam, ngayThang, idNhanVien, seqNum, isUsed);
        }

        public List<GenCodeInfo> GetListGenCodeInfo(string sanPham, string maTrungTam, int idNhanVien)
        {
            return
                GetListCommand<GenCodeInfo>(
                    @"select sanpham, trungtam, ngaytao, gencode from tbl_gen_serial_nhanvien
                where sanpham = :sanpham
                    and trungtam = :trungtam
                    and idnhanvien = :idnhanvien
                    and isused = 0", sanPham, maTrungTam, idNhanVien);
        }

        public IEnumerable<GenCodeInfo> GetListGenCodeInfoInNumRow(string sanPham, string maTrungTam, int idNhanVien, int numRow)
        {
            return
                GetListCommand<GenCodeInfo>(
                    @"select sanpham, trungtam, ngaytao, gencode from tbl_gen_serial_nhanvien
                where sanpham = :sanpham
                    and trungtam = :trungtam
                    and idnhanvien = :idnhanvien
                    and isused = 0
                    and rownum <= :numrow", sanPham, maTrungTam, idNhanVien, numRow);
        }

        public void DeleteSerial(string s)
        {
            ExecuteCommand(
                @"delete tbl_gen_serial_nhanvien where sanpham || ' ' || trungtam || ' ' || ngaytao || ' ' || gencode = :serial",
                s);
        }

        public void UseSerial(string s)
        {
            ExecuteCommand(
                @"update tbl_gen_serial_nhanvien set isused = 1 where sanpham || ' ' || trungtam || ' ' || ngaytao || ' ' || gencode = :serial",
                s);
        }
    }
}