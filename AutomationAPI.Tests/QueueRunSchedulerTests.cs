using AutomationAPI.Repositories.Models;
using AutomationAPI.Repositories.TestRunner;
using NUnit.Framework;

namespace AutomationAPI.Tests
{
    [TestFixture]
    public class QueueRunSchedulerTests
    {
        private static PendingExecutionQueue Item(int? loginUserId, int? environmentId = 21) => new()
        {
            QueueId = Guid.NewGuid(),
            LoginUserId = loginUserId,
            EnvironmentId = environmentId
        };

        [Test]
        public void SingleSlot_RunsStrictlyOneAtATime()
        {
            var scheduler = new QueueRunScheduler(1);
            var first = Item(1);
            var second = Item(2);

            Assert.That(scheduler.TryReserve(first), Is.True);
            Assert.That(scheduler.TryReserve(second), Is.False);

            scheduler.Release(first.QueueId);
            Assert.That(scheduler.TryReserve(second), Is.True);
        }

        [Test]
        public void ZeroOrNegativeLimit_IsTreatedAsOne()
        {
            Assert.That(new QueueRunScheduler(0).MaxParallelRuns, Is.EqualTo(1));
            Assert.That(new QueueRunScheduler(-3).MaxParallelRuns, Is.EqualTo(1));
        }

        [Test]
        public void DifferentLoginUsers_RunInParallelUpToTheLimit()
        {
            var scheduler = new QueueRunScheduler(3);

            Assert.That(scheduler.TryReserve(Item(1)), Is.True);
            Assert.That(scheduler.TryReserve(Item(2)), Is.True);
            Assert.That(scheduler.TryReserve(Item(3)), Is.True);
            Assert.That(scheduler.TryReserve(Item(4)), Is.False);
            Assert.That(scheduler.RunningCount, Is.EqualTo(3));
            Assert.That(scheduler.HasFreeSlot, Is.False);
        }

        [Test]
        public void SameLoginUser_NeverOverlaps()
        {
            var scheduler = new QueueRunScheduler(5);
            var first = Item(7);
            var second = Item(7);

            Assert.That(scheduler.TryReserve(first), Is.True);
            Assert.That(scheduler.TryReserve(second), Is.False);

            scheduler.Release(first.QueueId);
            Assert.That(scheduler.TryReserve(second), Is.True);
        }

        [Test]
        public void DifferentLoginUserEntriesForTheSameAccount_NeverOverlap()
        {
            var scheduler = new QueueRunScheduler(5);
            var techAdmin = new PendingExecutionQueue { QueueId = Guid.NewGuid(), LoginUserId = 4, LoginUserName = "autotechadmin", EnvironmentId = 21 };
            var stateAdmin = new PendingExecutionQueue { QueueId = Guid.NewGuid(), LoginUserId = 7, LoginUserName = "AutoTechAdmin ", EnvironmentId = 21 };
            var test1 = new PendingExecutionQueue { QueueId = Guid.NewGuid(), LoginUserId = 6, LoginUserName = "test1", EnvironmentId = 21 };

            Assert.That(scheduler.TryReserve(techAdmin), Is.True);
            Assert.That(scheduler.TryReserve(stateAdmin), Is.False);
            Assert.That(scheduler.TryReserve(test1), Is.True);
        }

        [Test]
        public void SameAccountInDifferentEnvironments_CanRunTogether()
        {
            var scheduler = new QueueRunScheduler(5);

            Assert.That(scheduler.TryReserve(new PendingExecutionQueue { QueueId = Guid.NewGuid(), LoginUserId = 4, LoginUserName = "autotechadmin", EnvironmentId = 21 }), Is.True);
            Assert.That(scheduler.TryReserve(new PendingExecutionQueue { QueueId = Guid.NewGuid(), LoginUserId = 12, LoginUserName = "autotechadmin", EnvironmentId = 22 }), Is.True);
        }

        [Test]
        public void NoLoginUser_SharesTheEnvironmentDefaultAccount()
        {
            var scheduler = new QueueRunScheduler(5);

            Assert.That(scheduler.TryReserve(Item(null, 21)), Is.True);
            Assert.That(scheduler.TryReserve(Item(null, 21)), Is.False);
            Assert.That(scheduler.TryReserve(Item(null, 22)), Is.True);
        }

        [Test]
        public void SameQueueItem_IsNeverReservedTwice()
        {
            var scheduler = new QueueRunScheduler(5);
            var item = Item(1);

            Assert.That(scheduler.TryReserve(item), Is.True);
            Assert.That(scheduler.IsRunning(item.QueueId), Is.True);
            Assert.That(scheduler.TryReserve(item), Is.False);
            Assert.That(scheduler.RunningCount, Is.EqualTo(1));
        }

        [Test]
        public void Release_OfUnknownItem_IsHarmless()
        {
            var scheduler = new QueueRunScheduler(2);
            var item = Item(1);
            scheduler.TryReserve(item);

            scheduler.Release(Guid.NewGuid());

            Assert.That(scheduler.RunningCount, Is.EqualTo(1));
            Assert.That(scheduler.TryReserve(Item(1)), Is.False);
        }

        [Test]
        public async Task ConcurrentReservations_NeverExceedTheLimitOrDoubleBookAUser()
        {
            var scheduler = new QueueRunScheduler(4);
            var items = Enumerable.Range(0, 200).Select(i => Item(i % 10)).ToList();

            var reserved = await Task.WhenAll(items.Select(i => Task.Run(() => scheduler.TryReserve(i) ? i : null)));
            var winners = reserved.Where(i => i != null).ToList();

            Assert.That(winners, Has.Count.EqualTo(4));
            Assert.That(winners.Select(w => w!.LoginUserId).Distinct().Count(), Is.EqualTo(4));
            Assert.That(scheduler.RunningCount, Is.EqualTo(4));
        }
    }
}
