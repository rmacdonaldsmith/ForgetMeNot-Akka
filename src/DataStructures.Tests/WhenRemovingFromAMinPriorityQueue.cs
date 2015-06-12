using ForgetMeNot.DataStructures;
using NUnit.Framework;

namespace ReminderService.DataStructures.Tests
{
    [TestFixture]
	public class WhenRemovingFromAMinPriorityQueue
    {
		private MinPriorityQueue<string> _pq = new MinPriorityQueue<string>((x,y) => x.CompareTo(y) > 0);

        [TestFixtureSetUp]
        public void InitializePriorityQueue()
        {
            _pq.Insert("p");
            _pq.Insert("r");
            _pq.Insert("i");
            _pq.Insert("o");
            _pq.Insert("r");
            _pq.Insert("i");
            _pq.Insert("t");
            _pq.Insert("y");
            _pq.Insert("q");
            _pq.Insert("u");
            _pq.Insert("e");
            _pq.Insert("u");
            _pq.Insert("e");
        }

		[Test]
		public void Should_have_items_in_the_correct_order()
		{
			var expected = new string[] {"e","e","i","i","o","p","q","r","r","t","u","u","y"};
			CollectionAssert.AreEqual(expected, _pq);
		}

        [Test]
        public void Should_return_the_min_item()
        {
			Assert.AreEqual("e", _pq.RemoveMin());
            Assert.AreEqual(12, _pq.Size);
			Assert.AreEqual("e", _pq.RemoveMin());
			Assert.AreEqual(11, _pq.Size);
			Assert.AreEqual("i", _pq.RemoveMin());
			Assert.AreEqual(10, _pq.Size);
			Assert.AreEqual("i", _pq.RemoveMin());
			Assert.AreEqual(9, _pq.Size);
			Assert.AreEqual("o", _pq.RemoveMin());
			Assert.AreEqual(8, _pq.Size);
        }
    }
}
