﻿using System;
using System.IO;
using System.Linq;
using NFluent;
using NLog;
using NUnit.Framework;

namespace Registry.Test
{
    [TestFixture]
    internal class TestRegistryHive
    {
        [OneTimeSetUp]
        public void PreTestSetup()
        {
            LogManager.Configuration = null;
        }

        [Test]
        public void CheckHardAndSoftParsingErrors()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            Check.That(Sam.SoftParsingErrors).IsEqualTo(0);
            Check.That(Sam.HardParsingErrors).IsEqualTo(0);
        }


        [Test]
        public void DeletedFindTest()
        {
            var f = @"D:\Sync\RegistryHives\NTUSER.DAT";
            var r = new RegistryHive(f);
            r.RecoverDeleted = true;
            r.ParseHive();

            var ts = "2014-12-08 13:39:33 +00:00";

            var td = DateTimeOffset.Parse(ts);

            var t = r.GetDeletedKey(@"Software\Microsoft\VisualStudio\12.0_Config\Debugger", td.ToString());

            Check.That(t).IsNotNull();
            Check.That(t.NKRecord.IsDeleted).IsTrue();
            ;
        }

        [Test]
        public void HBinSizeShouldMatchReadSize()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            Check.That(Sam.Header.Length).IsEqualTo(Sam.HBinRecordTotalSize);
        }

        [Test]
        public void HBinSizeShouldNotMatchReadSize()
        {
            var r = new RegistryHive(@"..\..\Hives\SAM_DUPENAME");
            //if you don't call parse, it wont match

            Check.That(r.Header.Length).IsNotEqualTo(r.HBinRecordTotalSize);
        }

        [Test]
        public void OneOff()
        {
            var r = new RegistryHive(@"C:\Users\eric\Desktop\UsrClassWorks.7Brokev9.dat");
            r.RecoverDeleted = true;
            r.ParseHive();
        }


        [Test]
        public void RecoverDeletedShouldBeTrue()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            Sam.RecoverDeleted = true;

            Check.That(Sam.RecoverDeleted).IsEqualTo(true);
            Sam.RecoverDeleted = false;
        }


        [Test]
        public void ShouldExportFileAllRecords()
        {
            var UsrclassDeleted = new RegistryHive(@"..\..\Hives\UsrClassDeletedBags.dat");
            UsrclassDeleted.RecoverDeleted = true;
            UsrclassDeleted.FlushRecordListsAfterParse = false;
            UsrclassDeleted.ParseHive();

            UsrclassDeleted.ExportDataToCommonFormat(@"UsrclassDeletedNoDeletedStuff.txt", false);

            Check.That(UsrclassDeleted.Header.Length).IsEqualTo(UsrclassDeleted.HBinRecordTotalSize);
        }


        [Test]
        public void ShouldExportFileDeletedRecords()
        {
            var UsrclassDeleted = new RegistryHive(@"..\..\Hives\UsrClassDeletedBags.dat");
            UsrclassDeleted.RecoverDeleted = true;
            UsrclassDeleted.FlushRecordListsAfterParse = false;
            UsrclassDeleted.ParseHive();

            UsrclassDeleted.ExportDataToCommonFormat(@"UsrclassDeletedDeletedStuff.txt", true);

            Check.That(UsrclassDeleted.Header.Length).IsEqualTo(UsrclassDeleted.HBinRecordTotalSize);
        }

        [Test]
        public void ShouldExportHiveWithRootValues()
        {
            var SamRootValue = new RegistryHive(@"..\..\Hives\SAM_RootValue");
            SamRootValue.FlushRecordListsAfterParse = false;
            SamRootValue.ParseHive();


            SamRootValue.ExportDataToCommonFormat(@"SamRootValueNoDeletedStuff.txt", false);
        }

        [Test]
        public void ShouldExportValuesToFile()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var keys = UsrClass1.FindByValueSize(100000).ToList();

            foreach (var valueBySizeInfo in keys)
            {
                File.WriteAllBytes($"{valueBySizeInfo.Value.ValueName}.bin", valueBySizeInfo.Value.ValueDataRaw);

                Check.That(File.Exists($"{valueBySizeInfo.Value.ValueName}.bin")).IsTrue();
            }
        }

        [Test]
        public void ShouldFind100HitsForURLInKeyAndValueName()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();
            var keyHits = UsrClass1.FindInKeyName("URL").ToList();

            Check.That(keyHits.Count).IsEqualTo(21);

            var valHits = UsrClass1.FindInValueName("URL").ToList();

            Check.That(valHits.Count).IsEqualTo(79);
        }

        [Test]
        public void ShouldFind1248AfterTimeStamp()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();


            var dt = new DateTimeOffset(2014, 11, 13, 15, 51, 17, TimeSpan.FromSeconds(0));
            var hits = UsrClass1.FindByLastWriteTime(dt, null).ToList();

            Check.That(hits.Count).IsEqualTo(14);
        }

        [Test]
        public void ShouldFind1544eforeTimeStamp()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var dt = new DateTimeOffset(2014, 5, 20, 14, 19, 40, TimeSpan.FromSeconds(0));
            var hits = UsrClass1.FindByLastWriteTime(null, dt).ToList();

            Check.That(hits.Count).IsEqualTo(21);
        }

        [Test]
        public void ShouldFind32HitsForFoodInKeyName()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();


            var hits = UsrClass1.FindInKeyName("food").ToList();

            Check.That(hits.Count).IsEqualTo(32);
        }

        [Test]
        public void ShouldFind4HitsFor320033003200InValueDataSlack()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindInValueDataSlack("32-00-33-00-32-00", false, true).ToList();

            Check.That(hits.Count).IsEqualTo(6);
        }

        [Test]
        public void ShouldFindBase64()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindBase64(20).ToList();

            Check.That(hits.Count).IsEqualTo(137);
        }

        [Test]
        public void ShouldFind4HitsForBinaryDataInValueData()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindInValueData("43-74-53-83-24-55-30").ToList();

            Check.That(hits.Count).IsEqualTo(6);

            hits = UsrClass1.FindInValueData("DeB").ToList();

            Check.That(hits.Count).IsEqualTo(28);
        }

        [Test]
        public void ShouldFind4HitsForBinaryDataInValueDataWithRegEx()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindInValueData("04-00-EF-BE", true).ToList();

            Check.That(hits.Count).IsEqualTo(56);

            hits = UsrClass1.FindInValueData("47-4F-4F-4E", true).ToList();

            Check.That(hits.Count).IsEqualTo(4);

            hits = UsrClass1.FindInValueData("44-65-62", true).ToList(); //finds deb

            Check.That(hits.Count).IsEqualTo(2);

            hits = UsrClass1.FindInValueData("44-65-73", true).ToList(); //finds des

            Check.That(hits.Count).IsEqualTo(1);

            hits = UsrClass1.FindInValueData("44-65-(62|73)", true).ToList(); //finds deb or des

            Check.That(hits.Count).IsEqualTo(3);
        }

        [Test]
        public void ShouldFind4HitsForBingXInKeyNamesWithRegEx()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();


            var hits = UsrClass1.FindInKeyName("Microsoft.Bing[FHW]", true).ToList();

            Check.That(hits.Count).IsEqualTo(44);

            hits = UsrClass1.FindInKeyName("Microsoft.Bing[FHW]o", true).ToList();

            Check.That(hits.Count).IsEqualTo(11);
        }

        [Test]
        public void ShouldFind4HitsForBingXInValueDataWithRegEx()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindInValueData("URL:bing[mhs]", true).ToList();

            Check.That(hits.Count).IsEqualTo(3);

            hits = UsrClass1.FindInValueData("URL:bing[mhts]", true).ToList();

            Check.That(hits.Count).IsEqualTo(4);
        }

        [Test]
        public void ShouldFind4HitsForPostboxURLInValueData()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindInValueData("Postbox URL").ToList();

            Check.That(hits.Count).IsEqualTo(4);
        }

        [Test]
        public void ShouldFindAKeyWithClassName()
        {
            var SystemOnDemand = new RegistryHiveOnDemand(@"..\..\Hives\SYSTEM");
            var key =
                SystemOnDemand.GetKey(
                    @"CsiTool-CreateHive-{00000000-0000-0000-0000-000000000000}\ControlSet001\Control\Lsa\Data");

            Check.That(key.ClassName).IsNotEmpty();
        }

        [Test]
        public void ShouldFindAKeyWithoutRootKeyName()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            var key = Sam.GetKey(@"SAM\Domains");

            Check.That(key).IsNotNull();
        }

        [Test]
        public void ShouldFindFiveValuesForSize4096()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var keys = UsrClass1.FindByValueSize(4096).ToList();

            Check.That(keys.Count).IsEqualTo(5);
        }

        [Test]
        public void ShouldFindHitsValueNamesWithRegEx()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindInValueName("(App|Display)Name", true).ToList();

            Check.That(hits.Count).IsEqualTo(326);

            hits = UsrClass1.FindInValueName("Capability(Co|Si)", true).ToList();

            Check.That(hits.Count).IsEqualTo(66);
        }

        [Test]
        public void ShouldFindKeyWithMixedCaseName()
        {
            var UsrClassFtp = new RegistryHiveOnDemand(@"..\..\Hives\UsrClass FTP.dat");

            var key =
                UsrClassFtp.GetKey(
                    @"S-1-5-21-2417227394-2575385136-2411922467-1105_CLAsses\ActivAtableClasses\CLsID");

            Check.That(key).IsNotNull();
        }

        [Test]
        public void ShouldFindKeyWithMixedCaseNameWithoutRootName()
        {
            var UsrClassFtp = new RegistryHiveOnDemand(@"..\..\Hives\UsrClass FTP.dat");

            var key = UsrClassFtp.GetKey(@"ActivAtableClasses\CLsID");

            Check.That(key).IsNotNull();
        }

        [Test]
        public void ShouldFindNoHitsForZimmermanInKeyName()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();


            var hits = UsrClass1.FindInKeyName("Zimmerman").ToList();

            Check.That(hits.Count).IsEqualTo(0);
        }

        [Test]
        public void ShouldFindThreeHitsForMuiCacheInKeyName()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var hits = UsrClass1.FindInKeyName("MuiCache").ToList();

            Check.That(hits.Count).IsEqualTo(3);
        }

        [Test]
        public void ShouldFindTwoBetweenTimeStamp()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var start = new DateTimeOffset(2014, 5, 20, 19, 00, 00, TimeSpan.FromSeconds(0));
            var end = new DateTimeOffset(2014, 5, 20, 23, 59, 59, TimeSpan.FromSeconds(0));
            var hits = UsrClass1.FindByLastWriteTime(start, end).ToList();

            Check.That(hits.Count).IsEqualTo(2);
        }

        [Test]
        public void ShouldFindTwoValuesForSize100000()
        {
            var UsrClass1 = new RegistryHive(@"..\..\Hives\UsrClass 1.dat");
            UsrClass1.RecoverDeleted = true;
            UsrClass1.FlushRecordListsAfterParse = false;
            UsrClass1.ParseHive();

            var keys = UsrClass1.FindByValueSize(100000).ToList();

            Check.That(keys.Count).IsEqualTo(2);
        }

        [Test]
        public void ShouldHaveGoodRegMultiSz()
        {
            var UsrclassDeleted = new RegistryHive(@"..\..\Hives\UsrClassDeletedBags.dat");
            UsrclassDeleted.RecoverDeleted = true;
            UsrclassDeleted.FlushRecordListsAfterParse = false;
            UsrclassDeleted.ParseHive();

            //S-1-5-21-146151751-63468248-1215037915-1000_Classes\Local Settings\MuiCache\6\52C64B7E
            var key =
                UsrclassDeleted.GetKey(
                    @"S-1-5-21-146151751-63468248-1215037915-1000_Classes\Local Settings\MuiCache\6\52C64B7E");

            var val = key.Values.Single(t => t.ValueName == "LanguageList");

            Check.That(val).IsNotNull();

            Check.That(val.ValueName).IsEqualTo("LanguageList");
            Check.That(val.ValueData).IsEqualTo("en-US en");
        }

        [Test]
        public void ShouldHaveHardAndSoftParsingValuesOfZero()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            Check.That(Sam.HardParsingErrors).IsEqualTo(0);
            Check.That(Sam.SoftParsingErrors).IsEqualTo(0);
        }

        [Test]
        public void ShouldHaveHeaderLengthEqualToReadDataSize()
        {
            var UsrclassDeleted = new RegistryHive(@"..\..\Hives\UsrClassDeletedBags.dat");
            UsrclassDeleted.RecoverDeleted = true;
            UsrclassDeleted.FlushRecordListsAfterParse = false;
            UsrclassDeleted.ParseHive();

            Check.That(UsrclassDeleted.Header.Length).IsEqualTo(UsrclassDeleted.HBinRecordTotalSize);
        }

        [Test]
        public void ShouldReturnKeyBasedOnRelativePath()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();
            var key =
                Sam.GetKey(0x418);

            Check.That(key).IsNotNull();
        }

        [Test]
        public void ShouldReturnNullWhenKeyPathNotFound()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            var key =
                Sam.GetKey(@"SAM\Domains\DoesNotExist");

            Check.That(key).IsNull();
        }

        [Test]
        public void ShouldReturnNullWhenRelativeOffsetNotFound()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            var key =
                Sam.GetKey(0x999418);

            Check.That(key).IsNull();
        }

        [Test]
        public void ShouldTakeByteArrayInConstructor()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            var r = new RegistryHive(Sam.FileBytes);

            Check.That(r.Header).IsNotNull();
            Check.That(r.HivePath).IsEqualTo("None");
            Check.That(r.HiveType).IsEqualTo(HiveTypeEnum.Sam);
        }

