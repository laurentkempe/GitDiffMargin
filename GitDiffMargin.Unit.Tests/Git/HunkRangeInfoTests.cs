#region using

using System.Collections.Generic;
using GitDiffMargin.Git;
using NUnit.Framework;
using Shouldly;

#endregion

namespace GitDiffMargin.Unit.Tests.Git
{
    // ReSharper disable InconsistentNaming 

    [TestFixture]
    public class HunkRangeInfoTests
    {
        [Test]
        public void IsAddition_AllDiffLinesWithStartsWithPlusSign_ExpectedTrue()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+ ", "+ " }.ToArray());

            //Act
            bool isAddition = hunkRangeInfo.IsAddition;

            //Assert
            isAddition.ShouldBe(true);
        }

        [Test]
        public void IsAddition_NotAllDiffLinesStartsWithPlusSign_ExpectedFalse()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+ ", "- " }.ToArray());

            //Act
            bool isAddition = hunkRangeInfo.IsAddition;

            //Assert
            isAddition.ShouldBe(false);
        }

        [Test]
        public void IsModification_DiffLinesStartsWithPlusSignAndWithMinus_ExpectedTrue()
        {
            //Arrange
            var hunkRangeInfo = new HunkRangeInfo(new HunkRange("-41,0"), new HunkRange("+42,20"), new List<string> { "+ ", "- " }.ToArray());

            //Act
            bool isModification = hunkRangeInfo.IsModification;

            //Assert
            isModification.ShouldBe(true);
        }
    }

    // ReSharper restore InconsistentNaming 
}