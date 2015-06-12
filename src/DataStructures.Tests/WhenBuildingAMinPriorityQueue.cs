using ForgetMeNot.DataStructures;
using NUnit.Framework;

namespace ReminderService.DataStructures.Tests
{
    [TestFixture]
	public class WhenBuildingAMinPriorityQueue
    {
		private MinPriorityQueue<int> _pq = new MinPriorityQueue<int> ((x, y) => x > y);

        [TestFixtureSetUp]
        public void Initial()
        {
			_pq.Insert(9);
			_pq.Insert(10);
			_pq.Insert(11);
			_pq.Insert(12);
            _pq.Insert(1);
            _pq.Insert(2);
            _pq.Insert(3);
            _pq.Insert(4);
            _pq.Insert(5);
            _pq.Insert(6);
            _pq.Insert(7);
            _pq.Insert(8);
            _pq.Insert(13);
        }

        [Test]
		public void Should_peak_the_smallest_key()
        {
			Assert.AreEqual(1, _pq.Min());
        }

        [Test]
        public void Should_maintain_a_count_of_elements()
        {
            Assert.AreEqual(13, _pq.Size);
        }

        [Test]
        public void Should_keep_elements_in_order()
        {
			var expected = new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13};
            CollectionAssert.AreEqual(expected, _pq);
        }
    }
}
