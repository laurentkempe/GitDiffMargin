using GitDiffMargin.Git;
using NUnit.Framework;
using Shouldly;

namespace GitDiffMargin.Unit.Tests.Git
{
    // ReSharper disable InconsistentNaming 

    [TestFixture]
    public class HunkRangeTests
    {
        [Test]
        public void HunkRange_HunkNewFileWith3AsContextLines_ExpectHunkNewFile()
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"12,11", 3);

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(14);
            hunkRange.NumberOfLines.ShouldBe(5);
        }

        [Test]
        public void HunkRange_HunkOriginalFile_ExpectHunkOriginalFile()
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"41,0", 0);

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(40);
            hunkRange.NumberOfLines.ShouldBe(0);
        }

        [Test]
        public void HunkRange_HunkOriginalFileWith3AsContextLines_ExpectHunkOriginalFile()
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"12,7", 3);

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(14);
            hunkRange.NumberOfLines.ShouldBe(1);
        }

        [Test]
        public void HunkRange_ValidHunk_ExpectHunkNewFile()
        {
            //Arrange
            //Act
            var hunkRange = new HunkRange(@"42,20", 0);

            //Assert
            hunkRange.StartingLineNumber.ShouldBe(41);
            hunkRange.NumberOfLines.ShouldBe(20);
        }

        [Test]
        public void NumberOfLines_HunkWithoutLineNumber_ExpectDefaultTo1LineNumber()
        {
            //Arrange
            var hunkRange = new HunkRange(@"-18", 0);

            //Act
            var numberOfLines = hunkRange.NumberOfLines;

            //Assert
            numberOfLines.ShouldBe(1);
        }

        [Test]
        public void StartingLineNumber_HunkWithoutLineNumber_ExpectLineNumber()
        {
            //Arrange
            var hunkRange = new HunkRange(@"18", 0);

            //Act
            var startingLineNumber = hunkRange.StartingLineNumber;

            //Assert
            startingLineNumber.ShouldBe(17);
        }
    }

    // ReSharper restore InconsistentNaming 
}