//        [Test]
//        public void ShouldThrowExceptionNoRootKey()
//        {
//            Check.ThatCode(() =>
//            {
//                var r = new RegistryHive(@"..\..\Hives\SECURITYNoRoot");
//                r.ParseHive();
//            }).Throws<KeyNotFoundException>();
//        }

        [Test]
        public void ShouldThrowExceptionWhenCallingParseHiveTwice()
        {
            Check.ThatCode(() =>
                {
                    var r = new RegistryHive(@"..\..\Hives\SAMBadHBinHeader");
                    r.ParseHive();
                    r.ParseHive();
                })
                .Throws<Exception>();
        }

        [Test]
        public void ShouldThrowExceptionWithBadHbinHeader()
        {
            Check.ThatCode(() =>
                {
                    var r = new RegistryHive(@"..\..\Hives\SAMBadHBinHeader");
                    r.ParseHive();
                })
                .Throws<Exception>();
        }

        [Test]
        public void TestsListRecordsContinued3()
        {
            var UsrClassFtp = new RegistryHiveOnDemand(@"..\..\Hives\UsrClass FTP.dat");

            var key =
                UsrClassFtp.GetKey(
                    @"S-1-5-21-2417227394-2575385136-2411922467-1105_Classes\ActivatableClasses\CLSID");

            Check.That(key).IsNotNull();
        }

        [Test]
        public void VerifyHiveTestShouldPass()
        {
            var Sam = new RegistryHive(@"..\..\Hives\SAM");
            Sam.FlushRecordListsAfterParse = false;
            Sam.ParseHive();

            var m = Sam.Verify();

            Check.That(m.HasValidHeader).IsTrue();
        }
    }
}