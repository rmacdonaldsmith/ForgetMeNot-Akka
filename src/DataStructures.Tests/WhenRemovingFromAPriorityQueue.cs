using ForgetMeNot.DataStructures;
using NUnit.Framework;

namespace ReminderService.DataStructures.Tests
{
    [TestFixture]
    public class WhenRemovingFromAPriorityQueue
    {
        private PriorityQueue<string> _pq;

        [TestFixtureSetUp]
        public void Initial()
        {
            _pq = new PriorityQueue<string>(1);
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

            var expected = new string[] {"y","u","u","t","r","r","q","p","o","i","i","e","e"};
            CollectionAssert.AreEqual(expected, _pq);
        }

        [Test]
        public void Should_return_the_max_item()
        {
            Assert.AreEqual("y", _pq.RemoveMax());
            Assert.AreEqual(12, _pq.Size);
        }
    }
}
