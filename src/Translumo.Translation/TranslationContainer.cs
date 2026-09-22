using System;
using Translumo.Translation.Configuration;

namespace Translumo.Translation
{
    public abstract class TranslationContainer
    {
        public int FailUsesCounter { get; protected set; }

        public DateTime? BlockedUntilUtc { get; protected set; }

        public bool IsBlocked => BlockedUntilUtc != null && BlockedUntilUtc.Value > DateTime.UtcNow;

        public DateTime LastTimeUsedUtc { get; protected set; }

        public bool IsPrimary { get; }

        protected readonly object Obj = new object();

        private const int FAIL_USE_LIMITATION = 3;
        private const int BLOCK_TIME_MIN = 15;

        protected TranslationContainer(bool isPrimary)
        {
            this.IsPrimary = isPrimary;
            this.LastTimeUsedUtc = DateTime.MinValue;
        }

        public virtual void MarkContainerIsUsed(bool isSuccessful)
        {
            lock (Obj)
            {
                LastTimeUsedUtc = DateTime.UtcNow;
                if (isSuccessful)
                {
                    Restore();
                }
                else
                {
                    FailUsesCounter++;
                    Reset();
                    if (FailUsesCounter >= FAIL_USE_LIMITATION)
                    {
                        Block();
                    }
                }
            }
        }

        public virtual void Restore()
        {
            BlockedUntilUtc = null;
            FailUsesCounter = 0;
        }

        public virtual void Block()
        {
            BlockedUntilUtc = DateTime.UtcNow.AddMinutes(BLOCK_TIME_MIN * (BlockedUntilUtc.HasValue ? 2 : 1));
        }

        public virtual void Reset()
        {
        }
    }
}
