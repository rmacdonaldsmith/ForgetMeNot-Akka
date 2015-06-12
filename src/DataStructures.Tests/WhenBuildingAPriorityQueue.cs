using ForgetMeNot.DataStructures;
using NUnit.Framework;

namespace ReminderService.DataStructures.Tests
{
    [TestFixture]
    public class WhenBuildingAPriorityQueue
    {
        private PriorityQueue<int> _pq;

        [TestFixtureSetUp]
        public void Initial()
        {
            _pq = new PriorityQueue<int>(1);
            _pq.Insert(1);
            _pq.Insert(2);
            _pq.Insert(3);
            _pq.Insert(4);
            _pq.Insert(5);
            _pq.Insert(6);
            _pq.Insert(7);
            _pq.Insert(8);
            _pq.Insert(9);
            _pq.Insert(10);
            _pq.Insert(11);
            _pq.Insert(12);
            _pq.Insert(13);
        }

        [Test]
        public void Should_return_the_highest_key()
        {
            Assert.AreEqual(13, _pq.Max());
        }

        [Test]
        public void Should_maintain_a_count_of_elements()
        {
            Assert.AreEqual(13, _pq.Size);
        }

        [Test]
        public void Should_keep_elements_in_order()
        {
            var expected = new int[] {13,12,11,10,9,8,7,6,5,4,3,2,1};
            CollectionAssert.AreEqual(expected, _pq);
        }
    }
}
