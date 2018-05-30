using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNE.Scheduler.Extension
{
  public static  class TimeHelper
    {
      public static Time GetTime(this DateTime dt)
      {
          return new Time(dt .Hour ,dt .Minute ,dt .Second);
      }
    }
}
