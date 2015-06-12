using System;

namespace ForgetMeNot.Common
{
    public static class SystemTime
    {
        private static DateTime _setTime = DateTime.MinValue;

        public static void Clear()
        {
            _setTime = DateTime.MinValue;
        }

        public static void Set(DateTime toSet)
        {
            _setTime = toSet;
        }

        public static DateTime Now()
        {
            if (_setTime == DateTime.MinValue)
                return DateTime.Now;
            return _setTime;
        }

		public static DateTime UtcNow()
		{
			return Now ().ToUniversalTime ();
		}

		public static DateTime FreezeTime()
		{
			Set (Now ());
			return Now ();
		}
    }
}
