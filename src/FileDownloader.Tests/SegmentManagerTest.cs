﻿using System.Collections.Generic;
using System.Linq;
using dotnetCampus.FileDownloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Extensions.Contracts;

namespace FileDownloader.Tests
{
    [TestClass]
    public class SegmentManagerTest
    {
        [ContractTestCase]
        public void Finished()
        {
            "分配两段，在全部下载完成之后，那么下载完成".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);

                var firstDownloadSegment = segmentManager.GetNewDownloadSegment();
                var secondDownloadSegment = segmentManager.GetNewDownloadSegment();
                secondDownloadSegment.DownloadedLength = fileLength / 2;
                firstDownloadSegment.DownloadedLength = fileLength / 2;

                Assert.AreEqual(true, segmentManager.IsFinished());
            });

            "只分配一段，在一段没有完成，那么下载没有完成".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);

                var firstDownloadSegment = segmentManager.GetNewDownloadSegment();
                Assert.AreEqual(fileLength, firstDownloadSegment.RequirementDownloadPoint);
                firstDownloadSegment.DownloadedLength = fileLength / 2;

                Assert.AreEqual(false, segmentManager.IsFinished());
            });

            "只分配一段，在一段下载完成之后，那么下载完成".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);

                var firstDownloadSegment = segmentManager.GetNewDownloadSegment();
                Assert.AreEqual(fileLength, firstDownloadSegment.RequirementDownloadPoint);
                firstDownloadSegment.DownloadedLength = fileLength;

                Assert.AreEqual(true, segmentManager.IsFinished());
            });
        }

        [ContractTestCase]
        public void GetNewDownloadSegment()
        {
            "在获取第二段的时候，如第一段有下载内容，会在下载内容点之后继续第二段".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);

                var firstDownloadSegment = segmentManager.GetNewDownloadSegment();
                Assert.AreEqual(fileLength, firstDownloadSegment.RequirementDownloadPoint);

                const long downloadLength = 100;
                firstDownloadSegment.DownloadedLength = downloadLength;

                var secondDownloadSegment = segmentManager.GetNewDownloadSegment();

                Assert.AreEqual(0, firstDownloadSegment.StartPoint);
                Assert.AreEqual((fileLength / 2) + (downloadLength / 2), firstDownloadSegment.RequirementDownloadPoint);

                Assert.AreEqual((fileLength / 2) + (downloadLength / 2), secondDownloadSegment.StartPoint);
                Assert.AreEqual(fileLength, secondDownloadSegment.RequirementDownloadPoint);
            });

            "多次获取将会不断分段，所有分段合起来是文件大小".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);
                var downloadSegmentList = new List<DownloadSegment>();

                for (int i = 0; i < 100; i++)
                {
                    DownloadSegment downloadSegment = segmentManager.GetNewDownloadSegment();
                    downloadSegmentList.Add(downloadSegment);
                }

                var length = downloadSegmentList.Select(temp => temp.RequirementDownloadPoint - temp.StartPoint).Sum();
                Assert.AreEqual(fileLength, length);
            });

            "在获取第三段的时候，可以获取第一段和第二段的中间".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);

                var firstDownloadSegment = segmentManager.GetNewDownloadSegment();
                Assert.AreEqual(fileLength, firstDownloadSegment.RequirementDownloadPoint);

                var secondDownloadSegment = segmentManager.GetNewDownloadSegment();

                var thirdDownloadSegment = segmentManager.GetNewDownloadSegment();
                Assert.AreEqual(250, thirdDownloadSegment.StartPoint);
            });

            "在获取第二段的时候，将修改第一段需要下载的长度，同时第二段从中间开始".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);

                var firstDownloadSegment = segmentManager.GetNewDownloadSegment();
                Assert.AreEqual(fileLength, firstDownloadSegment.RequirementDownloadPoint);

                var secondDownloadSegment = segmentManager.GetNewDownloadSegment();

                Assert.AreEqual(0, firstDownloadSegment.StartPoint);
                Assert.AreEqual(fileLength / 2, firstDownloadSegment.RequirementDownloadPoint);

                Assert.AreEqual(fileLength / 2, secondDownloadSegment.StartPoint);
                Assert.AreEqual(fileLength, secondDownloadSegment.RequirementDownloadPoint);
            });

            "第一段下载内容的长度是文件的长度".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);

                var downloadSegment = segmentManager.GetNewDownloadSegment();

                Assert.AreEqual(fileLength, downloadSegment.RequirementDownloadPoint);
            });

            "默认第一段下载内容是从零开始".Test(() =>
            {
                const long fileLength = 1000;
                var segmentManager = new SegmentManager(fileLength);
                var downloadSegment = segmentManager.GetNewDownloadSegment();
                Assert.AreEqual(0, downloadSegment.StartPoint);
            });
        }
    }
}
