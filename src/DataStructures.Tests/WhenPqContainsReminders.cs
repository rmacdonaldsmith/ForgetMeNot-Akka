using System;
using NUnit.Framework;
using ForgetMeNot.Messages;
using System.Linq;

namespace ForgetMeNot.DataStructures.Tests
{
	[TestFixture]
	public class WhenPqContainsReminders
	{
		private MinPriorityQueue<ReminderMessage.ISchedulable> _pq = new MinPriorityQueue<ReminderMessage.ISchedulable>((a,b) => a.DueAt > b.DueAt);
		private DateTime _now;

		[TestFixtureSetUp]
		public void InitializeQ()
		{
			_now = DateTime.Now;
			_pq.Insert (new TestReminder(_now.AddSeconds(-10), Guid.NewGuid()));
			_pq.Insert (new TestReminder(_now.AddSeconds(10), Guid.NewGuid()));
			_pq.Insert (new TestReminder(_now.AddSeconds(20), Guid.NewGuid()));
			_pq.Insert (new TestReminder(_now.AddSeconds(-20), Guid.NewGuid()));
			_pq.Insert (new TestReminder(_now.AddSeconds(-30), Guid.NewGuid()));
			_pq.Insert (new TestReminder(_now.AddSeconds(40), Guid.NewGuid()));
		}

		[Test]
		public void Should_list_items_in_order()
		{
			var expected = new DateTime[]{ _now.AddSeconds(-30), _now.AddSeconds(-20), _now.AddSeconds(-10), _now.AddSeconds(10), _now.AddSeconds(20), _now.AddSeconds(40) };
			var actual = _pq.Select (i => i.DueAt);
			CollectionAssert.AreEqual (expected, actual);
		}

		[Test]
		public void Should_return_the_min_item()
		{
			Assert.AreEqual(_now.AddSeconds(-30), _pq.RemoveMin().DueAt);
			Assert.AreEqual(5, _pq.Size);
			Assert.AreEqual(_now.AddSeconds(-20), _pq.RemoveMin().DueAt);
			Assert.AreEqual(4, _pq.Size);
			Assert.AreEqual(_now.AddSeconds(-10), _pq.RemoveMin().DueAt);
			Assert.AreEqual(3, _pq.Size);
			Assert.AreEqual(_now.AddSeconds(10), _pq.RemoveMin().DueAt);
			Assert.AreEqual(2, _pq.Size);
			Assert.AreEqual(_now.AddSeconds(20), _pq.RemoveMin().DueAt);
			Assert.AreEqual(1, _pq.Size);
		}
	}

	public class TestReminder : ReminderMessage.ISchedulable
	{
		public DateTime DueAt { get; set; }
		public Guid ReminderId { get; set; }	

		public TestReminder (DateTime dueAt, Guid reminderId)
		{
			DueAt = dueAt;
			ReminderId = reminderId;
		}
	}
}

