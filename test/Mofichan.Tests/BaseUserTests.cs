using System.Collections.Generic;
using Mofichan.Backend;
using Mofichan.Core.Interfaces;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BaseUserTests
    {
        public static IEnumerable<object[]> EqualUserExamples
        {
            get
            {
                yield return new object[]
                {
                    new MockUser(id: "Tom", name: "foo"),
                    new MockUser(id: "Tom", name: "bar")
                };

                yield return new object[]
                {
                    new MockUser(id: "Dick", name: "bar"),
                    new MockUser(id: "Dick", name: "baz123")
                };

                yield return new object[]
                {
                    new MockUser(id: "Harry", name: "abc", userType: UserType.Adminstrator),
                    new MockUser(id: "Harry", name: "123")
                };
            }
        }

        public static IEnumerable<object[]> UnequalUserExamples
        {
            get
            {
                yield return new object[]
                {
                    new MockUser(id: "Tom", name: "foo"),
                    new MockUser(id: "Tommy", name: "foo")
                };

                yield return new object[]
                {
                    new MockUser(id: "Amy", name: "Amy"),
                    new MockUser(id: "Amy2", name: "Amy")
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualUserExamples))]
        public void Users_Should_Be_Considered_Equal_If_They_Have_The_Same_Id(IUser userA, IUser userB)
        {
            userA.Equals(userB).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(UnequalUserExamples))]
        public void Users_Should_Not_Be_Considered_Equal_If_They_Have_Different_Ids(IUser userA, IUser userB)
        {
            userA.Equals(userB).ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(EqualUserExamples))]
        public void Users_That_Are_Equal_Should_Generate_The_Same_Hash_Code(IUser userA, IUser userB)
        {
            userA.GetHashCode().ShouldBe(userB.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(EqualUserExamples))]
        public void Room_Occupants_Should_Be_Equal_If_Underlying_Users_Are_Equal(IUser userA, IUser userB)
        {
            var roomOccupantA = new RoomOccupant(userA, Mock.Of<IRoom>());
            var roomOccupantB = new RoomOccupant(userB, Mock.Of<IRoom>());

            roomOccupantA.Equals(roomOccupantB).ShouldBeTrue();
            roomOccupantA.GetHashCode().ShouldBe(roomOccupantB.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(UnequalUserExamples))]
        public void Room_Occupants_Should_Not_Be_Equal_If_Underlying_Users_Are_Not_Equal(IUser userA, IUser userB)
        {
            var roomOccupantA = new RoomOccupant(userA, Mock.Of<IRoom>());
            var roomOccupantB = new RoomOccupant(userB, Mock.Of<IRoom>());

            roomOccupantA.Equals(roomOccupantB).ShouldBeFalse();
        }
    }
}
